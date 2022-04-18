using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UmdhGui.ViewModel;

namespace UmdhGui.View
{
    /// <summary>
    ///     Interaction logic for ProcessSelectionWindow.xaml
    ///     Selected process is found (binding) in the view model after the window is closed.
    /// </summary>
    public partial class ProcessWindow
    {
        public ProcessWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ProcessList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DialogResult = true;
        }
    }
}