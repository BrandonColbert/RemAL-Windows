using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RemAL {
	class LanManager : WiFiManager {
		private UdpClient udpListener;

		public LanManager(int port) : base(port) {

		}

		public override void Enable() {
			base.Enable();

			//Broadcast lan port
			new Task(async () => {
				if(udpListener != null)
					udpListener.Dispose();

				RemalUtils.Log("Broadcasting to " + GetName() + " on port " + port);

				while(isRunning) {
					using(udpListener = new UdpClient(port)) {
						try {
							udpListener.EnableBroadcast = true;
							var data = await udpListener.ReceiveAsync();

							if(isRunning) {
								var request = Encoding.ASCII.GetString(data.Buffer);
								Socket client = udpListener.Client;

								if(request == "REMAL_DETECT") {
									IPAddress address = (data.RemoteEndPoint as IPEndPoint).Address;
									RemalUtils.Log("Discovered by " + address.ToString() + ", notifying...");

									client.Connect(data.RemoteEndPoint);
									client.Send(Encoding.ASCII.GetBytes("REMAL_DETECT"));
									client.Shutdown(SocketShutdown.Both);
								}
							}
						} catch(ObjectDisposedException) {}
					}
				}
			}).Start();
		}

		public override void Disable() {
			base.Disable();
			RemalUtils.Log("Stopped " + GetName() + " broadcast");
		}

		public override string GetName() {
			return "LAN";
		}
	}
}