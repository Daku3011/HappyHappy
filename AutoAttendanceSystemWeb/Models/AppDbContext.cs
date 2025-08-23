using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoAttendanceSystemWeb.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Attendance> Attendances => Set<Attendance>();
        public DbSet<FaceSample> FaceSamples => Set<FaceSample>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Student>().HasIndex(s => s.StudentName);
            b.Entity<Attendance>()
             .HasIndex(a => new { a.StudentId, a.Timestamp }); // helps de-dup
        }
    }
}
