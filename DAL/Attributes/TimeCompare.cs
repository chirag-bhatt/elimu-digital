﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace DAL.Attributes
{
    public class TimeCompare : ValidationAttribute
    {
        private readonly Comparison _comp;
        private readonly string _otherProp;
        private readonly int _duration;
        private readonly DurationSpec _durationSpec;

        /// <summary>
        /// Validation attribute that compares two timestamps using a comparison specification
        /// to ensure the correct timestamps are used.
        /// </summary>
        /// <param name="comparison">Specification to use in comparing with the other timestamp.
        /// </param>
        /// <param name="otherProperty">The Name of the other property to compare with. Use
        /// the exact PropertyName or DisplayName.
        /// </param>
        /// <param name="duration">What is the maximum allowable difference between the two
        /// timestamps;
        /// </param>
        /// <param name="durationSpec">Specification for the <paramref name="duration"/> value
        /// specified. It can either be: Days, Hours, Minutes or Seconds.
        /// </param>
        public TimeCompare(Comparison comparison=Comparison.GreaterThan,
                           string otherProperty=null,
                           DurationSpec durationSpec = DurationSpec.Hours,
                           int duration=1)
        {
            _comp = comparison;
            _otherProp = otherProperty;
            _duration = duration;
            _durationSpec = durationSpec;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var type = (Type)validationContext.ObjectInstance;
            object propValue = new object();

            if (!string.IsNullOrWhiteSpace(_otherProp))
            {
                propValue = type.GetProperty(_otherProp).GetValue(validationContext.ObjectInstance);
            }

            DateTime endTime = Convert.ToDateTime(value);
            DateTime startTime = Convert.ToDateTime(propValue);
            
            TimeSpan diff = endTime.Subtract(startTime);

            return CheckDurationSpec(diff);
        }

        private ValidationResult CheckDurationSpec(TimeSpan timeSpan)
        {
            switch (_durationSpec)
            {
                case DurationSpec.Days:
                    if(_comp == Comparison.GreaterThan)
                    {
                        if (timeSpan.Days >= _duration)
                            return ValidationResult.Success;
                        else
                            return new ValidationResult($"EndTime should be greater than StartTime by at-least {_duration} day(s).");
                    }
                    else if(_comp == Comparison.LessThan)
                    {
                        if (timeSpan.Days < _duration)
                            return ValidationResult.Success;
                        else
                            return new ValidationResult($"EndTime should be less than StartTime by at-least {_duration} day(s).");
                    }
                    else
                        return null;
                case DurationSpec.Hours:
                    if (_comp == Comparison.GreaterThan)
                    {
                        if (timeSpan.Hours >= _duration)
                            return ValidationResult.Success;
                        else
                            return new ValidationResult($"EndTime should be greater than StartTime by at-least {_duration} hour(s).");
                    }
                    else if (_comp == Comparison.LessThan)
                    {
                        if (timeSpan.Hours < _duration)
                            return ValidationResult.Success;
                        else
                            return new ValidationResult($"EndTime should be less than StartTime by at-least {_duration} hour(s).");
                    }
                    else
                        return null;
                case DurationSpec.Minutes:
                    if (_comp == Comparison.GreaterThan)
                    {
                        if (timeSpan.Minutes >= _duration)
                            return ValidationResult.Success;
                        else
                            return new ValidationResult($"EndTime should be greater than StartTime by at-least {_duration} minute(s).");
                    }
                    else if (_comp == Comparison.LessThan)
                    {
                        if (timeSpan.Minutes < _duration)
                            return ValidationResult.Success;
                        else
                            return new ValidationResult($"EndTime should be less than StartTime by at-least {_duration} minute(s).");
                    }
                    else
                        return null;
                case DurationSpec.Seconds:
                    if (_comp == Comparison.GreaterThan)
                    {
                        if (timeSpan.Seconds >= _duration)
                            return ValidationResult.Success;
                        else
                            return new ValidationResult($"EndTime should be greater than StartTime by at-least {_duration} second(s).");
                    }
                    else if (_comp == Comparison.LessThan)
                    {
                        if (timeSpan.Seconds < _duration)
                            return ValidationResult.Success;
                        else
                            return new ValidationResult($"EndTime should be less than StartTime by at-least {_duration} second(s).");
                    }
                    else
                        return null;
                default:
                    return null;
            }
        }
    }
}
