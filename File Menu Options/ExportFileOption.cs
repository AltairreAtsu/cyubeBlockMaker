using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyubeBlockMaker.File_Menu_Options
{
	public class ExportFileOption : IControlMenuOption
	{
		public void Execute()
		{
			MainWindow.mainWindow.Export();
		}

		public string GetName()
		{
			return "Export";
		}
	}
}
