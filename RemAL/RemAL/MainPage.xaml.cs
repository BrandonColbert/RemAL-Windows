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
		private ConnectionManager usbman, lanman, blueman, wifiman, sshman;

		public MainPage() {
			InitializeComponent();

			RemalUtils.LogEvent += async msg => await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
				string block = "";

				foreach(LogData d in RemalUtils.GetLog())
					block += d.text + (d.occurences > 0 ?  (" (x" + (1 + d.occurences) + ")") : "") + "\n";

				outputView.Text = "->" + block;
			});

			wifiman = new WiFiManager(int.Parse(wifiPort.Text));
			lanman = new LanManager(int.Parse(lanPort.Text));
		}

		private void DragOver_TileCreate(object sender, DragEventArgs e) {
			e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Link;
			e.DragUIOverride.Caption = "Create tile for app";
		}

		private async void Drop_TileCreate(object sender, DragEventArgs e) {
			if(e.DataView.Contains(StandardDataFormats.StorageItems)) {
				var items = await e.DataView.GetStorageItemsAsync();

				if(items.Count > 0)
					RemalUtils.createTile(items[0].Path);
			}
		}

		private void Click_Con_USB(object sender, RoutedEventArgs e) {
			//if(checkboxUSB.IsChecked.Value) usbman.Enable();
			//else usbman.Disable();
		}

		private void Click_Con_WiFi(object sender, RoutedEventArgs e) {
			if(checkboxWiFi.IsChecked.Value) wifiman.Enable();
			else wifiman.Disable();
		}

		private void Click_Con_LAN(object sender, RoutedEventArgs e) {
			if(checkboxLAN.IsChecked.Value) lanman.Enable();
			else lanman.Disable();
		}

		private void Click_Con_Bluetooth(object sender, RoutedEventArgs e) {
			//if(checkboxBluetooth.IsChecked.Value) blueman.Enable();
			//else blueman.Disable();
		}

		private void Click_Con_SSHh(object sender, RoutedEventArgs e) {
			//if(checkboxSSH.IsChecked.Value) sshman.Enable();
			//else sshman.Disable();
		}
	}
}
