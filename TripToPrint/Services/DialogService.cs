using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

using WinForms = System.Windows.Forms;

namespace TripToPrint.Services
{
    public interface IDialogService
    {
        Task InvalidOperationMessage(string message);
        string AskUserToSelectFile(string title, string initialFolder = null, string[] filter = null);
        string AskUserToSaveFile(string title, string fileName, string[] filter = null);
    }

    public class DialogService : IDialogService
    {
        public async Task InvalidOperationMessage(string message)
        {
            using (new NormalCursor())
            {
                var window = Application.Current.Windows.OfType<MetroWindow>().SingleOrDefault(x => x.IsActive);
                await window.ShowMessageAsync("Invalid operation", message, MessageDialogStyle.Affirmative, new MetroDialogSettings {
                    AnimateShow = true,
                    ColorScheme = MetroDialogColorScheme.Accented
                });
            }
        }

        public string AskUserToSelectFile(string title, string initialFolder = null, string[] filter = null)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                ShowReadOnly = true,
                CheckFileExists = true,
                Filter = string.Join("|", (filter ?? new string[0]).Concat(new[] { "All files (*.*)|*.*" }))
            };

            if (ofd.ShowDialog() == true)
                return ofd.FileName;
            return null;
        }

        public string AskUserToSaveFile(string title, string fileName, string[] filter = null)
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = title,
                DefaultExt = Path.GetExtension(fileName),
                FileName = fileName,
                Filter = string.Join("|", (filter ?? new string[0]).Concat(new[] { "All files (*.*)|*.*" }))
            };

            if (saveDialog.ShowDialog() == true)
                return saveDialog.FileName;

            return null;
        }
    }
}
