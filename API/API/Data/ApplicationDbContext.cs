﻿using API.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore;
namespace API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        //Fluent API methods.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CourseEducationalProgram>()
                .HasKey(bc => new { bc.CourseId, bc.EducationalProgramId });
            modelBuilder.Entity<CourseEducationalProgram>()
                .HasOne(bc => bc.Course)
                .WithMany(b => b.CourseEducationalProgram)
                .HasForeignKey(bc => bc.EducationalProgramId);
            modelBuilder.Entity<CourseEducationalProgram>()
                .HasOne(bc => bc.EducationalProgram)
                .WithMany(c => c.CourseEducationalProgram)
                .HasForeignKey(bc => bc.CourseId);
        }
        public DbSet<User> Users { get; set; }
        public DbSet<UserInformation> UsersInformation { get; set; }
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<EducationalProgram> EducationalProgram { get; set; }
        public DbSet<Faculty> Faculty{ get; set; }
        public DbSet<CourseClassroom> CoursesClassroom { get; set; }
        public DbSet<CourseEducationalProgram> CourseEducationalPrograms { get; set; }

    }
}
