using System;
using System.Net.Sockets;

namespace ChatApplicationAppLibrary.Model
{
    public class ClientOnServerSide
    {
        public Guid Id { get; set; }
        public TcpClient TcpClient { get; set; }
        public string Name { get; set; }
    }
}