using Microsoft.Win32;
using System.Windows;

namespace CyubeBlockMaker
{
	internal class OpenFileOption : IControlMenuOption
	{
		public void Execute()
		{
			OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.Filter = "JSON (*.json)|*.json";
			fileDialog.Title = "Select a Custom Block to open";

			if (fileDialog.ShowDialog() == true)
			{
				string filePath = fileDialog.FileName;
				CustomBlock? block = JsonManager.ReadJson(filePath);
				if (block == null)
				{
					MessageBox.Show("Error Loading the specified  file, please try again.");
					return;
				}
				MainWindow.mainWindow.LoadCustomBlock(block);
			}
		}

		public string GetName()
		{
			return "Open";
		}
	}
}
