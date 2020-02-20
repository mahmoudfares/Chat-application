using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApplicationAppLibrary.Model
{
    public class Message
    {
        public string Text { get; set; }
        public string SenderName { get; set; }
        public string WPFMessage { get => $"From {SenderName}:  {Text}"; }
    }
}
