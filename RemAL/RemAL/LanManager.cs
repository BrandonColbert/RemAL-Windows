using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RemAL {
	class LanManager {
		private const int DELAY_TIME = 100;
		private static bool isRunning;
		private static UdpClient listener;

		public static void Start(int port) {
			isRunning = true;

			if(listener == null) {
				new Task(async () => {
					listener = new UdpClient(port);
					listener.EnableBroadcast = true;

					Debug.WriteLine("Listening on " + (listener.Client.LocalEndPoint as IPEndPoint).Address);

					while(isRunning) {
						var data = await listener.ReceiveAsync();

						if(isRunning) {
							var request = Encoding.ASCII.GetString(data.Buffer);
							Socket client = listener.Client;

							client.Connect(data.RemoteEndPoint);

							Debug.WriteLine("Connected client");

							if(request == "REMAL_DETECT") {
								Debug.WriteLine("Sending detection");

								client.Send(Encoding.ASCII.GetBytes("REMAL_DETECT"));
							} else {
								bool didHandshake = false, keepAlive = true;

								Debug.WriteLine("Waiting for handshake");

								new Task(() => {
									while(isRunning && keepAlive) {
										byte[] b = new byte[64];

										int length = client.Receive(b);
										onMessage(Encoding.ASCII.GetString(b, 0, length), ref didHandshake);
									}

									client.Shutdown(SocketShutdown.Both);
									client.Dispose();
								}).Start();

								new Task(async () => {
									await Task.Delay(5000);

									if(!didHandshake) {
										keepAlive = false;
										Debug.WriteLine("Terminating for being alive too long");
									}
								}).Start();

								Debug.WriteLine(request);
							}
						}
					}

					listener.Dispose();
					listener = null;
				}).Start();
			}
		}

		public static void Stop() {
			isRunning = false;
		}

		private static void onMessage(string msg, ref bool didHandshake) {
			Debug.WriteLine("[RemAL] " + msg);
		}
	}
}