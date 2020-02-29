using ChatApplicationAppLibrary.Model;
using ChatApplicationAppLibrary.Model.Shared;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
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

        //Thread of the UI
        SynchronizationContext uiContext = SynchronizationContext.Current;


        public ClientViewModel()
        {
            Client = new ClientModel();
            Client.Messages.Add("You are not connected to any server yet");
            
            //Add function to the close event.
            Application.Current.MainWindow.Closing += new CancelEventHandler(MainWindowClosing);
        }

        //Connect and disconnect Command for the UI
        public ICommand ConnectDisconnectCommand
        {
            get { return new DelegateCommand<object>(ConnectDisConnectFunc, CanConnectDisConnect); }
        }

        //Can always connect and disconnect.
        private bool CanConnectDisConnect(object context) => true;

        private async void ConnectDisConnectFunc(object context)
        {
            try
            {
                if (Client.Active)
                {
                    await Task.Run(CloseConnectWithServer);
                    return;
                }

                await Task.Run(ConnectToServer);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        //Connect to server
        private async Task ConnectToServer()
        {
            try
            {
                TcpClient = new TcpClient();
                IPAddress address = IPAddress.Parse(Client.IpAddress);

                await TcpClient.ConnectAsync(address, Client.PortNumber);


                network = TcpClient.GetStream();

                //Write to the server the client name. 
                await WriteToServer(Client.Name);
                
                Client.Active = true;
                
                //Add message in the UI from the UI thread.
                uiContext.Send(x => Client.Messages.Add("Connect to the server"), null);

                //Begin to listen to the server.
                await Task.Run(ListenToServer);
            }
            catch (SocketException socketEx)
            {
                //Show the message to the use just in case the connect is not esatblished.
                MessageBox.Show(socketEx.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //Keep listen to server while the client is active
        private async void ListenToServer()
        {
            while (Client.Active)
            {
                try
                {
                    await Task.Run(ReadFromServer);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }
        }

        private async Task ReadFromServer()
        {
            StringBuilder message = new StringBuilder();
            var streamParts = new byte[Client.BufferSize];
            int numberOfBytesRead = 0;
            do
            {
                try
                {
                    //Read the from the server with the buffer size.
                    numberOfBytesRead = await network.ReadAsync(streamParts, 0, streamParts.Length);

                    //Add the readed bytes to the stringbuilder. 
                    message.AppendFormat("{0}", Encoding.ASCII.GetString(streamParts, 0, numberOfBytesRead));
                }
                catch (IOException e)
                {
                    await Task.Run(CloseConnectWithServer);
                    Console.WriteLine(e.Message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
                //Keep reading until reading the end of message signature. 
            } while (!ReachedTheEndOfTheMessage(message));

            var chatMessage = message.ToString();

            if (chatMessage != "")
            {
                chatMessage = FilteMessageWithourSignature(chatMessage);
                await HandleIncomingMessage(chatMessage);
            }
        }

        //check of the function ends with the end of message signatue. 
        bool ReachedTheEndOfTheMessage(StringBuilder sb)
        {
            var messageToChek = sb.ToString();
            return messageToChek.EndsWith(NOTsApplicationProtocol.END_OF_MESSAGE_SIGNATURE);
        }
           
        //Delete the end of message signature from the message. 
        private string FilteMessageWithourSignature(string message)
        {
            int indexOfSignature = message.IndexOf(NOTsApplicationProtocol.END_OF_MESSAGE_SIGNATURE);

            if (indexOfSignature != -1)
            {
                message = message.Remove(indexOfSignature);
            }

            return message;
        }
        
        //Decide what to do with inconming message.
        private async Task HandleIncomingMessage(string message)
        {
            var messageToCheck = message.ToLower();
            const string leaveMessag = "from server: " + NOTsApplicationProtocol.LEAVE_CHAT_SECRET_WORD;
            switch (messageToCheck)
            {
                case leaveMessag:

                    //Disconnect when the server does't work any more. 
                    uiContext.Send(x => Client.Messages.Add("server crasht"), null);
                    await Task.Run(CloseConnectWithServer);
                    break;

                case "":
                    break;
                default:

                    //Add message in the UI from the UI thread.
                    uiContext.Send(x => Client.Messages.Add(message), null);
                    break;
            }
        }
        
        //Clos connect with the server and send bye to the server. 
        private async void CloseConnectWithServer()
        {
            try
            {
                await WriteToServer("bye");
                TcpClient.Close();
                uiContext.Send(x => Client.Messages.Add("Disconnected from the server"), null);
                Client.Active = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //Send command for the UI
        public ICommand SendMessageCommand
        {
            get { return new DelegateCommand<object>(SendMessageFunc, CanSendMessageFun); }
        }
        
        //Can only send message when the the client connected to the server.
        private bool CanSendMessageFun(object obj) => Client.Active;


        private async void SendMessageFunc(object obj)
        {
            await WriteToServer(Client.ToSendMessage);
            
            //Add the message to the UI
            Client.Messages.Add(Client.ToSendMessage);

            //Clear the message in the UI
            Client.ToSendMessage = "";
        }

        private async Task WriteToServer(string message)
        {

            message = string.Concat(message, NOTsApplicationProtocol.END_OF_MESSAGE_SIGNATURE);
            int startIndex = 0;

            //Keep sending the message parts unitl reaches the lase part. 
            while (startIndex < message.Length)
            {
                try
                {
                    var streamParts = Encoding.ASCII.GetBytes(ToSendString(startIndex, message));
                    await network.WriteAsync(streamParts, 0, streamParts.Length);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                //Increase the start index. 
                startIndex += Client.BufferSize;
            }
        }

        //Devide the message in parts and send the right parts based on the buffer size. 
        string ToSendString(int satartIndex, string messageString)
        {
            if (satartIndex + Client.BufferSize < messageString.Length)
                return messageString.Substring(satartIndex, Client.BufferSize);

            return messageString.Substring(satartIndex);
        }

        // close the connection when the window closed. 
        async void MainWindowClosing(object sender, CancelEventArgs e)
        {
            await Task.Run(CloseConnectWithServer);
        }
    }
}
