using AutoAttendanceSystemWeb.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutoAttendanceSystemWeb.Data
{
    // ApplicationDbContext inherits from IdentityDbContext to include ASP.NET Core Identity tables
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Custom DbSets (your tables)
        public DbSet<Student> Student { get; set; }
        public DbSet<Attendance> Attendances { get; set; }

        // Optional: model configurations
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Example: configure table names if you want different ones
            builder.Entity<Student>().ToTable("Students");
            builder.Entity<Attendance>().ToTable("Attendances");
        }
    }
}
