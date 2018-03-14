﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DAL.Models;
using System.Reflection;
using Common.ViewModels;

namespace Services
{
    public class DataManager : IDataManager
    {
        private readonly IRepositoryFactory _repos;

        public DataManager(IRepositoryFactory factory)
        {
            _repos = factory;
        }

        public IEnumerable<Lecturer> MyLecturers(int studentId, int count)
        {
            // get all my units
            IEnumerable<Lecturer> lecturers = _repos.StudentUnits
                                              .ListWith("Unit",
                                                        "Unit.Lecturer",
                                                        "Unit.Lecturer.Profile",
                                                        "Unit.Lecturer.Likes")
                                              .Where(x => x.StudentId == studentId)
                                              .Select(x => x.Unit)
                                              .Select(x => x.Lecturer)
                                              .Take(count)
                                              .SkipWhile(x => x == null)
                                              .Distinct();
            return lecturers;
        }

        public IList<Student> MyStudents(int lecturerId, int count)
        {
            IList<Student> students = _repos.Lecturers
                                            .GetWith(lecturerId, "Units",
                                                      "Units.UnitStudents",
                                                      "Units.UnitStudents.Student",
                                                      "Units.UnitStudents.Student.Course",
                                                      "Units.UnitStudents.Student.Profile")
                                            .Units
                                            .SelectMany(x => x.UnitStudents)
                                            .Select(x => x.Student)
                                            .Take(count)
                                            .SkipWhile(x => x == null)
                                            .Distinct()
                                            .ToList();
            return students;
        }

        public IEnumerable<Class> MyClasses<T>(int id, int count) where T : class
        {
            IEnumerable<Class> classes = new List<Class>();

            if(typeof(T) == typeof(Student))
            {
                classes = _repos.Students
                                .GetWith(id,
                                        "StudentUnits",
                                        "StudentUnits.Unit",
                                        "StudentUnits.Unit.Class",
                                        "StudentUnits.Unit.Class.Units",
                                        "StudentUnits.Unit.Class.Likes")
                                .StudentUnits
                                .Select(x => x.Unit)
                                .Select(x => x.Class)
                                .SkipWhile(x => x == null);
            }
            else if(typeof(T) == typeof(Lecturer))
            {
                classes = _repos.Lecturers
                                .GetWith(id,
                                        "Units",
                                        "Units.Class",
                                        "Units.Class.Units",
                                        "Units.Class.Likes")
                                .Units
                                .Select(x => x.Class)
                                .SkipWhile(x => x == null);
            }

            return classes.Take(count);
        }

        public IEnumerable<Unit> MyUnits<T>(int id, int count) where T : class
        {
            IEnumerable<Unit> units = new List<Unit>();

            if (typeof(T) == typeof(Student))
            {
                var std = _repos.Students
                              .GetWith(id,
                                       "Course",
                                       "Course.Units",
                                       "Course.Units.Lecturer",
                                       "Course.Units.Lecturer.Profile",
                                       "Course.Units.Class",
                                       "Course.Units.UnitStudents",
                                       "Course.Units.Likes");
                
                if(std.Course != null)
                {
                    units = std.Course.Units;
                }
            }
            else if(typeof(T) == typeof(Lecturer))
            {
                var lec = _repos.Lecturers
                              .GetWith(id,
                                       "Units",
                                       "Units.Lecturer",
                                       "Units.Lecturer.Profile",
                                       "Units.UnitStudents",
                                       "Units.Likes");

                if(lec.Units != null)
                {
                    units = lec.Units.SkipWhile(x => x == null);
                }
            }

            return units.Take(count);
        }

        public SummaryViewModel GetSummary()
        {
            // create summary view model
            var summary = new SummaryViewModel
            {
                Total_Classes = _repos.Classes.List.Count(),
                Students_Total = _repos.Students.List.Count(),
                Lec_Total = _repos.Lecturers.List.Count(),
                Courses_Total = _repos.Courses.List.Count(),
                Lec_NoProfile = _repos.Lecturers.ListWith("Profile")
                                                .Count(x => x.Profile == null),
                Students_Enrolled = _repos.Students.ListWith("Course")
                                                   .Count(x => x.Course != null),
                Units_NoClass = _repos.Units.ListWith("Class")
                                            .Count(x => x.Class == null)
            };

            return summary;
        }

        public IEnumerable<Student> MyClassMates(int id)
        {
            IEnumerable<Student> students = new List<Student>();

            students = _repos.Students
                             .GetWith(id,
                                    "StudentUnits",
                                    "StudentUnits.Student",
                                    "StudentUnits.Student.Profile",
                                    "StudentUnits.Student.Course")
                             .StudentUnits
                             .Select(x => x.Student)
                             .SkipWhile(x => x == null)
                             .Distinct();

            return students;
        }
    }
}
