using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RemAL {
	public class WiFiManager : ConnectionManager {
		private const String HANDSHAKE = "REMAL_HANDSHAKE",
			REMAL_DC = "REMAL_DISCONNECT",
			TILE_CREATE = "TILE_CREATE";

		protected volatile bool isRunning;
		protected int port;
		protected TcpListener listener;
		protected Dictionary<string, TcpClient> connectedAddresses = new Dictionary<string, TcpClient>();

		public WiFiManager() {

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
							if(!connectedAddresses.ContainsKey(address)) {
								RemalUtils.Log("Connecting to " + address);

								var stream = client.GetStream();

								if(ReadMessage(stream).Equals(HANDSHAKE)) {
									WriteMessage(stream, HANDSHAKE);
									connectedAddresses.Add(address, client);
									RemalUtils.Log("Handshake with " + address + " succeeded");

									client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

									new Task(() => {
										String msg;

										while(isRunning && (msg = ReadMessage(stream)).Length > 0)
											RemalUtils.OnMessage(msg);

										connectedAddresses.Remove(address);
										RemalUtils.Log(address + " disconnected");
									}).Start();
								}
							} else {
								RemalUtils.Log("Received another connection request from " + address + "?");
							}
						}
					} catch(ObjectDisposedException) { }
				}
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

		public bool IsActive() {
			return isRunning;
		}

		public void CreateTile(string path) {
			foreach(KeyValuePair<String, TcpClient> p in connectedAddresses)
				WriteMessage(p.Value.GetStream(), TILE_CREATE + ":" + path);
		}
	}
}
