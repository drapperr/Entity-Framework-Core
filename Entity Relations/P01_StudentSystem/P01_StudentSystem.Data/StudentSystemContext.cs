using Microsoft.EntityFrameworkCore;
using P01_StudentSystem.Data.Models;
using System;

namespace P01_StudentSystem.Data
{
    public class StudentSystemContext : DbContext
    {
        public StudentSystemContext(DbContextOptions options) : base(options)
        {
        }

        protected StudentSystemContext()
        {
        }

        public DbSet<Course> MyProperty { get; set; }
    }
}
