﻿using DAL.Models;
using DAL.Models.Fees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Contexts
{
    public class LePadContext : DbContext
    {
        public LePadContext(DbContextOptions<LePadContext> options)
            :base(options) { }


        
        // Database Tables/DbSets
        public virtual DbSet<Answer> Answers { get; set; }
        public virtual DbSet<Class> Classes { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<Content> Contents { get; set; }
        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<Exam> Exams { get; set; }
        public virtual DbSet<Lecturer> Lecturers { get; set; }
        public virtual DbSet<Like> Likes { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<Profile> Profiles { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<School> Schools { get; set; }
        public virtual DbSet<Score> Scores { get; set; }
        public virtual DbSet<Student> Students { get; set; }
        public virtual DbSet<Unit> Units { get; set; }
        public virtual DbSet<Admin> Administrators { get; set; }
        public virtual DbSet<StudentUnit> StudentUnits { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<DiscussionBoard> DiscussionBoards { get; set; }
        public virtual DbSet<CourseworkProgress> CourseworkProgress { get; set; }
        public virtual DbSet<StudentCourse> StudentCourses { get; set; }
        public virtual DbSet<ExamSession> ExamSessions { get; set; }
        public virtual DbSet<FeeStructure> FeeStructures { get; set; }
        public virtual DbSet<FeePayment> FeePayments { get; set; }
        public virtual DbSet<BaseFeeStructure> BaseFeeStructure { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<StudentUnit>()
                   .HasKey(s => new { s.StudentId, s.UnitId });

            builder.Entity<StudentUnit>()
                   .HasOne(s => s.Student)
                   .WithMany(s => s.StudentUnits)
                   .HasForeignKey(s => s.StudentId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentUnit>()
                   .HasOne(s => s.Unit)
                   .WithMany(s => s.UnitStudents)
                   .HasForeignKey(s => s.UnitId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentCourse>()
                   .HasKey(c => new { c.StudentId, c.CourseId });

            builder.Entity<StudentCourse>()
                   .HasOne(c => c.Student)
                   .WithMany(c => c.StudentCourses)
                   .HasForeignKey(c => c.StudentId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentCourse>()
                   .HasOne(c => c.Course)
                   .WithMany(c => c.CourseStudents)
                   .HasForeignKey(c => c.CourseId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DiscussionBoard>()
                   .HasOne(d => d.Unit)
                   .WithMany(d => d.Boards)
                   .HasForeignKey(d => d.UnitId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Student>()
                   .HasMany(s => s.Payments)
                   .WithOne(x => x.Student)
                   .HasForeignKey(x => x.StudentId)
                   .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
