using System.Windows;

namespace UmdhGui.View
{
    internal class MessageBoxService
    {
        internal void ShowError(string message, string caption)
        {
            MessageBox.Show(Application.Current.MainWindow, message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}