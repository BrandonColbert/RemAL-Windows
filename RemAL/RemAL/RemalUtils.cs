using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace RemAL {
	public struct LogData {
		public string text;
		public int occurences;

		public LogData(string text) {
			this.text = text;
			this.occurences = 0;
		}
	}

	public class RemalUtils {
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

		public static void CreateTile(string path) {
			Log("Creating tile for: " + path);
			
			CreateTile(path, MainWindow.usbman);
			CreateTile(path, MainWindow.lanman);
			CreateTile(path, MainWindow.blueman);
			CreateTile(path, MainWindow.wifiman);
			CreateTile(path, MainWindow.sshman);
		}

		private static void CreateTile(string path, ConnectionManager man) {
			if(man != null && man.IsActive())
				man.CreateTile(path);
		}

		public static void OnMessage(string msg) {
			string[] request = msg.Split(new char[] { ':' }, 2);

			if(request.Length == 2) {
				string type = request[0], details = request[1];

				switch(type) {
					case "app":
						break;
					case "path": {
						details = details.Replace('/', '\\');

						Log("Received: " + type + " - " + details);

						if(File.Exists(details))
							executeShellCommand("\"" + details + "\"");
						else if(Directory.Exists(details))
								executeShellCommand("explorer.exe \"" + details + "\"");
							else
							Log("Unable to find a file/folder at '" + details + "'");

						break;
					}
					case "macro":
						break;
					case "shell": {
						executeShellCommand(details);

						break;
					}
				}
			} else {
				Log("Received Strange: " + msg);
			}
		}

		public static void executeShellCommand(string command) {
			ProcessStartInfo p = new ProcessStartInfo();
			p.CreateNoWindow = true;
			p.FileName = "cmd.exe";
			p.Arguments = "/c " + command;
			p.WindowStyle = ProcessWindowStyle.Hidden;
			Process.Start(p);
		}
	}
}
