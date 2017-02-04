using System.IO;
using System.Linq;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace TripToPrint.Services
{
    public interface IDialogService
    {
        void InvalidOperationMessage(string message);
        bool Confirm(string message, string title);
        string AskUserToSelectFile(string title, string initialFolder = null, string[] filter = null);
        string AskUserToSaveFile(string title, string fileName, string[] filter = null);
        string AskUserToSelectFolder(string initialFolder);
    }

    public class DialogService : IDialogService
    {
        public void InvalidOperationMessage(string message)
        {
            MessageBox.Show(message, "Invalid operation", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public bool Confirm(string message, string title)
        {
            return MessageBox.Show(message, title, MessageBoxButton.YesNo) == MessageBoxResult.Yes;
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

        public string AskUserToSelectFolder(string initialFolder)
        {
            var browser = new WinForms.FolderBrowserDialog
            {
                SelectedPath = initialFolder,
                ShowNewFolderButton = false
            };

            if (browser.ShowDialog() == WinForms.DialogResult.OK)
                return browser.SelectedPath;

            return null;
        }
    }
}
