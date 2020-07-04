using System;
using Eto.Forms;
using Eto.Drawing;
using System.Threading.Tasks;

namespace OpenTabletDriver.Installer
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			Title = "OpenTabletDriver Updater";
			ClientSize = new Size(400, 350);

			var updateStatus = new Panel()
			{
				Padding = 10
			};
			
			var installCommand = new Command { MenuText = "Install", ToolBarText = "Install", Shortcut = Application.Instance.CommonModifier | Keys.I };
			var uninstallCommand = new Command { MenuText = "Uninstall", ToolBarText = "Uninstall", Shortcut = Application.Instance.CommonModifier | Keys.I };

			installCommand.Executed += async (sender, e) => 
			{
				await App.Current.Installer.InstallBinaries();
				await UpdateInstallInfo(updateStatus, installCommand, uninstallCommand);
			};
			uninstallCommand.Executed += async (sender, e) =>
			{
				await App.Current.Installer.DeleteBinaries();
				await UpdateInstallInfo(updateStatus, installCommand, uninstallCommand);
			};

						var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += (sender, e) => Application.Instance.Quit();

			this.PreLoad += async (sender, e) => await UpdateInstallInfo(updateStatus, installCommand, uninstallCommand);

			Content = new TableLayout
			{
				Padding = 10,
				Rows =
				{
					new TableRow
					{
						Cells =
						{
							new TableCell(updateStatus, true)
						}
					}
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
						}
					},
				},
				ApplicationItems =
				{
					// application (OS X) or file menu (others)
				},
				QuitItem = quitCommand
			};

			// create toolbar
			ToolBar = new ToolBar
			{
				Items =
				{
					installCommand,
					uninstallCommand
				}
			};
		}

		private async Task UpdateInstallInfo(Panel view, Command installCommand, Command uninstallCommand)
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
			if (App.Current.Installer.IsInstalled && await App.Current.Installer.CheckForUpdate())
			{
				var updateBox = new StackLayoutItem
				{
					Control = "An update is available.",
					HorizontalAlignment = HorizontalAlignment.Center
				};
				control.Items.Add(updateBox);
			}
			
			installCommand.Enabled = !App.Current.Installer.IsInstalled;
			uninstallCommand.Enabled = App.Current.Installer.IsInstalled;

			view.Content = control;
		}
	}
}
