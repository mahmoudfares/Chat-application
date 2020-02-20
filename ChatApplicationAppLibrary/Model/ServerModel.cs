using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ChatApplicationAppLibrary.Annotations;
using ChatApplicationAppLibrary.Model.Enums;

namespace ChatApplicationAppLibrary.Model
{
    public class ServerModel: INotifyPropertyChanged
    {
        public ObservableCollection<Message> Messages { get; private set; }
        private bool active = false;
        public ObservableCollection<ClientOnServerSide> Clients { get; set; }
        private int bufferSize = (int) Settings.MaximumBufferSize;
        private int portNumber = (int) Settings.MinimumAllowedPortNumber;
        private string name = "Nots server";
        public bool EnaleSettingsEditing { get => !active;}
        string toSendMessage = "";

        public string StartStopButtonText { 
            get
            {
                if (active)
                    return "Stop";
                return "Start";
            } 
        }

        public bool Active
        {
            get => active;
            set { 
                active = value;
                OnPropertyChanged("EnaleSettingsEditing");
                OnPropertyChanged("StartStopButtonText");
            }
        }
        
        public int BufferSize
        {
            get => bufferSize;
            set {
                if (!IsInTheRang(value, (int)Settings.MinimumBufferSize, (int)Settings.MaximumBufferSize))
                {
                    portNumber = (int)Settings.MaximumBufferSize;
                    MessageBox.Show($"For bufffer size must be between {(int)Settings.MinimumBufferSize -1} and { (int)Settings.MaximumBufferSize + 1}");
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
                if(!IsInTheRang(value, (int) Settings.MinimumAllowedPortNumber, (int) Settings.MaximumAllowedPortNumber))
                {
                    portNumber = (int)Settings.MinimumAllowedPortNumber;
                    MessageBox.Show($"For port number must be between {(int) Settings.MinimumAllowedPortNumber - 1} and { (int) Settings.MaximumAllowedPortNumber + 1}");
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

        public ServerModel()
        {
            Messages = new ObservableCollection<Message>();
            Clients = new ObservableCollection<ClientOnServerSide>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool IsInTheRang(int number, int minimum, int maximum) => (number >= minimum && number <= maximum);
    }
}
