﻿using DAL.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DAL.Models
{
    public class Exam : Base
    {
        public Exam()
        {
            this.Code = Guid.NewGuid();
        }

        public Guid Code { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime Start { get; set; }
        [Required]
        public DateTime End { get; set; }
        public DateTime Date { get; set; }

        public virtual Unit Unit { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<ExamSession> Sessions { get; set; }

        [NotMapped]
        public DateTime Moment
        {
            get
            {
                var x = this.Date;
                var y = this.Start.TimeOfDay;

                var z = new DateTime(x.Year, x.Month, x.Day, y.Hours, y.Minutes, y.Seconds, y.Milliseconds);
                return z;
            }
        }
    }
}
