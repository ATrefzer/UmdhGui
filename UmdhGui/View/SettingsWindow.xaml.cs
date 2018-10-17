using System.Windows;

namespace UmdhGui.View
{
    /// <summary>
    ///     Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Click_Ok(object sender, RoutedEventArgs e)
        {
            // Closes the dialog. Content is already saved.
            DialogResult = true;
        }
    }
}