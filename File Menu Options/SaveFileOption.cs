using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyubeBlockMaker
{
	internal class SaveFileOption : IControlMenuOption
	{
		public void Execute()
		{
			MainWindow.mainWindow.Save();
		}

		public string GetName()
		{
			return "Save";
		}
	}
}
