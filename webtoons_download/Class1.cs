using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webtoons_download
{
	class Class1
	{
		public class Stat
		{
			public string type { get; set; }
			public string url { get; set; }
		}

		public class Url
		{
			public string type { get; set; }
			public string url { get; set; }
		}

		public class AudioPlayUrlInfo
		{
			public string encodingOptionId { get; set; }
			public List<Url> urls { get; set; }
		}

		public class RootObject
		{
			public string code { get; set; }
			public string message { get; set; }
			public string audioId { get; set; }
			public double playTime { get; set; }
			public int playCount { get; set; }
			public string waveform { get; set; }
			public int serviceId { get; set; }
			public string cdn { get; set; }
			public string countryCode { get; set; }
			public List<Stat> stats { get; set; }
			public List<AudioPlayUrlInfo> audioPlayUrlInfos { get; set; }
		}
	}
}
