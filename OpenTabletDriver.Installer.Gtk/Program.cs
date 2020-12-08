using System;
using Eto.Forms;

namespace OpenTabletDriver.Installer.Gtk
{
	class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			App.Current.Arguments = args;
			new Application(Eto.Platforms.Gtk).Run(new MainForm());
		}
	}
}
