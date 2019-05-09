using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RemAL {
	class WiFiManager : ConnectionManager {
		private const String HANDSHAKE = "REMAL_HANDSHAKE",
			ACTION_VALID = "REMAL_ACTION_VALID",
			ACTION_INVALID = "REMAL_ACTION_INVALID",
			REMAL_DC = "REMAL_DISCONNECT";

		protected volatile bool isRunning;
		protected int port;
		protected TcpListener listener;
		protected List<string> connectedAddresses = new List<string>();

		public WiFiManager(int port) {
			setPort(port);
		}

		public void setPort(int port) {
			this.port = port;
		}

		public virtual void Enable() {
			isRunning = true;

			//Acquire new connections
			new Task(async () => {
				if(listener != null)
					listener.Stop();

				listener = new TcpListener(IPAddress.Any, port);
				listener.Start();
				RemalUtils.Log("Waiting for connections to port " + port);

				while(isRunning) {
					try {
						TcpClient client = await listener.AcceptTcpClientAsync();

						if(isRunning) {
							string address = (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString();
							if(!connectedAddresses.Contains(address)) {
								RemalUtils.Log("Connecting to " + address);

								var stream = client.GetStream();

								if(ReadMessage(stream).Equals(HANDSHAKE)) {
									WriteMessage(stream, HANDSHAKE);
									RemalUtils.Log("Handshake with " + address + " succeeded");

									new Task(() => {
										while(isRunning) {
											RemalUtils.Log("Waiting for msg");
											onMessage(ReadMessage(stream));
										}

									}).Start();
								}
							} else {
								RemalUtils.Log("Received another connection request from " + address + "?");
							}
						}
					} catch(ObjectDisposedException) {}
				}

				/*
				bool didHandshake = false, keepAlive = true;

				RemalUtils.Log("Waiting for handshake");

				new Task(() => {
					while(isRunning && keepAlive) {
						byte[] b = new byte[64];

						int length = client.Receive(b);
						onMessage(Encoding.ASCII.GetString(b, 0, length), ref didHandshake);
					}

					client.Shutdown(SocketShutdown.Both);
				}).Start();

				new Task(async () => {
					await Task.Delay(5000);

					if(!didHandshake) {
						keepAlive = false;
						RemalUtils.Log("Terminating for being alive too long");
					}
				}).Start();

				RemalUtils.Log(request);
				*/
			}).Start();
		}

		private void WriteMessage(NetworkStream stream, string msg) {
			byte[] data = Encoding.ASCII.GetBytes(msg + "\n");

			stream.Write(data, 0, data.Length);
			stream.Flush();
		}

		private string ReadMessage(NetworkStream stream) {
			byte[] requestLengthBuffer = new byte[4];
			stream.Read(requestLengthBuffer, 0, requestLengthBuffer.Length);

			if(BitConverter.IsLittleEndian)
				Array.Reverse(requestLengthBuffer);

			int length = BitConverter.ToInt32(requestLengthBuffer, 0);

			byte[] requestBuffer = new byte[length];
			stream.Read(requestBuffer, 0, length);

			return Encoding.ASCII.GetString(requestBuffer, 0, length);
		}

		public virtual void Disable() {
			isRunning = false;
			RemalUtils.Log("Stopped waiting for " + GetName() + " connections");
		}

		public virtual string GetName() {
			return "WiFi";
		}

		private void onMessage(string msg) {
			RemalUtils.Log("[RemAL] " + msg);
		}
	}
}
