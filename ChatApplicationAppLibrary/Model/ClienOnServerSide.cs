using System;
using System.Net.Sockets;

namespace ChatApplicationAppLibrary.Model
{
    //This class is only used in the server side application 
    public class ClientOnServerSide
    {
        public Guid Id { get; set; }
        public TcpClient TcpClient { get; set; }
        public string Name { get; set; }
    }
}