﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace API.Models.DatabaseModels
{
    public class Course
    {
        [Key]
        public string CourseId { get; set; }
        public string Name { get; set; }
        public double Credits { get; set; }
        public string? requiredCourseId { get; set; }
        public bool? isAvailable { get; set; }
    }
}
