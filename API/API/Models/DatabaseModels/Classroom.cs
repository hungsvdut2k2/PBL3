﻿using System.Text.Json.Serialization;

namespace API.Models.DatabaseModels
{
    public class Classroom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public List<UserInformation> UserInformation { get; set; }
        [JsonIgnore]
        public Faculty Faculty { get; set; }
        [JsonIgnore]
        public int FacultyId { get; set; }
    }
}
