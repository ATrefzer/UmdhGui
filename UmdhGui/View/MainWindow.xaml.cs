using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Input;

namespace UmdhGui.View
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MainWindow : RibbonWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            var txtBox = sender as TextBox;
            if (txtBox != null)
            {
                var binding = BindingOperations.GetBindingExpression(txtBox, TextBox.TextProperty);
                if (binding != null)
                    binding.UpdateSource();
            }
        }
    }
}