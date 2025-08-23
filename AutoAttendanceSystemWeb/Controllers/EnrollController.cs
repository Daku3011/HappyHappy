using AutoAttendanceSystemWeb.Models;
using Microsoft.AspNetCore.Mvc;
using OpenCvSharp;

[Route("enroll")]
public class EnrollController : Controller
{
    private readonly AppDbContext _db;
    private readonly FaceDetectionService _detector;
    private readonly FaceRecognizerService _recognizer;
    private readonly IWebHostEnvironment _env;

    public EnrollController(AppDbContext db, FaceDetectionService d, FaceRecognizerService r, IWebHostEnvironment env)
    {
        _db = db; _detector = d; _recognizer = r; _env = env;
    }

    [HttpGet("")]
    public IActionResult Index() => View();

    // Receive base64 frame, detect face, save cropped grayscale to /faces/{studentId}
    [HttpPost("capture")]
    public async Task<IActionResult> Capture([FromForm] int studentId, [FromForm] string dataUrl)
    {
        var student = await _db.Students.FindAsync(studentId);
        if (student == null) return BadRequest("student not found");

        var bytes = Convert.FromBase64String(dataUrl.Split(',')[1]);
        using var mat = Mat.FromImageData(bytes, ImreadModes.Color);
        var face = _detector.ExtractLargestFace(mat);
        if (face == null) return BadRequest("no face detected");
        using var gray = new Mat();
        Cv2.CvtColor(face, gray, ColorConversionCodes.BGR2GRAY);
        Cv2.Resize(gray, gray, new Size(200, 200));

        var dir = Path.Combine(_env.WebRootPath, "faces", studentId.ToString());
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, $"img_{DateTime.UtcNow.Ticks}.jpg");
        Cv2.ImWrite(file, gray);

        _db.FaceSamples.Add(new FaceSample { StudentId = studentId, ImagePath = $"/faces/{studentId}/{Path.GetFileName(file)}" });
        await _db.SaveChangesAsync();

        return Ok("saved");
    }

    // Retrain model from disk (call this after you capture ~10–15 images per student)
    [HttpPost("train")]
    public IActionResult Train()
    {
        _recognizer.RetrainFromDisk();
        return Ok("trained");
    }
}
