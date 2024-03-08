using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Notepad.Components.UserControls
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ClearableTextBox : Window
    {
        public TextBox TextBoxControl => txtInput;

        /// <summary>
        /// Initialize the Clearable Title Box
        /// </summary>
        public ClearableTextBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set the visibility of the text whether
        /// the user wrote in the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtInput.Text))
            {
                tbPlaceholder.Visibility = Visibility.Visible;
            }
            else
            {
                tbPlaceholder.Visibility = Visibility.Collapsed;
            }
        }
    }
}
