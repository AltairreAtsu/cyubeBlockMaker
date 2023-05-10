using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyubeBlockMaker
{
	internal class NewFileOption : IControlMenuOption
	{
		public void Execute()
		{
			MainWindow.mainWindow.ResetWindow();
		}

		public string GetName()
		{
			return "New";
		}
	}
}
