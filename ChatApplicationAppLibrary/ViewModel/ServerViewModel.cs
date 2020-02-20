using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ChatApplicationAppLibrary.Model;
using ChatApplicationAppLibrary.Model.Shared;
using Microsoft.VisualStudio.PlatformUI;

namespace ChatApplicationAppLibrary.ViewModel
{
    public class ServerViewModel
    {
        TcpListener tcpListener;
        SynchronizationContext uiContext = SynchronizationContext.Current;
        Thread beginAcceptClientThread;
        

        public ServerViewModel()
        {
            Server = new ServerModel();
            Server.Messages.Add(new Message { Text = "Ready to start the server", SenderName = "Server" });
            Application.Current.MainWindow.Closing += new CancelEventHandler(MainWindow_Closing);
        }

        public ServerModel Server { get; }

        public ICommand MyButtonClickCommand
        {
            get { return new DelegateCommand<object>(StartStopListen, CanStartStopListen); }
        }

        private async void StartStopListen(object context)
        {
            try
            {
                if (Server.Active)
                {
                    CLoseConnection();
                }
                else
                {
                    IntiizeTcpListner();
                    beginAcceptClientThread = new Thread(BeginAcceptClients);
                    beginAcceptClientThread.Start();
                    await Task.Run(ReadFromAllClients);

                }
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        private void CLoseConnection()
        {
            try
            {
                Task.Run(() => WriteToCLients(NOTsApplicationProtocol.LEAVE_CHAT_SECRET_WORD)).Wait();
                tcpListener.Stop();
                Server.Active = false;
                Server.Clients.Clear();
                Server.Messages.Add(CreateMessage("Server has been closed"));
   
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private bool CanStartStopListen(object context)
        {
            return true;
        }

        public ICommand SendMessageCommand
        {
            get { return new DelegateCommand<object>(SendMessageFunc, CanSendMessageFun); }
        }

        private bool CanSendMessageFun(object context)
        {
            return (Server.Active && Server.Clients.Count > 0);
        }

        private void SendMessageFunc(object context)
        {
            try
            {
                Task.Run(() => WriteToCLients(Server.ToSendMessage)).Wait();
                Server.Messages.Add(CreateMessage(Server.ToSendMessage));
                Server.ToSendMessage = "";
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private async void WriteToCLients(string message)
        {
            if (Server.Clients.Count > 0)
            {
                message = string.Concat($"From Server: {message}", NOTsApplicationProtocol.END_OF_MESSAGE_SIGNATURE);

                foreach (ClientOnServerSide clientFromColection in Server.Clients.ToList())
                {
                    await WriteToSingleClient(clientFromColection, message);
                }

            }
        }

        private async Task WriteToSingleClient(ClientOnServerSide client, string message)
        {
            try
            {
                var network = client.TcpClient.GetStream();
                var streamParts = Encoding.UTF8.GetBytes(message);
                await network.WriteAsync(streamParts, 0, streamParts.Length);
                network.Flush();
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        private string toSendString(string messageString)
        {
            if (messageString.Length > Server.BufferSize)
                return messageString.Substring(0, Server.BufferSize);
            
            return messageString;
        }

        /*
        private string FilterMessage(string message)
        {
            if (message.Length < Server.BufferSize)
                return message.Remove(0);

            return message.Remove(0, Server.BufferSize);
        }
        */

        private void IntiizeTcpListner()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, Server.PortNumber);
                tcpListener.Start();
                
                Server.Messages.Add(CreateMessage("Waiting for clients"));
                Server.Active = true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Something went wrong {e.Message}");
            }
        }

        private void BeginAcceptClients()
        {
            while (true)
            {
                try
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    ReadNameFromClient(tcpClient);
                }catch(Exception e)
                {
                    tcpListener.Stop();
                    Console.WriteLine(e.Message);
                    break;
                }
            }
        }

        private void ReadFromAllClients()
        {
            while (true)
            {
                foreach(ClientOnServerSide client in Server.Clients.ToList())
                {
                    try
                    { 
                      Task.Run(() => ReadFromClient(client)).Wait();
                    }
                    catch (InvalidOperationException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }

        }

        private void AddClientToServer(TcpClient tcpClient, string name)
        {
            try
            {
                var client = new ClientOnServerSide()
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    TcpClient = tcpClient
                };
                uiContext.Send(x => Server.Clients.Add(client), null);
                uiContext.Send(x => Server.Messages.Add(CreateMessage($"{client.Name} just connected")), null);
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private Message CreateMessage(string text, string senderName = "Server") => new Message { SenderName = senderName, Text = text };

        private async void ReadNameFromClient(TcpClient client)
        {
            var network = client.GetStream();
            string name = "";
            try
            {
                byte[] streamParts = new byte[Server.BufferSize];

                while (!name.Contains(NOTsApplicationProtocol.END_OF_MESSAGE_SIGNATURE))
                {
                    await network.ReadAsync(streamParts, 0, Server.BufferSize);
                    name = string.Concat(name, Encoding.UTF8.GetString(streamParts));
                }
                name = FilteMessageWithourSignature(name);
                AddClientToServer(client, name);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        private async void ReadFromClient(ClientOnServerSide client)
        {
            string message = "";
            NetworkStream network = client.TcpClient.GetStream();
            try
            {
                    var streamParts = new byte[Server.BufferSize];
                    while (!message.Contains(NOTsApplicationProtocol.END_OF_MESSAGE_SIGNATURE) && network.DataAvailable)
                    {
                        await network.ReadAsync(streamParts, 0, 50);
                        message = string.Concat(message, Encoding.UTF8.GetString(streamParts));
                    }
                    network.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            message = FilteMessageWithourSignature(message);
            if(message != "")
            {
                HandleIncomingMessage(message, client);
            }
        }

        private string FilteMessageWithourSignature(string message)
        {
            int indexOfSignature = message.IndexOf(NOTsApplicationProtocol.END_OF_MESSAGE_SIGNATURE);
            if(indexOfSignature != -1)
            {
                message = message.Remove(indexOfSignature);
            }
            return message;
        }

        private void HandleIncomingMessage(string message, ClientOnServerSide client)
        {
            var messageToCheck = message.ToLower();
            switch (messageToCheck) 
            { 
                case NOTsApplicationProtocol.LEAVE_CHAT_SECRET_WORD:
                    uiContext.Send(x=> Server.Messages.Add(CreateMessage($"{client.Name} just leaved")), null);
                    uiContext.Send(x=> Server.Clients.Remove(client), null);
                    break;
                default:
                    Task.Run(() => RedirectToClients(client, $"From {client.Name}: {message}")).Wait();
                    uiContext.Send(x=> Server.Messages.Add(CreateMessage(message, client.Name)), null);

                    break;
            }
        }

        private async void RedirectToClients(ClientOnServerSide client, string message)
        {
            message = string.Concat(message, NOTsApplicationProtocol.END_OF_MESSAGE_SIGNATURE);

            if (Server.Clients.Count > 1) {
                var toRedirectClients = Server.Clients.Where(x => x != client).ToList();
                foreach(ClientOnServerSide toRedirectClient in toRedirectClients)
                {
                    await WriteToSingleClient(toRedirectClient, message);
                }

            }
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            CLoseConnection();
        }


    }
}
