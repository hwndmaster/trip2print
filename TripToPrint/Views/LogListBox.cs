using System.Drawing;
using System.Resources;
using System.Windows.Forms;

using TripToPrint.Core.Logging;

namespace TripToPrint.Views
{
    public sealed class LogListBox : ListBox
    {
        private const int DEFAULT_IMAGE_WIDTH = 16;

        private readonly Bitmap _imageError;
        private readonly Bitmap _imageWarning;

        public LogListBox()
        {
            this.DrawMode = DrawMode.OwnerDrawVariable;
            this.ItemHeight = 18;
            this.IntegralHeight = false;

            var resources = new ResourceManager(typeof(LogListBox));
            _imageError = (Bitmap)resources.GetObject("Error");
            _imageWarning = (Bitmap)resources.GetObject("Warning");
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index >= this.Items.Count || e.Index <= -1)
                return;

            var item = this.Items[e.Index] as LogItem;
            var text = item?.Text ?? this.Items[e.Index].ToString();
            var severity = item?.Severity ?? LogSeverity.Info;
            var color = GetColorByLogSeverity(severity);
            var image = GetImageByLogSeverity(severity);

            e.DrawBackground();
            e.DrawFocusRectangle();

            if (image != null)
            {
                e.Graphics.DrawImage(image, new PointF(0, e.Bounds.Y));
            }

            e.Graphics.DrawString(text, this.Font, new SolidBrush(color),
                new PointF(e.Bounds.X + DEFAULT_IMAGE_WIDTH, e.Bounds.Y + 1));
        }

        private Bitmap GetImageByLogSeverity(LogSeverity logSeverity)
        {
            switch (logSeverity)
            {
                case LogSeverity.Warning:
                    return _imageWarning;
                case LogSeverity.Error:
                    return _imageError;
                case LogSeverity.Info:
                default:
                    return null;
            }
        }

        private Color GetColorByLogSeverity(LogSeverity logSeverity)
        {
            switch (logSeverity)
            {
                case LogSeverity.Warning:
                    return Color.Chocolate;
                case LogSeverity.Error:
                    return Color.OrangeRed;
                case LogSeverity.Info:
                default:
                    return SystemColors.WindowText;
            }
        }
    }
}
