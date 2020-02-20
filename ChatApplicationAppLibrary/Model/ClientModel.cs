using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using ChatApplicationAppLibrary.Annotations;
using ChatApplicationAppLibrary.Model.Enums;

namespace ChatApplicationAppLibrary.Model
{
    public class ClientModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> Messages { get; private set; } = new ObservableCollection<string>();
        bool active = false;
        int bufferSize = (int)Settings.MaximumBufferSize;
        int portNumber = (int)Settings.MinimumAllowedPortNumber;
        string toSendMessage;
        public bool EnaleSettingsEditing { get => !active; }
        public string IpAddress { get; private set; } = "127.0.0.1";
        List<string> Names { get; set; } = new List<string>() { "Mahmoud", "Roy", "Corine", "Marijn", "Ido", "Chris", "Suli", "Jesse", "Daan" };
        string name;

        public ClientModel()
        {
            name = Names.ElementAt(new Random().Next(0, Names.Count()));
        }

        public string ConnectDisconnectButtonText
        {
            get
            {
                if (active)
                    return "Disconnect";

                return "Connect";
            }
        }

        public bool Active
        {
            get => active;
            set { 
                active = value;

                //Change the other properties in the UI when this changes 
                OnPropertyChanged("EnaleSettingsEditing");
                OnPropertyChanged("ConnectDisconnectButtonText");
            }
        }
        
        public int BufferSize
        {
            get => bufferSize;
            set {
                if (!IsInTheRang(value, (int)Settings.MinimumBufferSize, (int)Settings.MaximumBufferSize))
                {
                    portNumber = (int)Settings.MaximumBufferSize;
                    MessageBox.Show($"For bufffer size must be between {(int)Settings.MinimumBufferSize - 1} and { (int)Settings.MaximumBufferSize + 1}");
                    return;
                }
                bufferSize = value;
                OnPropertyChanged();
            }
        }

        public int PortNumber
        {
            get => portNumber;
            set
            {
                if (!IsInTheRang(value, (int)Settings.MinimumAllowedPortNumber, (int)Settings.MaximumAllowedPortNumber))
                {
                    portNumber = (int)Settings.MinimumAllowedPortNumber;
                    MessageBox.Show($"For port number must be between {(int)Settings.MinimumAllowedPortNumber - 1} and { (int)Settings.MaximumAllowedPortNumber + 1}");
                    return;
                }
                portNumber = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        public string ToSendMessage
        {
            get => toSendMessage;
            set
            {
                toSendMessage = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //Updates the property in the UI.
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool IsInTheRang(int number, int minimum, int maximum) => (number >= minimum && number <= maximum);

    }
}