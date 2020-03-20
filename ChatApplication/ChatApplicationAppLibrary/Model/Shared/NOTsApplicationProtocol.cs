using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApplicationAppLibrary.Model.Shared
{
    public static class NOTsApplicationProtocol
    {
        public const string END_OF_MESSAGE_SIGNATURE = "#end#";
        public const string LEAVE_CHAT_SECRET_WORD = "bye";
        public const string SERVER_CRASH_SECRET_WORD = "server-is-sad";
    }
}
