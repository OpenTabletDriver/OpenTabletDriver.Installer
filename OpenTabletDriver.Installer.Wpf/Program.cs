using System;
using Eto.Forms;

namespace OpenTabletDriver.Installer.Wpf
{
	class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			App.Current.Arguments = args;
			new Application(Eto.Platforms.Wpf).Run(new MainForm());
		}
	}
}
