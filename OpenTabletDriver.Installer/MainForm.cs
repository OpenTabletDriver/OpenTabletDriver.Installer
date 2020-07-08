using System;
using Eto.Forms;
using Eto.Drawing;
using System.Threading.Tasks;
using System.Timers;
using InstallerLib;

namespace OpenTabletDriver.Installer
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			Title = "OpenTabletDriver Updater";
			ClientSize = new Size(400, 350);
			Icon = App.Current.Logo.WithSize(App.Current.Logo.Size);

			Hide();

			var status = new Panel()
			{
				Padding = 10
			};

			var showFolder = new Command { MenuText = "Show install folder...", ToolBarText = "Show install folder" };
			showFolder.Executed += (sender, e) => InstallerLib.Platform.Open(App.Current.InstallationDirectory);

			var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += (sender, e) => Application.Instance.Quit();

			var installButton = new Button
			{
				Text = "Install"
			};
			installButton.Click += toggleInstallState;

			var buttonPanel = new StackLayout
			{
				Orientation = Orientation.Horizontal,
				VerticalContentAlignment = VerticalAlignment.Center,
				HorizontalContentAlignment = HorizontalAlignment.Center,
				Spacing = 5,
				Padding = 5,
				Items = 
				{
					installButton,
					new Button((sender, e) => StartDriver())
					{
						Text = "Start"
					}
				}
			};

			async Task updateInfoView() => await UpdateInstallInfo(status, installButton);
			async void toggleInstallState(object sender, EventArgs e)
			{
                if (!App.Current.Installer.IsInstalled)
					await App.Current.Installer.InstallBinaries();
                else
					await App.Current.Installer.DeleteBinaries();
				await updateInfoView();
            }

			this.PreLoad += async (sender, e) => await updateInfoView();

			Content = new StackLayout
			{
				Padding = 10,
				Items = 
				{
					new StackLayoutItem(buttonPanel, HorizontalAlignment.Center),
					new StackLayoutItem(status, HorizontalAlignment.Center)
				}
			};

			// create menu
			Menu = new MenuBar
			{
				Items =
				{
					// File submenu
					new ButtonMenuItem
					{ 
						Text = "&File",
						Items =
						{
							showFolder
						}
					},
				},
				ApplicationItems =
				{
					// application (OS X) or file menu (others)
				},
				QuitItem = quitCommand
			};

			Unhide();
		}

		private async Task UpdateInstallInfo(Panel view, Button installButton)
		{
			var control = new StackLayout
			{
				HorizontalContentAlignment = HorizontalAlignment.Center,
				VerticalContentAlignment = VerticalAlignment.Center,
				Items = 
				{
					new StackLayoutItem
					{
						Control = new ImageView
						{
							Image = App.Current.Logo,
							Size = new Size(150, 150)
						},
						HorizontalAlignment = HorizontalAlignment.Center
					},
					new StackLayoutItem
					{
						Control = $"OpenTabletDriver is {(App.Current.Installer.IsInstalled ? "" : "not ")}installed.",
						HorizontalAlignment = HorizontalAlignment.Center
					}
				}
			};

			var updateAvailable = await App.Current.Installer.CheckForUpdate();
			if (App.Current.Installer.IsInstalled && updateAvailable)
			{
				var updateBox = new StackLayoutItem
				{
					Control = "An update is available.",
					HorizontalAlignment = HorizontalAlignment.Center
				};
				control.Items.Add(updateBox);
			}

			using (var fs = App.Current.Installer.VersionInfoFile.OpenRead())
			{
				var ver = VersionInfo.Deserialize(fs);
				var versionCtrl = new StackLayoutItem
				{
					Control = ver.InstalledVersion,
					HorizontalAlignment = HorizontalAlignment.Center
				};
				control.Items.Add(versionCtrl);
			}
			
			view.Content = control;

			installButton.Text = App.Current.Installer.IsInstalled ? "Uninstall" : "Install";

			if (App.Current.Installer.IsInstalled && !updateAvailable)
				StartDriver();
		}

		public void Hide()
		{
			this.Minimize();
			this.ShowInTaskbar = false;
		}

		public void Unhide()
		{
			this.BringToFront();
			this.ShowInTaskbar = true;
		}

		private void StartDriver()
		{
			Hide();
			App.Current.Launcher.StartDaemon();
			App.Current.Launcher.StartApp();
			var watchdog = new Timer(TimeSpan.FromSeconds(1).TotalMilliseconds);
			watchdog.Elapsed += (sender, e) => 
			{
				if (App.Current.Launcher.AppProcess.Process.HasExited)
				{
					App.Current.Launcher.DaemonProcess.Stop();
					watchdog.Stop();
					watchdog.Dispose();
					Unhide();
				}
			};
			watchdog.Start();
		}
	}
}
