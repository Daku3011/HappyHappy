using OpenCvSharp;
using System.Drawing;

public class FaceDetectionService
{
    private readonly CascadeClassifier _cascade;

    public FaceDetectionService(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.WebRootPath, "models", "haarcascade_frontalface_default.xml");
        _cascade = new CascadeClassifier(path);
    }

    public Rect[] DetectFaces(Mat bgrFrame)
    {
        using var gray = new Mat();
        Cv2.CvtColor(bgrFrame, gray, ColorConversionCodes.BGR2GRAY);
        Cv2.EqualizeHist(gray, gray);
        return _cascade.DetectMultiScale(gray, 1.1, 5, HaarDetectionTypes.ScaleImage, new OpenCvSharp.Size(80, 80));
    }

    public Mat? ExtractLargestFace(Mat bgrFrame)
    {
        var faces = DetectFaces(bgrFrame);
        if (faces.Length == 0) return null;
        // choose largest
        var rect = faces.OrderByDescending(r => r.Width * r.Height).First();
        return new Mat(bgrFrame, rect).Clone();
    }
}
