﻿using DAL.Models;
using Microsoft.AspNetCore.Identity;
using Services.Interfaces;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DAL.Contexts;

namespace Services.Implementations
{
    public class NotificationManager : INotificationManager
    {
        private readonly AppDbContext _appDb;
        private readonly IRepositoryFactory _repos;
        private readonly IEmailSender _emailSender;

        public NotificationManager(AppDbContext appDbContext, IRepositoryFactory factory, IEmailSender emailSender)
        {
            _appDb = appDbContext;
            _repos = factory;
            _emailSender = emailSender;
        }

        public async Task OnNewContent(Content content)
        {
            if (content.Unit == null)
            {
                throw new Exception("Send notifications operation failed.", new Exception("Null unit object in uploaded content."));
            }

            string subject = "New Content Uploaded.";
            string message = $"Your lecturer has uploaded a {content.Type.ToString()} to '{content.Unit.Name}' " +
                             $"learning materials with the title <b>{content.Title}</b>.";
            
            // get all students for this unit
            IList<Student> students = _repos.StudentUnits
                                            .ListWith("Student")
                                            .Where(x => x.UnitId == content.Unit.Id)
                                            .Select(x => x.Student)
                                            .ToList();

            IList<AppUser> accounts = GetStudentAccounts(students);

            // create notifications for all students
            SendNotifications(message, students);
            // post in discussion board
            PostContentToBoard(content);
            _repos.Commit();

            // send emails
            await SendEmails(subject, message, accounts);
        }

        public async Task OnNewExam(Exam exam)
        {
            if (exam.Unit == null)
            {
                throw new Exception("Send notifications operation failed.", new Exception("Null unit object in exam object."));
            }

            string subject = "EXAM";
            string message = $"Your lecturer has added an exam for '{exam.Unit.Name}' .<br/><br/>" +
                             $"The exam is scheduled for <b>{exam.Start.ToString()}</b>." +
                             $"Prepare & revise widely using unit materials.";


            // get all students for this unit
            IList<Student> students = _repos.StudentUnits
                                            .ListWith("Student")
                                            .Where(x => x.UnitId == exam.Unit.Id)
                                            .Select(x => x.Student)
                                            .ToList();

            IList<AppUser> accounts = GetStudentAccounts(students);

            // create notifications for all students
            SendNotifications(message, students);
            // post in discussion board
            PostExamToBoard(exam, message);
            _repos.Commit();

            // send emails
            await SendEmails(subject, message, accounts);
        }

        public void OnNewUnit(Unit unit)
        {
            // create 
            var board = new DiscussionBoard()
            {
                Name = "Welcome Discussion Board",
                Unit = unit
            };

            _repos.DiscussionBoards.Create(board);
            _repos.Commit();
        }

        #region Private Section
        private IList<AppUser> GetStudentAccounts(IList<Student> students)
        {
            List<AppUser> accounts = new List<AppUser>();

            foreach (var item in students)
            {
                string s = item.AccountId.ToString();

                var user = _appDb.Users.FirstOrDefault(x => x.Id == s);

                if(user != null)
                {
                    accounts.Add(user);
                }
            }

            return accounts;
        }
        private void PostContentToBoard(Content content)
        {
            var post = new Post()
            {
                Message = $"Uploaded content: {content.Title}",
            };

            if(content.UploadedBy != null)
            {
                if(content.UploadedBy.Profile != null)
                {
                    post.By = content.UploadedBy.Profile;
                }
            }

            post.Medias = new List<Content>
            {
                content
            };
            post = _repos.Posts.Create(post);

            // get unit board
            var board = _repos.DiscussionBoards
                              .ListWith("Unit", "Posts")
                              .FirstOrDefault(x => x.UnitId == content.Unit.Id);

            if(board == null)
            {
                board = new DiscussionBoard()
                {
                    Name = "Default Board",
                    Unit = content.Unit
                };

                board = _repos.DiscussionBoards.Create(board);
            }

            _repos.Commit();
            if(board.Posts == null)
            {
                board.Posts = new List<Post>();
            }
            board.Posts.Add(post);
            board = _repos.DiscussionBoards.Update(board);
        }
        private void PostExamToBoard(Exam exam, string message)
        {
            var post = new Post()
            {
                Message = $"Exam: {exam.Code.ToString().Substring(0,6).ToUpper()}.<br/>" +
                          $"{message}",
                By = exam.Unit?.Lecturer?.Profile
            };
            post = _repos.Posts.Create(post);

            // get unit board
            var board = _repos.DiscussionBoards
                              .ListWith("Unit", "Posts")
                              .FirstOrDefault(x => x?.Unit.Id == exam.Unit.Id);

            if (board == null)
            {
                board = new DiscussionBoard()
                {
                    Name = "Default Board",
                    Unit = exam.Unit
                };

                board = _repos.DiscussionBoards.Create(board);
            }

            if(board.Posts == null)
            {
                board.Posts = new List<Post>();
            }

            board.Posts.Add(post);
        }
        private void SendNotifications(string message, IList<Student> students)
        {
            IList<int> accounts = new List<int>();

            foreach (var item in students)
            {
                accounts.Add(item.Id);
            }

            for (int i = 0; i < accounts.Count; i++)
            {
                var ntf = new Notification()
                {
                    AccountId = accounts[i],
                    Read = false,
                    Message = message
                };

                _repos.Notifications.Create(ntf);
            }
        }
        private async Task SendEmails(string subject, string message, IList<AppUser> accounts)
        {
            IList<string> emails = new List<string>();

            foreach (var item in accounts)
            {
                emails.Add(item.Email);
            }

            await _emailSender.SendEmailAsync(subject, message, emails.ToArray());
        }
        #endregion
    }
}
