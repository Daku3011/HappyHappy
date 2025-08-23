using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoAttendanceSystemWeb.Models
{
    public class Attendance 
    {
        public int AttendanceId { get; set; }
        public int StudentId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        [ForeignKey(nameof(StudentId))] public Student? Student { get; set; }
        public string Source { get; set; } = "Webcam"; // optional
    }
}
