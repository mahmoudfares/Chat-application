using Microsoft.VisualStudio.PlatformUI;
using Proxy_server_app.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Proxy_server_app.ViewModel
{
    public class ServerViewModel
    {
        TcpListener tcpListener;
        readonly SynchronizationContext uiContext = SynchronizationContext.Current;
        readonly Stopwatch stopwatch;
        

        public ServerModel Server { get; private set; }

        public ServerViewModel()
        {
            Server = new ServerModel();
            Server.Log.Add("Ready to start");
            stopwatch = new Stopwatch();
        }

        //Start & stop server Command for the UI
        public ICommand StartStopButtonCommand => new DelegateCommand<object>(StartStopListenFunc, CanStartStopListen);

        public ICommand ClearLogButtonCommand => new DelegateCommand<object>(ClearLogFunc, CanClearLog);

        bool CanClearLog(object context) => Server.Log.Count > 0;

        void ClearLogFunc(object context) => Server.Log.Clear();

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

                InitializeTcpListener();
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
                        await OpenChannel(tcpClient);
                    }
                });
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private async Task OpenChannel(TcpClient tcpClient)
        {
            if( stopwatch.ElapsedMilliseconds >= (Server.CacheTimeout * 1000) )
            {
                Server.Cache.Clear();
                stopwatch.Restart();
            }

            try
            {
                using (var clientNetwork = tcpClient.GetStream())
                {
                    var httpRequestHeader = new Model.HttpRequestHeader(await ReadRequest(clientNetwork));

                    if (Server.HideUserAgent)
                    {
                        httpRequestHeader.HideUserAgent();
                    }

                    if (Server.RequestHeadersLogging)
                        uiContext.Send(x => Server.Log.Add(httpRequestHeader.ToString()), null);

                    var serverClient = new TcpClient();
                    await serverClient.ConnectAsync(httpRequestHeader.Host, 80);

                    using (var network = serverClient.GetStream())
                    {
                        var requestToOutSideWorld = Encoding.UTF8.GetBytes(httpRequestHeader.ToString());

                        if (httpRequestHeader.AcceptTypeIsVideoOrImage && Server.ContentFilter)
                        { 
                            var placeHolder = Encoding.UTF8.GetBytes("Content filterd");
                            await clientNetwork.WriteAsync(placeHolder, 0, placeHolder.Length);
                            return; 
                        }

                        if (!Server.Cache.ContainsKey(httpRequestHeader.ToString()))
                        {
                            List<byte> content = new List<byte>();
                            await network.WriteAsync(requestToOutSideWorld, 0, requestToOutSideWorld.Length);
                            
                            var readedFromServerBytes = await ReadFromServerAndRedirectIfNeeded(network, clientNetwork);
                            var response = Encoding.ASCII.GetString(readedFromServerBytes.ToArray(), 0, readedFromServerBytes.Count);

                            content.AddRange(readedFromServerBytes);
                            SaveInCacheIfNeeded(httpRequestHeader, content);
                            LogResponseIfNeeded(response);
                        }
                        else
                        {
                            await WriteFromCach(clientNetwork, httpRequestHeader, network);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        async Task<List<byte>> ReadFromServerAndRedirectIfNeeded(NetworkStream serverNetwork, NetworkStream clientNetwork = null, int bufferSize = 0)
        {
            var maxBufferSize = bufferSize == 0 ? Server.BufferSize : bufferSize;
            var readFromServerParts = new byte[2500];
            var toReturnBytesList = new List<byte>();
            do
            {
                int numberOfBytesRead = await serverNetwork.ReadAsync(readFromServerParts, 0, maxBufferSize);
                if (clientNetwork != null)
                {
                    await clientNetwork.WriteAsync(readFromServerParts, 0, numberOfBytesRead);
                }
                toReturnBytesList.AddRange(readFromServerParts.Take(numberOfBytesRead));

            } while (serverNetwork.DataAvailable);

            return toReturnBytesList;
        }

        private async Task WriteFromCach(NetworkStream clientNetwork, Model.HttpRequestHeader httpClientRequestHeader, NetworkStream network)
        {
            Server.Cache.TryGetValue(httpClientRequestHeader.ToString(), out var content);

            uiContext.Send(x => Server.Log.Add("From cache"), null);

            if (content != null)
            {
                var toSendParts = content.ToArray();
                await clientNetwork.WriteAsync(toSendParts, 0, toSendParts.Length);
            }
        }

        /*
        private async Task<bool> ContentIsModifiedInServer(NetworkStream network, Model.HttpResponseHeader httpResponse, Model.HttpRequestHeader httpClientRequestHeader, NetworkStream clientNetwork )
        {
            var httpRequestHeader = new Model.HttpRequestHeader(CreateHttpRequestHeaderString(httpResponse.ETag, httpClientRequestHeader));
            var requestToOutSideWorld = Encoding.UTF8.GetBytes(httpRequestHeader.ToString());
            await network.WriteAsync(requestToOutSideWorld, 0, requestToOutSideWorld.Length);
            uiContext.Send(x => Server.Log.Add($"{httpRequestHeader.ToString()}"), null);
            var responseParts = await ReadFromServerAndRedirectIfNeeded(network, null, 2500);
            var responseString = Encoding.ASCII.GetString(responseParts.ToArray(), 0, responseParts.Count);
            var responseHeader = new Model.HttpResponseHeader(responseString);
            uiContext.Send(x => Server.Log.Add(responseHeader.ToString()), null);
            await clientNetwork.WriteAsync(responseParts.ToArray(), 0, responseParts.Count);
            Server.Cache.TryUpdate(httpClientRequestHeader.ToString(), responseParts, null);
            return !responseHeader.IsNotModified;
        }

        private string CreateHttpRequestHeaderString(string ETag, Model.HttpRequestHeader requestHeader)
        {
            //string lastTimeSaved = DateTime.Now.Subtract(stopwatch.Elapsed).ToString("ddd, dd MMM yyy HH:mm:ss");
            string lastTimeSaved = DateTime.Now.AddDays(-1).ToString("ddd, dd MMM yyy HH:mm:ss");

            return $"GET {requestHeader.UrlAddress}HTTP/1.0\r\n" +
                "Host: localhost\r\n" +
                $"{requestHeader.UserAgent}\r\n" +
                $"{requestHeader.AcceptTypes}\r\n" +
                "Accept-Language: en-US,en;q=0.5\r\n" +
                "Accept-Encoding: gzip, deflate\r\n" +
                "Connection: keep-alive\r\n" +
                $"Referer: http://localhost/test/index.html \r\n" +
                $"If-Modified-Since: {lastTimeSaved} GMT\r\n" +
                $"If-None-Match: {ETag}\r\n" +
                "Pragma: no-cache\r\n\r\n";
        }
        */

        private void SaveInCacheIfNeeded(Model.HttpRequestHeader httpHeader, List<byte> content)
        {
            if (Server.CacheTimeout == 0)return;

            Server.Cache.TryAdd(httpHeader.ToString(), content);

            if (Server.Cache.Count == 1 )
                stopwatch.Restart();
        }

        private void LogResponseIfNeeded(string response)
        {
            if (!Server.ResponseHeadersLogging) return;
            var httpResponse = new Model.HttpResponseHeader(response);
            uiContext.Send(x => Server.Log.Add(httpResponse.ToString()), null);
        }

        //Close the server.
        void CLoseConnection()
        {
            try
            {
                tcpListener.Stop();
                Server.Active = false;
                Server.Log.Add("Server disconnected");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        void InitializeTcpListener()
        {
            try
            {
                var ipAddress = Dns.Resolve("localhost").AddressList[0];
                tcpListener = new TcpListener(ipAddress, Server.PortNumber);
                tcpListener.Start();

                //Server.Messages.Add(CreateMessage("Waiting for clients"));
                Server.Active = true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Something went wrong {e.Message}");
            }
        }


        static async Task<string> ReadRequest(NetworkStream network)
        {
            var request = new StringBuilder();
            var streamParts = new byte[1024];
                do
                {
                    int numberOfBytesRead = await network.ReadAsync(streamParts, 0, streamParts.Length);
                    request.AppendFormat("{0}", Encoding.ASCII.GetString(streamParts, 0, numberOfBytesRead));

                } while (network.DataAvailable);
            return request.ToString();
        }

    }
}
