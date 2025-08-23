using Microsoft.AspNetCore.Mvc;

namespace AutoAttendanceSystemWeb.Models
{
    public class FaceSample 
    {
        public int FaceSampleId { get; set; }
        public int StudentId { get; set; }
        public string ImagePath { get; set; } = ""; // /faces/{studentId}/img_001.jpg
    }
}
