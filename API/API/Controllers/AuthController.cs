﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.Models.DatabaseModels;
using API.Models.DtoModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;

namespace API.Controllers
{
    [EnableCors("Cau Khong")]
    [Route("api/account")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        public static IWebHostEnvironment _enviroment;

        public AuthController(ApplicationDbContext context, IConfiguration configuration, IWebHostEnvironment environment)
        {
            _context = context;
            _configuration = configuration;
            _enviroment = environment;
        }
        [HttpPost("register")]
        public async Task<ActionResult<LoginDto>> Register(RegisterDto request)
        {
            Classroom classroom = await _context.Classrooms.Where(w => w.Name == request.ClassName).FirstAsync();
            Faculty faculty = await _context.Faculty.Where(w => w.FacultyId == classroom.FacultyId).FirstAsync();
            List<Classroom> classInFaculty = _context.Classrooms.Where(w => w.FacultyId == faculty.FacultyId).ToList();
            var userInformation = new UserInformation
            {
                UserId = request.UserId,
                Name = request.Name,
                Dob = request.Dob,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Gender = request.Gender,
                ImageUrl = string.Empty,
                Classroom = classroom,
                CourseClassroomUserInformation = null
            };
            string Password = randomPassword();
            _context.UsersInformation.Add(userInformation);
            _context.SaveChanges();
            CreatePasswordHash(Convert.ToString(Password), out byte[] passwordHash, out byte[] passwordSalt);
            var user = new User
            {
                Username = request.UserId,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = request.Role,
                UserInformation = userInformation
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            LoginDto res = new LoginDto
            {
                Username = user.Username,
                Password = Password
            };
            return Ok(res);
        }

        private string randomPassword()
        {
            StringBuilder str_build = new StringBuilder();
            Random random = new Random();

            char letter;

            for (int i = 0; i < 10; i++)
            {
                double flt = random.NextDouble();
                int shift = Convert.ToInt32(Math.Floor(25 * flt));
                letter = Convert.ToChar(shift + 65);
                str_build.Append(letter);
            }

            return str_build.ToString();
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<ActionResult<User>> Get(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return BadRequest("Not Found");
            return Ok(user);
        }
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginDto request)
        {
            User user = await _context.Users.Where(w => w.Username == request.Username).FirstAsync();
            if (user == null)
            {
                return NotFound();
            }
            if (Verified(request.Password, user.PasswordHash, user.PasswordSalt) == true)
            {
                string token = CreateToken(user);
                return Ok(token);
            }
            return BadRequest("Wrong Password");
        }
        private bool Verified(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Name, user.Username),
                new Claim("role",user.Role),
                new Claim(JwtRegisteredClaimNames.NameId, user.UserInformationId)
            };
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: cred);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
        [HttpPut("reset-password")]
        public async Task<ActionResult<User>> ResetPassword(ResetPassowordDto request)
        {
            User user = await _context.Users.Where(w => w.Username == request.Username).FirstAsync();
            if (user == null)
            {
                return NotFound();
            }
            UserInformation userInformation = await _context.UsersInformation.Where(w => w.UserId == user.UserInformationId).FirstOrDefaultAsync();
            if (userInformation.PhoneNumber != request.PhoneNumber || userInformation.Email != request.Email)
            {
                return BadRequest("Uncertain Information");
            }
            CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            await _context.SaveChangesAsync();
            return Ok(user);
        }
        [Route("{userId}")]
        [HttpDelete]
        public async Task<ActionResult<User>> Delete(int userId)
        {
            User user = await _context.Users.FindAsync(userId);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        public class FileUpLoadAPI
        {
            public IFormFile files { get; set; }
        }
        [HttpPost("upload-file")]
        public async Task<ActionResult<List<LoginDto>>> uploadFile([FromForm] FileUpLoadAPI data)
        {
            //download file from client
            if (data.files.Length > 0)
            {
                if (!Directory.Exists(_enviroment.WebRootPath + "\\Download\\"))
                {
                    Directory.CreateDirectory(_enviroment.WebRootPath + "\\Download\\");
                }

                using (FileStream fileStream =
                       System.IO.File.Create(_enviroment.WebRootPath + "\\Download\\" + data.files.FileName))
                {
                    data.files.CopyTo(fileStream);
                    fileStream.Flush();
                }
                //work with excel file
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                FileInfo fileInfo = new FileInfo(_enviroment.WebRootPath + "\\Download\\" + data.files.FileName);
                ExcelPackage excelPackage = new ExcelPackage(fileInfo);
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.FirstOrDefault();
                List<ReturnedAccount> accounts = new List<ReturnedAccount>();
                int rows = worksheet.Dimension.Rows;
                for (int i = 2; i <= rows; i++)
                {
                    RegisterDto request = new RegisterDto
                    {
                        ClassName = worksheet.Cells[i, 1].Text,
                        UserId = worksheet.Cells[i, 2].Text,
                        Name = worksheet.Cells[i, 3].Text,
                        Gender = worksheet.Cells[i, 4].Text != "Nam",
                        Dob = worksheet.Cells[i, 5].Text,
                        Email = worksheet.Cells[i, 6].Text,
                        PhoneNumber = worksheet.Cells[i, 7].Text,
                        Role = "Student"
                    };
                    var result = await Register(request);
                    var castResult = (OkObjectResult)result.Result;
                    var finalResult = (LoginDto)castResult.Value;
                    var account = new ReturnedAccount
                    {
                        Name = request.Name,
                        ClassName = request.ClassName,
                        Account = finalResult
                    };
                    accounts.Add(account);
                }
                return Ok(accounts);
            }
            return BadRequest();
        }
    }
}
