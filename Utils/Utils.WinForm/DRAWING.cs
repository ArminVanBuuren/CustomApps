using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace Utils.WinForm
{
    public struct FormLocation
    {
        public Screen Screen { get; internal set; }
        public Point Location { get; internal set; }
    }

    public static class DRAWING
    {
        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(this Bitmap image, int width, int height)
        {
            if (image.Width == width && image.Height == height)
                return image;

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }


            //using (var mss = new MemoryStream())
            //{
            //    EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 60L);
            //    ImageCodecInfo imageCodec = ImageCodecInfo.GetImageEncoders().FirstOrDefault(o => o.FormatID == ImageFormat.Jpeg.Guid);
            //    EncoderParameters parameters = new EncoderParameters(1);
            //    parameters.Param[0] = qualityParam;
            //    destImage.Save(mss, imageCodec, parameters);
            //    return (Bitmap)Image.FromStream(mss);
            //}

            return destImage;
        }

        /// <summary>Returns the location of the form relative to the top-left corner
        /// of the screen that contains the top-left corner of the form, or null if the
        /// top-left corner of the form is off-screen.</summary>
        public static FormLocation GetLocationWithinScreen(Form form)
        {
            foreach (var screen in Screen.AllScreens)
            {
                if (screen.Bounds.Contains(form.Location))
                {
                    return new FormLocation
                    {
                        Screen = screen,
                        Location = new Point(form.Location.X - screen.Bounds.Left, form.Location.Y - screen.Bounds.Top)
                    };
                }
            }

            return new FormLocation
            {
                Screen = Screen.AllScreens.FirstOrDefault(),
                Location = form.Location
            };
        }
    }
}