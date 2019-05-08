using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Text;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RemAL {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
		private static TextBlock outputBlock;

		public MainPage() {
			InitializeComponent();

			outputBlock = outputView;
        }

		private void DragOver_TileCreate(object sender, DragEventArgs e) {
			e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Link;
			e.DragUIOverride.Caption = "Create tile for app";
		}

		private async void Drop_TileCreate(object sender, DragEventArgs e) {
			if(e.DataView.Contains(StandardDataFormats.StorageItems)) {
				var items = await e.DataView.GetStorageItemsAsync();

				if(items.Count > 0)
					createTile(items[0].Path);
			}
		}

		private void createTile(String path) {
			outputText("Create tile for: " + path);
		}

		public static void outputText(Object text) {
			if(outputBlock != null)
				outputBlock.Text = outputBlock.Text + "\n" + text.ToString();
		}

		private void Click_Con_USB(object sender, RoutedEventArgs e) {
			if(checkboxWiFi.IsChecked.Value) {
				
			} else {
				
			}
		}

		private async void Click_Con_WiFi(object sender, RoutedEventArgs e) {
			if(checkboxUSB.IsChecked.Value) {
				
			} else {
				var listener = new TcpListener(IPAddress.Any, 24545);

				outputText("Started listener on " + (listener.LocalEndpoint as IPEndPoint).Address.ToString());

				listener.Start();

				outputText("Accepting clients");

				TcpClient client = await listener.AcceptTcpClientAsync();

				outputText("Got connection from: " + (client.Client.RemoteEndPoint as IPEndPoint).Address);

				byte[] buffer = new byte[64];
				var data = await client.GetStream().ReadAsync(buffer, 0, buffer.Length);

				outputText(Encoding.ASCII.GetString(buffer));

			}
		}

		private async void Click_Con_LAN(object sender, RoutedEventArgs e) {
			if(checkboxLAN.IsChecked.Value) {
				LanManager.Start(24545);
			} else {
				LanManager.Stop();
			}
		}

		private void Click_Con_Bluetooth(object sender, RoutedEventArgs e) {
			if(checkboxBluetooth.IsChecked.Value) {

			} else {

			}
		}

		private string GetLocalAddress() {
			return NetworkInformation.GetHostNames().FirstOrDefault(hostname => hostname.IPInformation != null && hostname.Type == HostNameType.Ipv4).ToString();
		}
	}
}
