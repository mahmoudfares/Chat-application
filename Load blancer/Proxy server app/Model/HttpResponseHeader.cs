using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy_server_app.Model
{
    class HttpResponseHeader
    {
        string headerString;
        string[] headers;

        public HttpResponseHeader(string responseString)
        {
            headerString = responseString;
            headerString = headerString.Substring(0, headerString.IndexOf("\r\n\r\n"));
            string[] seperator = { "\r\n" };
            headers = headerString.Split(seperator, StringSplitOptions.None);
        }

        public bool IsNotModified
        {
            get
            {
                return headers[0].Contains("Not Modified");
            }
        }

        public string ETag
        {
            get{
                string ETagString = headers.First(x => x.Contains("ETag:"));
                var startIndex = ETagString.IndexOf(":") + 1;
                var eTag = ETagString.Substring(startIndex);
                return eTag.Trim();
            }
            
        }
        
        public override string ToString()
        {
            return string.Join("\r", headers);
        }
    }
}
