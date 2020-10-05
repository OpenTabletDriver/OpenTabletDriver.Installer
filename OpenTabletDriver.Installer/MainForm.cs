using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Eto.Drawing;
using Eto.Forms;
using InstallerLib;
using InstallerLib.Info;
using InstallerLib.Migration;
using OpenTabletDriver.Installer.Tools;

namespace OpenTabletDriver.Installer
{
    public partial class MainForm : Form
	{
		public MainForm()
		{
			Title = "OpenTabletDriver Updater";
			ClientSize = new Size(400, 350);
			Icon = App.Logo.WithSize(App.Logo.Size);

			this.status = new StackLayout()
			{
				Orientation = Orientation.Vertical,
				VerticalContentAlignment = VerticalAlignment.Center,
				HorizontalContentAlignment = HorizontalAlignment.Center,
				Padding = 10,
				Spacing = 5,
			};

			var showFolder = new Command { MenuText = "Show install folder...", ToolBarText = "Show install folder" };
			showFolder.Executed += (sender, e) => SystemInfo.Open(InstallationInfo.Current.InstallationDirectory);

			var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += (sender, e) => Application.Instance.Quit();

			this.startButton = new Button((sender, e) => Start())
			{
				Text = "Start"
			};

			this.installButton = new Button(async (sender, e) => await Install())
			{
				Text = "Install"
			};

			this.uninstallButton = new Button((sender, e) => Uninstall())
			{
				Text = "Uninstall"
			};
			
			this.updateButton = new Button(async (sender, e) => await Install(isUpdate: true))
			{
				Text = "Update"
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

			PerformMigration();
			UpdateControls(autostart: true);
		}

		public async void UpdateControls(bool autostart = false)
		{
			if (!shownInstallerUpdate && await InstallerUpdater.CheckForUpdate())
			{
				var result = MessageBox.Show(
					"An update is available for the installer." + Environment.NewLine +
					"Do you wish to be directed to the latest release?",
					"Installer Update",
					MessageBoxButtons.YesNo,
					MessageBoxType.Information
				);
				switch (result)
				{
					case DialogResult.Yes:
                        SystemInfo.Open(GitHubInfo.InstallerReleaseUrl);
						Application.Instance.Quit();
						break;
				}
				autostart = false;
				shownInstallerUpdate = true;
			}

			bool installed = App.Current.Installer.IsInstalled;
			bool update = await App.Current.Installer.CheckForUpdate();
			
			var buttons = new List<Button>
			{
				installed ? this.uninstallButton : this.installButton,
				update & installed ? this.updateButton : null,
				installed ? startButton : null
			};

			var buttonPanel = new StackLayout
			{
				Orientation = Orientation.Horizontal,
				VerticalContentAlignment = VerticalAlignment.Center,
				HorizontalContentAlignment = HorizontalAlignment.Center,
				Spacing = 5,
				Padding = 5
			};
			foreach (var button in buttons)
				if (button != null)
					buttonPanel.Items.Add(button);

			this.Content = new StackLayout
			{
				Padding = 10,
				Items =
				{
					new StackLayoutItem(null, true),
					new StackLayoutItem(new Bitmap(App.Logo.WithSize(128, 128)), HorizontalAlignment.Center),
					new StackLayoutItem(status, HorizontalAlignment.Center),
					new StackLayoutItem(buttonPanel, HorizontalAlignment.Center),
					new StackLayoutItem(null, true)
				}
			};

			status.Items.Clear();
			if (installed)
			{
				var version = App.Current.Installer.GetInstalledVersion();
				status.Items.Add($"OpenTabletDriver {version?.InstalledVersion} is installed.");
				if (update)
					this.status.Items.Add("An update is available.");
			}
			else
			{
				status.Items.Add("OpenTabletDriver is not installed.");
			}

			if (autostart & installed & !update)
			{
				Start();
			}
		}

		private void PerformMigration()
		{
			var binaryMigrator = new BinaryFolderMigrator();
			if (binaryMigrator.NeedsMigration)
			{
				var result = MessageBox.Show(
					binaryMigrator.MigrationPrompt,
					"Migration",
					MessageBoxButtons.YesNo,
					MessageBoxType.Question
				);
				switch (result)
				{
					case DialogResult.Yes:
						binaryMigrator.Migrate();
						break;
					default:
						Environment.Exit(0);
						break; 
				}
			}
		}

		private Button startButton, installButton, uninstallButton, updateButton;
		private StackLayout status;
		private bool shownInstallerUpdate;

		public async Task Install(bool isUpdate = false)
		{
			using (new DisabledControls(startButton, installButton, uninstallButton, updateButton))
			{
				var installProgress = new ProgressBar
				{
					MinValue = 0,
					MaxValue = 100
				};
				App.Current.Installer.ProgressChanged += (sender, progress) => installProgress.Value = (int)(100 * progress);
				SetStatus("Installing...", installProgress);

				if (!await App.Current.Installer.Install())
				{
					var rateLimit = await Downloader.GetRateLimit();
					var resetTime = rateLimit.Resources.Core.Reset.ToLocalTime().DateTime;
					await ShowRateLimitError(isUpdate ? "update" : "install", resetTime);
				}
				UpdateControls();
			}
		}

		public void Uninstall()
		{
			using (new DisabledControls(startButton, installButton, uninstallButton, updateButton))
			{
				var uninstallProgress = new ProgressBar
				{
					MinValue = 0,
					MaxValue = 100
				};
				App.Current.Installer.ProgressChanged += (sender, progress) => uninstallProgress.Value = (int)(100 * progress);
				SetStatus("Uninstalling...", uninstallProgress);

				App.Current.Installer.Uninstall();
				UpdateControls();
			}
		}

		private void SetStatus(params Control[] controls)
		{
			status.Items.Clear();
			foreach (var control in controls)
				status.Items.Add(control);
		}

		public void Hide()
		{
			Application.Instance.AsyncInvoke(() => 
			{
				this.Minimize();
				this.ShowInTaskbar = false;
				this.Visible = false;
			});
		}

		public void Unhide()
		{
			Application.Instance.AsyncInvoke(async () => 
			{
				this.Visible = true;
				this.ShowInTaskbar = true;
				await Task.Delay(100);
				this.BringToFront();
			});
		}

		private async Task ShowRateLimitError(string action, DateTime resetTime)
		{
			var rateLimit = await Downloader.GetRateLimit();
			MessageBox.Show(
				$"Failed to {action} because you are currently rate limited on the GitHub API." + Environment.NewLine + 
				$"You will be able to {action} after {resetTime}.",
				MessageBoxType.Warning
			);
		}

		private void Start()
		{
			Hide();
			App.Current.Launcher.Start();
			var watchdog = new Timer(TimeSpan.FromSeconds(1).TotalMilliseconds);
			watchdog.Elapsed += (sender, e) => 
			{
				if (App.Current.Launcher.AppProcess.Process.HasExited)
				{
					watchdog.Stop();
					watchdog.Dispose();
					Unhide();
				}
			};
			watchdog.Start();
		}
	}
}
