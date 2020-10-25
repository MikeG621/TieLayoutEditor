using System;
using System.Windows.Forms;

namespace Idmr.TieLayoutEditor
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
#pragma warning disable IDE1006 // Naming Styles
		static void Main()
#pragma warning restore IDE1006 // Naming Styles
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}
