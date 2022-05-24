﻿using API.Data;
using API.Models.DatabaseModels;
using API.Models.DtoModels;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [EnableCors("Cau Khong")]
    [Route("api/faculty")]
    [ApiController]
    public class FacultyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FacultyController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Faculty>>> Get()
        {
            return _context.Faculty.ToList();
        }

        [Route("classes")]
        [HttpGet]
        public async Task<ActionResult<List<ReturnedFacultyDto>>> GetClassroomByFacultyId()
        {
            List<Faculty> faculties = _context.Faculty.ToList();
            List<ReturnedFacultyDto> facultiesDto = new List<ReturnedFacultyDto>();
            foreach (var faculty in faculties)
            {
                var classList = _context.Classrooms.Where(w => w.FacultyId == faculty.Id).ToList();
                ReturnedFacultyDto returnedFaculty = new ReturnedFacultyDto
                {
                    facultyId = faculty.Id,
                    facultyName = faculty.Name,
                    Classes = classList
                };
                facultiesDto.Add(returnedFaculty);
            }

            return Ok(facultiesDto);
        }
    }
}

