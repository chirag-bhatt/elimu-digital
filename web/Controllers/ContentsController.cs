﻿using AutoMapper;
using Common.Models;
using Common.ViewModels;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace web.Controllers
{
    [Authorize]
    [Route("contents")]
    public class ContentsController : Controller
    {
        private readonly INotificationManager _notify;
        private readonly IRepositoryFactory _repos;
        private readonly IProgressTracker _progressTracker;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUploader _uploader;
        private readonly IMapper _mapper;

        public ContentsController(INotificationManager notificationManager,
                                  IRepositoryFactory factory, 
                                  IProgressTracker progressTracker,
                                  UserManager<AppUser> userManager, 
                                  IUploader uploader,
                                  IMapper mapper)
        {
            _notify = notificationManager;
            _repos = factory;
            _progressTracker = progressTracker;
            _userManager = userManager;
            _uploader = uploader;
            _mapper = mapper;
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            var contents = _repos.Contents
                                 .ListWith("Unit", "Likes", "Comments")
                                 .OrderByDescending(x => x.Timestamp)
                                 .ToList();

            return View(contents);
        }

        [HttpGet]
        [Route("add/{unitId}/{name}")]
        public IActionResult Add(int unitId, string name)
        {
            if(unitId < 1)
            {
                return BadRequest("Invalid unit id.");
            }

            var unit = _repos.Units.GetWith(unitId,"Course");
            
            if(unit == null)
            {
                return NotFound("Unit record with that Id does not exist.");
            }

            ViewBag.unit = unit;

            return View();
        }

        [HttpPost]
        [Route("add/unit/{unitId}")]
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> Add(int unitId)
        {
            ContentViewModel model = new ContentViewModel
            {
                Title = Request.Form["Title"],
                Description = Request.Form["Desc"]
            };

            if (unitId < 1)
            {
                return BadRequest("Invalid unit id.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // get unit
            var unit = _repos.Units.Get(unitId);

            if (unit == null)
            {
                return NotFound("Unit record with that Id does not exist.");
            }

            // get current user account
            var user = await _userManager.GetUserAsync(User);

            // get lecturer account
            var lec = _repos.Lecturers.Get(user.AccountId);

            if(lec == null)
            {
                return NotFound("Current logged in user does not have a valid lecturer account.");
            }

            // get content data
            var content = new Content()
            {
                Title = model.Title,
                Description = model.Description,
                Unit = unit,
                UploadedBy = lec
            };

            int contentType = int.Parse(Request.Form["ContentType"]);

            if(contentType == 1)
            {
                if (string.IsNullOrWhiteSpace(Request.Form["Url"]))
                {
                    return Redirect(Request.Headers["Referer"]);
                }
                else
                {
                    content.FileName = "Uploaded via Url";
                    content.Type = (FormatType)int.Parse(Request.Form["UrlFileType"]);
                    content.Url = Request.Form["Url"];
                }
            }
            else if(contentType == 2)
            {
                // try upload attached file
                if (Request.Form.Files.Count < 1)
                {
                    return BadRequest("No file attached to the upload request. Contents must point to valid uploaded files.");
                }

                IFile file = new FormFile(Request.Form.Files[0]);

                content.FileName = file.FileName;
                content.Type = file.Format;

                try
                {
                    content.Url = await _uploader.Upload(file);
                }
                catch (Exception ex)
                {
                    return this.Error(HttpStatusCode.InternalServerError, "Uploading file failed! Please try again.");
                }
            }

            try
            {
                content = _repos.Contents.Create(content);
                _repos.Commit();

                await _notify.OnNewContent(content);

                return RedirectPermanent($"/contents/{content.Id}/{Services.Extensions.GenerateSlug(content.Title)}");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        [Route("edit/{id}/{title}")]
        [Authorize(Roles = "Lecturer, Admin")]
        public IActionResult Edit(int id, string title)
        {
            if (id < 1)
            {
                return BadRequest("Invalid content id.");
            }

            var content = _repos.Contents.Get(id);

            if (content == null)
            {
                return NotFound("Content record with that Id does not exist.");
            }

            ContentViewModel model = _mapper.Map<ContentViewModel>(content);

            return View(model);
        }


        [HttpGet]
        [Route("{id}/{title}")]
        public async Task<IActionResult> Details(int id, string title)
        {
            if(id < 1)
            {
                return BadRequest("Invalid content Id.");
            }

            var content = _repos.Contents
                                .GetWith(id, 
                                "UploadedBy", 
                                "UploadedBy.Profile",
                                "Unit", 
                                "Likes", 
                                "Comments",
                                "Comments.By");

            if(content == null)
            {
                return NotFound("Content record with that id does not exist.");
            }

            AppUser user = await _userManager.GetUserAsync(User);

            if(user.AccountType == AccountType.Student)
            {
                ViewBag.progress = _progressTracker.GetProgress(content.Id, user.AccountId);
            }
            else if(user.AccountType == AccountType.Lecturer)
            {
                ViewBag.studentsProgress = _progressTracker.TrackProgress(content.Id);
            }

            return View(content);
        }
    }
}
