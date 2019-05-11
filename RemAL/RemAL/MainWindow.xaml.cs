using System;
using System.Reflection;
using System.Windows;

namespace RemAL {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public static ConnectionManager usbman, lanman, blueman, wifiman, sshman;

		public MainWindow() {
			InitializeComponent();

			System.Windows.Forms.NotifyIcon icon = new System.Windows.Forms.NotifyIcon();
			icon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
			icon.Visible = true;
			icon.Click += (sender, args) => {
				Show();
				WindowState = WindowState.Normal;
			};


			RemalUtils.LogEvent += msg => Dispatcher.Invoke(() => {
				string block = "";

				foreach(LogData d in RemalUtils.GetLog())
					block += d.text + (d.occurences > 0 ? (" (x" + (1 + d.occurences) + ")") : "") + "\n";

				outputView.Text = "->" + block;
			});

			wifiman = new WiFiManager();
			lanman = new LanManager();
		}

		protected override void OnStateChanged(EventArgs e) {
			if(WindowState == WindowState.Minimized)
				Hide();

			base.OnStateChanged(e);
		}

		private void DragOver_TileCreate(object sender, DragEventArgs e) {
			e.Effects = DragDropEffects.Link;
			e.Handled = true;

			//e.DragUIOverride.Caption = "Create tile for app";
		}

		private void Drop_TileCreate(object sender, DragEventArgs e) {
			if(e.Data.GetDataPresent(DataFormats.FileDrop)) {
				string[] items = (string[])e.Data.GetData(DataFormats.FileDrop);

				foreach(string item in items)
					RemalUtils.CreateTile(item);
			}
		}

		private void Click_Con_USB(object sender, RoutedEventArgs e) {
			//if(checkboxUSB.IsChecked.Value) usbman.Enable();
			//else usbman.Disable();
		}

		private void Click_Con_WiFi(object sender, RoutedEventArgs e) {
			if(!lanman.IsActive()) {
				if(checkboxWiFi.IsChecked.Value) {
					((WiFiManager)wifiman).setPort(int.Parse(wifiPort.Text));
					wifiman.Enable();
				} else {
					wifiman.Disable();
				}
			} else {
				checkboxWiFi.IsChecked = !checkboxWiFi.IsChecked;
			}
		}

		private void Click_Con_LAN(object sender, RoutedEventArgs e) {
			if(!wifiman.IsActive()) {
				if(checkboxLAN.IsChecked.Value) {
					((LanManager)lanman).setPort(int.Parse(lanPort.Text));
					lanman.Enable();
				} else {
					lanman.Disable();
				}
			} else {
				checkboxLAN.IsChecked = !checkboxLAN.IsChecked;
			}
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
