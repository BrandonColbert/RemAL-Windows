using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Networking;
using Windows.Networking.Connectivity;

namespace RemAL {
	struct LogData {
		public string text;
		public int occurences;

		public LogData(string text) {
			this.text = text;
			this.occurences = 0;
		}
	}

	class RemalUtils {
		private const int MAX_LOG = 1000;
		private static List<LogData> logList = new List<LogData>();
		public delegate void OnLogEvent(String msg);
		public static event OnLogEvent LogEvent;

		public static void Log(Object msg) {
			string s = msg.ToString();
			Debug.WriteLine(s);

			LogData d;

			if(logList.Count > 0 && (d = logList[0]).text == s) {
				d.occurences++;
				logList[0] = d;
			} else {
				logList.Insert(0, new LogData(s));

				if(logList.Count > MAX_LOG)
					logList.RemoveAt(MAX_LOG - 1);
			}

			LogEvent(s);
		}

		public static LogData[] GetLog() {
			return logList.ToArray();
		}

		public static string GetLocalAddress() {
			return NetworkInformation.GetHostNames().FirstOrDefault(hostname => hostname.IPInformation != null && hostname.Type == HostNameType.Ipv4).ToString();
		}

		public static void createTile(String path) {
			RemalUtils.Log("Create tile for: " + path);
		}
	}
}
