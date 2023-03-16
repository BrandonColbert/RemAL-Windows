using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace RemAL {
	public class BluetoothManager : ConnectionManager {
		private static readonly Guid ID = Guid.Parse("228e1e8b-745b-4754-8c9d-8efb6b21189b");
		private volatile bool running;
		private RfcommServiceProvider provider;

		public void Disable() {
			if(running) {
				running = false;
				provider.StopAdvertising();
			}
		}

		public async void Enable() {
			if(!running) {
				running = true;

				RemalUtils.Log("Creating rfcomm provider");
				provider = await RfcommServiceProvider.CreateAsync(RfcommServiceId.FromUuid(ID));

				RemalUtils.Log("Preparing to listen");
				using(var listener = new StreamSocketListener()) {
					listener.ConnectionReceived += (listen, args) => {
						RemalUtils.Log("Received connection");

						var socket = args.Socket;
						DataReader reader = new DataReader(socket.InputStream);
						DataWriter writer = new DataWriter(socket.OutputStream);

						new Task(() => {
							String msg;

							while(running && (msg = ReadMessage(reader)).Length > 0)
								RemalUtils.OnMessage(msg);


						}).Start();
					};

					RemalUtils.Log("Binding...");
					await listener.BindServiceNameAsync(provider.ServiceId.AsString(), SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);
					RemalUtils.Log("Advertising...");
					provider.StartAdvertising(listener);
				}
			}
		}

		public void CreateTile(string path) {

		}

		private async void WriteMessage(DataWriter stream, string msg) {
			byte[] data = Encoding.ASCII.GetBytes(msg + "\n");
			
			stream.WriteBytes(data);
			await stream.FlushAsync();
		}

		private string ReadMessage(DataReader stream) {
			byte[] requestLengthBuffer = new byte[4];
			stream.ReadBytes(requestLengthBuffer);

			if(BitConverter.IsLittleEndian)
				Array.Reverse(requestLengthBuffer);

			int length = BitConverter.ToInt32(requestLengthBuffer, 0);

			byte[] requestBuffer = new byte[length];
			stream.ReadBytes(requestBuffer);

			return Encoding.ASCII.GetString(requestBuffer, 0, length);
		}

		public string GetName() {
			return "Bluetooth";
		}

		public bool IsActive() {
			return running;
		}
	}
}