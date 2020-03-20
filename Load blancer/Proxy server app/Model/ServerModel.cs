using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Proxy_server_app.Model
{
    public class ServerModel : INotifyPropertyChanged
	{
		private int portNumber = 8080;
		public ObservableCollection<string> Log { get; set; } = new ObservableCollection<string>();
		private bool active = false;
		public ConcurrentDictionary<string, List<byte>> Cache { get; } = new ConcurrentDictionary<string, List<byte>>();

		public string StartStopButtonText => active ? "Stop" : "Start";

		public int PortNumber
		{
			get => portNumber;
			set { portNumber = value; OnPropertyChanged(); }
		}

		private int bufferSize = 1024;

		public int BufferSize
		{
			get => bufferSize;
			set {
				bufferSize = value;
				OnPropertyChanged();
			}
		}

		private int cacheTimout = 0;

		public int CacheTimeout	
		{
			get => cacheTimout;
			set { cacheTimout = value; OnPropertyChanged(); }
		}

		private bool contentFilter = false;

		public bool ContentFilter
		{
			get => contentFilter;
			set { contentFilter = value; OnPropertyChanged(); }
		}

		private bool requestHeadersLogging = false;

		public bool RequestHeadersLogging
		{
			get => requestHeadersLogging;
			set { requestHeadersLogging = value; OnPropertyChanged(); }
		}

		private bool responseHeaders = false;

		public bool ResponseHeadersLogging 
		{
			get => responseHeaders;
			set { responseHeaders = value; OnPropertyChanged();}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		//Updates the property in the UI.
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private bool userAgent = false;

		public bool HideUserAgent { get => userAgent; set { userAgent = value; OnPropertyChanged(); } }

		public bool Active { get => active; set {
				active = value;
				OnPropertyChanged("StartStopButtonText");
			} 
		}
	}
}
