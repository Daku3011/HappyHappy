using OpenCvSharp;
using OpenCvSharp.Face;
using System.Drawing;

public class FaceRecognizerService
{
    private readonly IWebHostEnvironment _env;
    private readonly string _datasetRoot;
    private readonly string _modelPath;
    private readonly FaceRecognizer _lbph;

    public FaceRecognizerService(IWebHostEnvironment env)
    {
        _env = env;
        _datasetRoot = Path.Combine(env.WebRootPath, "faces");
        Directory.CreateDirectory(_datasetRoot);

        var appData = Path.Combine(env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(appData);
        _modelPath = Path.Combine(appData, "face_lbph_model.yml");

        _lbph = LBPHFaceRecognizer.Create(radius: 2, neighbors: 12, gridX: 8, gridY: 8, threshold: 80); // tune threshold
        TryLoadModel();
    }

    private void TryLoadModel()
    {
        if (File.Exists(_modelPath))
        {
            _lbph.Read(_modelPath);
        }
    }

    public void RetrainFromDisk()
    {
        var images = new List<Mat>();
        var labels = new List<int>();

        foreach (var dir in Directory.EnumerateDirectories(_datasetRoot))
        {
            var labelStr = new DirectoryInfo(dir).Name; // studentId as folder
            if (!int.TryParse(labelStr, out var label)) continue;

            foreach (var file in Directory.EnumerateFiles(dir, "*.jpg"))
            {
                using var img = Cv2.ImRead(file, ImreadModes.Grayscale);
                if (img.Empty()) continue;
                Cv2.EqualizeHist(img, img);
                images.Add(img.Clone());
                labels.Add(label);
            }
        }

        if (images.Count >= 2) // LBPH needs at least two samples ideally
        {
            _lbph.Train(images, labels);
            _lbph.Write(_modelPath);
        }
    }

    public (int? Label, double Confidence) Predict(Mat faceBgr)
    {
        using var gray = new Mat();
        Cv2.CvtColor(faceBgr, gray, ColorConversionCodes.BGR2GRAY);
        Cv2.Resize(gray, gray, new OpenCvSharp.Size(200, 200)); // normalize size
        Cv2.EqualizeHist(gray, gray);

        _lbph.Predict(gray, out int label, out double conf);
        // Lower conf is better for LBPH; if using threshold above, you can check conf <= threshold
        // But OpenCvSharp's LBPH often returns conf ~ 30–70 for good matches. We'll accept <=80.
        if (double.IsInfinity(conf) || conf > 80) return (null, conf);
        return (label, conf);
    }
}
