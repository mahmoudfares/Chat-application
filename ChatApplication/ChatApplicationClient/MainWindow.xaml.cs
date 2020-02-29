using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChatApplicationAppLibrary.ViewModel;

namespace ChatApplicationClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
             InitializeComponent();
        }

        void InputNumberValidator(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        void ServerPort_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (ValidateAllProperties())
            {
                btnConnect.IsEnabled = true;
            }
            else
            {
                btnConnect.IsEnabled = false;
                MessageBox.Show("Don't let the server properties empty and enter value bigger than zero in port and buffersize please!");
            }
        }

        bool ValidateAllProperties() => (ValidateServerBufferSize() && ValidateServerPortNumber() && !string.IsNullOrEmpty(Name.Text));
        bool ValidateServerBufferSize() => (!string.IsNullOrEmpty(BufferSize.Text) && Int32.Parse(BufferSize.Text) > 0);
        bool ValidateServerPortNumber() => (!string.IsNullOrEmpty(PortNumber.Text) && Int32.Parse(PortNumber.Text) > 1023);

        bool EnabelSendMessageButton() => ToSendMessage.Text.Length > 0;


    }
}
