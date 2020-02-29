using MaterialDesignThemes.Wpf;
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

namespace ChatApplicationServer
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

        private void InputNumberValidator(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ServerPort_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if(ValidateAllProperties())
            {
                StartStopServerButton.IsEnabled = true;
            }
            else
            {
                StartStopServerButton.IsEnabled = false;
                MessageBox.Show("Don't let the server properties empty and enter value bigger than zero in port and buffersize please!");
            }
        }

        private bool ValidateAllProperties() => (ValidateServerBufferSize() && ValidateServerPortNumber() && !string.IsNullOrEmpty(ServerName.Text));

        private bool ValidateServerBufferSize() => (!string.IsNullOrEmpty(ServerBufferSize.Text) && Int32.Parse(ServerBufferSize.Text) > 0);
        private bool ValidateServerPortNumber() => (!string.IsNullOrEmpty(ServerPort.Text) && Int32.Parse(ServerPort.Text) > 0);

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
