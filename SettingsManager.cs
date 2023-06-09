﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyubeBlockMaker
{
	public class SettingsManager
	{
		private AppSettings appSettings;
		public bool DoPreFillCreatorName { get { return appSettings.PreFillCreatorName != string.Empty; } }
		public bool SupressNoRecipeWarning { get { return appSettings.SupressNoRecipeWarning; } set { appSettings.SupressNoRecipeWarning = value; } }
		public string PreFillCreatorName { get { return appSettings.PreFillCreatorName; } set { appSettings.PreFillCreatorName = value; } }
		public bool UserHasSelectedImageApp { get { return appSettings.ImageAppPath != string.Empty; } }
		public string ImageAppPath { get { return appSettings.ImageAppPath; } set { appSettings.ImageAppPath = value; } }
		public bool AlwaysAutoGenerateBlankRecipe { get { return appSettings.AlwaysAutoGenerateBlankRecipe; } set { appSettings.AlwaysAutoGenerateBlankRecipe = value; } }
		public bool AlwaysAutoGenerateSmallAlbedo { get { return appSettings.AlwaysAutoGenerateSmallAlbedo; } set { appSettings.AlwaysAutoGenerateSmallAlbedo = value; } }
		public string CyubeInstallLocation { get { return appSettings.CyubeInstallLocation; } set { appSettings.CyubeInstallLocation = value; } }
		public bool UserHasFoundCyubeInstall { get { return appSettings.CyubeInstallLocation != string.Empty; } }

		public SettingsManager()
		{
			appSettings = TryReadSettingsFile();
		}

		public AppSettings TryReadSettingsFile()
		{
			if (File.Exists(MainWindow.SETTINGS_FILE))
			{
				appSettings = JsonManager.ReadAppSettings(MainWindow.SETTINGS_FILE);
				if (appSettings == null)
				{
					// Failed to read, delete then make new file
					File.Delete(MainWindow.SETTINGS_FILE);
					AppSettings appSettings = new AppSettings();
					JsonManager.WriteAppSettings(MainWindow.SETTINGS_FILE, appSettings);
					return appSettings;
				}
				else
				{
					// read correctly return the settings
					return appSettings;
				}
			}
			else
			{
				// If file doesn't exist make a new one and save it
				AppSettings appSettings = new AppSettings();
				JsonManager.WriteAppSettings(MainWindow.SETTINGS_FILE, appSettings);
				return appSettings;
			}
		}
		public void SaveSettings()
		{
			JsonManager.WriteAppSettings(MainWindow.SETTINGS_FILE, appSettings);
		}
	}
}
