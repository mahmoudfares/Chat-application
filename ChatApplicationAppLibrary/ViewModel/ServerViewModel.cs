using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
        public ServerModel Server { get; }


        //Thread of the UI
        SynchronizationContext uiContext = SynchronizationContext.Current;

        public ServerViewModel()
        {
            Server = new ServerModel();
            //
            Server.Messages.Add(new Message { Text = "Ready to start the server", SenderName = "Server" });
            
            //Add function to the close event.
            Application.Current.MainWindow.Closing += new CancelEventHandler(MainWindow_Closing);
        }

        //Start & stop server Command for the UI
        public ICommand StartStopButtonCommand
        {
            get { return new DelegateCommand<object>(StartStopListenFunc, CanStartStopListen); }
        }

        //Can always start and stop server.
        bool CanStartStopListen(object context) => true;

        async void StartStopListenFunc(object context)
        {
            try
            {
                if (Server.Active)
                {
                    await Task.Run(CLoseConnection);
                    return;
                }

                IntiizeTcpListner();

                Thread beginAcceptClientThread = new Thread(BeginAcceptClients);
                beginAcceptClientThread.Start();

                await Task.Run(ReadFromAllClients);
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //Close the server.
        async void CLoseConnection()
        {
            try
            {
                //Write to all the clients that the server not available any more. 
                await Task.Run(() => WriteToCLients(NOTsApplicationProtocol.LEAVE_CHAT_SECRET_WORD));

                tcpListener.Stop();
                Server.Active = false;

                //Delete allt the clients.
                Server.Clients.Clear();
                Server.Messages.Add(CreateMessage("Server has been closed"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //Write to all clients.
        private async void WriteToCLients(string message)
        {
            if (Server.Clients.Count > 0)
            {
                foreach (ClientOnServerSide clientFromCollection in Server.Clients.ToList())
                {
                    await WriteToSingleClient(clientFromCollection, message);
                }

            }
        }

        //Write to single client. 
        private async Task WriteToSingleClient(ClientOnServerSide client, string message)
        {
            //Add end of message signature. 
            message = string.Concat($"From Server: {message}", NOTsApplicationProtocol.END_OF_MESSAGE_SIGNATURE);

            int startIndex = 0;
            var network = client.TcpClient.GetStream();

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
                startIndex += Server.BufferSize;
            }

        }

        //Devide the message in parts and send the right parts based on the buffer size. 
        string ToSendString(int satartIndex, string messageString)
        {
            if (satartIndex + Server.BufferSize < messageString.Length)
                return messageString.Substring(satartIndex, Server.BufferSize);

            return messageString.Substring(satartIndex);
        }

        //Keep accepting cliets. 
        void BeginAcceptClients()
        {
            while (true)
            {
                try
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    ReadNameFromClient(tcpClient);
                }
                catch (Exception e)
                {
                    tcpListener.Stop();
                    Console.WriteLine(e.Message);
                    break;
                }
            }
        }

        void ReadNameFromClient(TcpClient client)
        {
            var network = client.GetStream();
            var streamParts = new byte[Server.BufferSize];
            int numberOfBytesRead = 0;
            StringBuilder message = new StringBuilder();

            do
            {
                try
                {
                    //Read the from the server with the buffer size.
                    numberOfBytesRead = network.Read(streamParts, 0, streamParts.Length);

                    //Add the readed bytes to the stringbuilder. 
                    message.AppendFormat("{0}", Encoding.ASCII.GetString(streamParts, 0, numberOfBytesRead));
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            
                //Keep reading until reading the end of message signature. 
            } while (!ReachedTheEndOfTheMessage(message));

            var name = message.ToString();
            name = FilteMessageWithourSignature(name);

            AddClientToServer(client, name);
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

        //Start the server. 
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

        async void ReadFromAllClients()
        {
            while (true)
            {
                foreach (ClientOnServerSide client in Server.Clients.ToList())
                {
                    try
                    {
                        await ReadFromClient(client);
                    }
                    catch (InvalidOperationException e)
                    {
                        Server.Clients.Remove(client);
                        Console.WriteLine(e.Message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                ProcessThreadCollection currentThreads = Process.GetCurrentProcess().Threads;
            }
        }

        private async Task ReadFromClient(ClientOnServerSide client)
        {

            StringBuilder message = new StringBuilder();
            var streamParts = new byte[Server.BufferSize];

            NetworkStream network = client.TcpClient.GetStream();

            if (!network.DataAvailable)
                return;

            try
            {
                do
                {
                    int numberOfBytesRead = await network.ReadAsync(streamParts, 0, streamParts.Length);
                    message.AppendFormat("{0}", Encoding.ASCII.GetString(streamParts, 0, numberOfBytesRead));
                } while (!ReachedTheEndOfTheMessage(message));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            var chatMessage = message.ToString();
            if (chatMessage != "")
            {
                chatMessage = FilteMessageWithourSignature(chatMessage);
                HandleIncomingMessage(chatMessage, client);
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //Send message command for the UI
        public ICommand SendMessageCommand
        {
            get { return new DelegateCommand<object>(SendMessageFunc, CanSendMessageFun); }
        }

        //Make the button available when the server is active and have at least one client.
        private bool CanSendMessageFun(object context)
        {
            return (Server.Active && Server.Clients.Count > 0);
        }

        //Write to all clients.
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

        //Create new Mssage object.
        private Message CreateMessage(string text, string senderName = "Server") => new Message { SenderName = senderName, Text = text };

        //Decide what to do with inconming message.
        private void HandleIncomingMessage(string message, ClientOnServerSide client)
        {
            var messageToCheck = message.ToLower();
            switch (messageToCheck) 
            { 
                case NOTsApplicationProtocol.LEAVE_CHAT_SECRET_WORD:
                    Task.Run(() => RedirectToClients(client, $"{client.Name} Just leaved")).Wait();
                    uiContext.Send(x=> Server.Messages.Add(CreateMessage($"{client.Name} just leaved")), null);
                    uiContext.Send(x=> Server.Clients.Remove(client), null);
                    break;
                default:
                    Task.Run(() => RedirectToClients(client, $"{client.Name} said ' {message} ' ")).Wait();
                    uiContext.Send(x=> Server.Messages.Add(CreateMessage(message, client.Name)), null);

                    break;
            }
        }

        //Redirect the incoming message to the all clients.
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

        // close the connection when the window closed. 
        async void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            await Task.Run(CLoseConnection);
        }


    }
}
