using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace TripToPrint.Services
{
    public interface IClipboardService
    {
        void SetText(string text);
    }

    [ExcludeFromCodeCoverage]
    public sealed class ClipboardService : IClipboardService
    {
        public void SetText(string text) => Clipboard.SetText(text, TextDataFormat.UnicodeText);
    }
}
