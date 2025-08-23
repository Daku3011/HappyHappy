using AutoAttendanceSystemWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenCvSharp;

[Route("recognize")]
public class RecognizeController : Controller
{
    private readonly FaceDetectionService _detector;
    private readonly FaceRecognizerService _recognizer;
    private readonly AppDbContext _db;

    public RecognizeController(FaceDetectionService d, FaceRecognizerService r, AppDbContext db)
    { _detector = d; _recognizer = r; _db = db; }

    // Page with live preview
    [HttpGet("")]
    public IActionResult Index() => View();

    // API – receive base64, predict, mark attendance (once per day)
    [HttpPost("scan")]
    public async Task<IActionResult> Scan([FromForm] string dataUrl)
    {
        var bytes = Convert.FromBase64String(dataUrl.Split(',')[1]);
        using var mat = Mat.FromImageData(bytes, ImreadModes.Color);

        var face = _detector.ExtractLargestFace(mat);
        if (face == null) return Ok(new { matched = false, message = "no face" });

        var (label, conf) = _recognizer.Predict(face);
        if (label == null) return Ok(new { matched = false, message = "unknown", conf });

        var student = await _db.Students.FirstOrDefaultAsync(s => s.StudentId == label && s.IsActive);
        if (student == null) return Ok(new { matched = false, message = "not active", conf });

        // De-dup: mark once per day per student
        var today = DateTime.Today;
        bool already = await _db.Attendances
            .AnyAsync(a => a.StudentId == student.StudentId && a.Timestamp >= today && a.Timestamp < today.AddDays(1));

        if (!already)
        {
            _db.Attendances.Add(new Attendance { StudentId = student.StudentId, Timestamp = DateTime.Now, Source = "Webcam" });
            await _db.SaveChangesAsync();
        }

        return Ok(new { matched = true, studentId = student.StudentId, name = student.StudentName, conf, already });
    }
}
