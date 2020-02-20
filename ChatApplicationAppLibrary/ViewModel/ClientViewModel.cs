using ChatApplicationAppLibrary.Model;
using ChatApplicationAppLibrary.Model.Shared;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ChatApplicationAppLibrary.ViewModel
{
    public class ClientViewModel
    {
        TcpClient TcpClient;

        public ClientModel Client { get; set; }

        NetworkStream network;

        SynchronizationContext uiContext = SynchronizationContext.Current;

        public ClientViewModel()
        {
            Client = new ClientModel();
            Client.Messages.Add("You are not connected to any server");
            Application.Current.MainWindow.Closing += new CancelEventHandler(MainWindowClosing);
        }

        public ICommand ConnectDisconnectCommand
        {
            get { return new DelegateCommand<object>(ConnectDisConnectFunc, CanConnectDisConnect); }
        }

        public ICommand SendMessageCommand
        {
            get { return new DelegateCommand<object>(SendMessageFunc, CanSendMessageFun); }
        }

        private bool CanSendMessageFun(object obj)
        {
            return Client.Active;
        }

        private async void SendMessageFunc(object obj)
        {
            await Task.Run(() => WriteToServer(Client.ToSendMessage));
            Client.Messages.Add(Client.ToSendMessage);
            Client.ToSendMessage = "";
        }

        private async void ConnectDisConnectFunc(object context)
        {
            try
            {
                if (Client.Active)
                {
                    await EndConnect();
                }
                else
                {
                    await StartConnect();
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private async Task StartConnect()
        {
            try
            {
                TcpClient = new TcpClient();

                await TcpClient.ConnectAsync("", Client.PortNumber);
                network = TcpClient.GetStream();

                await WriteToServer(Client.Name);

                Client.Active = true;
                Client.Messages.Add("Connect to the server");
                await Task.Run(ListenToServer);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private bool CanConnectDisConnect(object context)
        {
            return true;
        }

        private async Task EndConnect()
        {
            try
            {
                await WriteToServer("bye");
                TcpClient.Close();
                uiContext.Send(x => Client.Messages.Add("Disconnected from the server"), null);
                Client.Active = false;
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private async Task WriteToServer(string message)
        {

            message = string.Concat(message, NOTsApplicationProtocol.END_OF_MESSAGE_SIGNATURE);
            try
            {
                var streamParts = Encoding.UTF8.GetBytes(message);
                await network.WriteAsync(streamParts, 0, streamParts.Length);
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /*private string toSendString(string messageString)
        {
            if (messageString.Length > Client.BufferSize)
                return messageString.Substring(0, Client.BufferSize);

            return messageString;
        }

        private string FilterMessage(string message)
        {
            if (message.Length < Client.BufferSize)
               return message.Remove(0);

            return  message.Remove(0, Client.BufferSize);
        }*/

        private async void ListenToServer()
        {
            while(Client.Active)
            {
                try
                {
                    await ReadFromServer();
                }catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }
        }

        private async Task ReadFromServer()
        {
            string message = "";
            var streamParts = new byte[Client.BufferSize];
            while (!message.Contains(NOTsApplicationProtocol.END_OF_MESSAGE_SIGNATURE) && network.DataAvailable)
            {
                try
                {
                    await network.ReadAsync(streamParts, 0, streamParts.Length);
                    message = string.Concat(message, Encoding.UTF8.GetString(streamParts));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            if (message != "")
            {
                network.Flush();
                message = FilteMessageWithourSignature(message);
                HandleIncomingMessage(message);
            }
        }

        private string FilteMessageWithourSignature(string message)
        {
            int indexOfSignature = message.IndexOf(NOTsApplicationProtocol.END_OF_MESSAGE_SIGNATURE);
            if (indexOfSignature != -1)
            {
                message = message.Remove(indexOfSignature);
            }
            return message;
        }

        private void HandleIncomingMessage(string message)
        {
            var messageToCheck = message.ToLower();
            const string leaveMessag = "from server: " + NOTsApplicationProtocol.LEAVE_CHAT_SECRET_WORD;
            switch (messageToCheck)
            {
                case leaveMessag:
                    uiContext.Send(x => Client.Messages.Add("server crasht"), null);
                    Task.Run(EndConnect).Wait();
                    break;
                default:
                    uiContext.Send(x => Client.Messages.Add(message), null);
                    break;
            }
        }

        async void MainWindowClosing(object sender, CancelEventArgs e)
        {
            await EndConnect();
        }
    }
}
