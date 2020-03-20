using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy_server_app.Model
{
    class HttpRequestHeader
    {
        string[] headers { get; set; }

        public HttpRequestHeader(string headerString)
        {
            string[] seperator = { "\r\n" };
            headers = headerString.Split(seperator, StringSplitOptions.None);
        }

        public string Host { 
            get
            {
                string hostString = headers.First(x => x.Contains("Host:"));
                var startIndex = hostString.IndexOf(":") + 1;
                var host = hostString.Substring(startIndex);
                host = host.Trim();
                return (host.Contains("localhost") || host.StartsWith("www") || host.StartsWith("ocsp")) ? host : "www." + host;
            }
        }

        public string AcceptTypes
        {
            get
            { 
                var acceptTypesString = headers.First(x => x.Contains("Accept:"));
                return acceptTypesString;
            }
        }

        public string UrlAddress
        {
            get
            {
                string statusLine = headers[0];
                int startIndex = statusLine.IndexOf("http");
                int endIndex = statusLine.IndexOf("HTTP");
                int urlLength = endIndex - startIndex;
                return statusLine.Substring(startIndex, urlLength);
            }
        }
        public bool AcceptTypeIsVideoOrImage => (AcceptTypes.Contains("video") || AcceptTypes.Contains("image")) && !AcceptTypes.Contains("html");

        public void HideUserAgent()
        {
            for (int i = 0; i < headers.Length; i++)
            {
                if (headers[i].Contains("User-Agent"))
                {
                    headers[i] = "User-Agent: Unknown";
                }
            }

        }
        
        public string UserAgent
        {
            get
            {
                return headers.First(x => x.Contains("User-Agent"));
            }
        }

        public override string ToString()
        {
            return string.Join("\r\n", headers);
        }

    }
}
