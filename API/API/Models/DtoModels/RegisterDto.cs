﻿namespace API.Models.DtoModels
{
    public class RegisterDto
    {
        public string Name { get; set; }
        public bool Gender { get; set; }
        public string Dob { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public string ClassroomId  { get; set; }
    }
}
