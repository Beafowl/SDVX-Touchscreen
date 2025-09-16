using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace Model
{
    public class TemplateMatcher
    {
        public static double MatchingThreshold { get; set; } = 0.9;

        /// <summary>
        /// Checks whether the template can be found in the reference image.
        /// </summary>
        /// <param name="referencePath">A path to the reference image.</param>
        /// <param name="templatePath">A path to the template image.</param>
        /// <returns><c>true</c> if the template has been found, else <c>false</c>.</returns>
        public static bool TemplateInImage(string referencePath, string templatePath)
        {
            using (Mat refMat = new Mat(referencePath))
            using (Mat tplMat = new Mat(templatePath))
            using (Mat res = new Mat(refMat.Rows - tplMat.Rows + 1, refMat.Cols - tplMat.Cols + 1, MatType.CV_32FC1))
            {
                //Convert input images to gray
                Mat gref = refMat.CvtColor(ColorConversionCodes.BGR2GRAY);
                Mat gtpl = tplMat.CvtColor(ColorConversionCodes.BGR2GRAY);
                Cv2.MatchTemplate(gref, gtpl, res, TemplateMatchModes.CCoeffNormed);
                Cv2.Threshold(res, res, 0.8, 1.0, ThresholdTypes.Tozero);
                double minval, maxval;
                OpenCvSharp.Point minloc, maxloc;
                Cv2.MinMaxLoc(res, out minval, out maxval, out minloc, out maxloc);

                // min and max val are between 0 and 1, with 1 being a perfect match
                return maxval > MatchingThreshold;
            }
        }
        /// <summary>
        /// Checks whether the template can be found in the reference image.
        /// </summary>
        /// <param name="referencePath">A path to the reference image.</param>
        /// <param name="templatePath">A path to the template image.</param>
        /// <returns><c>true</c> if the template has been found, else <c>false</c>.</returns>
        public static bool TemplateInImage(Bitmap reference, string templatePath)
        {
            using (Mat refMat = BitmapConverter.ToMat(reference))
            using (Mat tplMat = new Mat(templatePath))
            using (Mat res = new Mat(refMat.Rows - tplMat.Rows + 1, refMat.Cols - tplMat.Cols + 1, MatType.CV_32FC1))
            {
                //Convert input images to gray
                Mat gref = refMat.CvtColor(ColorConversionCodes.BGR2GRAY);
                Mat gtpl = tplMat.CvtColor(ColorConversionCodes.BGR2GRAY);
                Cv2.MatchTemplate(gref, gtpl, res, TemplateMatchModes.CCoeffNormed);
                Cv2.Threshold(res, res, 0.8, 1.0, ThresholdTypes.Tozero);
                double minval, maxval;
                OpenCvSharp.Point minloc, maxloc;
                Cv2.MinMaxLoc(res, out minval, out maxval, out minloc, out maxloc);

                // min and max val are between 0 and 1, with 1 being a perfect match
                return maxval > MatchingThreshold;
            }
        }
    }
}
