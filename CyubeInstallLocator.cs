using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CyubeBlockMaker
{
	class CyubeInstallLocator
	{

		public LocationResult OpenSearchBroswer()
		{
			VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
			dialog.Multiselect = false;
			dialog.Description = "Please select your root CyubeVR folder from your steam folder!";
			var result = dialog.ShowDialog();
			if(result.HasValue && result.Value) 
			{
				if (IsCyubeRootFolder(dialog.SelectedPath))
				{
					return new LocationResult(dialog.SelectedPath, true);
				}
				else
				{
					if(MessageBox.Show("Cyube not detected in the selected folder. Be sure to select the folder that contains your cyubeVR.exe! Try again?", "Cyube folder not found!", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
					{
						return OpenSearchBroswer();
					}
					return new LocationResult("", false);
				}
			}
			else
			{
				return new LocationResult("", false);
			}
		}

		public LocationResult AttemptToAutoLocate()
		{
			string searchPathA = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\cyubeVR\\cyubeVR.exe";
			string searchPathB = "C:\\Program Files\\Steam\\steamapps\\common\\cyubeVR\\cyubeVR.exe";
			if(File.Exists(searchPathA))
			{
				return new LocationResult(Path.GetDirectoryName(searchPathA), true);
			}
			if(File.Exists(searchPathB))
			{
				return new LocationResult(Path.GetDirectoryName(searchPathB), true);
			}
			return new LocationResult("", false);
		}

		public bool IsCyubeRootFolder(string path)
		{
			string exePath = path + "\\cyubeVR.exe";
			if(File.Exists(exePath))
			{
				return true;
			}
			return false;
		}
	}

	public class LocationResult
	{
		public string path;
		public bool sucess;

		public LocationResult(string path, bool succes)
		{
			this.path = path;
			this.sucess = succes;
		}
	}
}
