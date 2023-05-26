using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CyubeBlockMaker
{
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		public SettingsManager SettingsManager;


		public SettingsWindow(SettingsManager settingsManager)
		{
			InitializeComponent();
			SettingsManager = settingsManager;
			CreatorNamePreFill_TextBox.Text = settingsManager.PreFillCreatorName;
			SupressNoRecipeWarning_CheckBox.IsChecked = settingsManager.SupressNoRecipeWarning;
			if(settingsManager.UserHasSelectedImageApp)
				AppName_Label.Content = System.IO.Path.GetFileNameWithoutExtension(settingsManager.ImageAppPath);
		}

		private void CreatorNamePreFill_TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			SettingsManager.PreFillCreatorName = CreatorNamePreFill_TextBox.Text;
		}

		private void SupressNoRecipeWarning_CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			SettingsManager.SupressNoRecipeWarning = SupressNoRecipeWarning_CheckBox.IsChecked.Value;
		}
		private void SupressNoRecipeWarning_CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			SettingsManager.SupressNoRecipeWarning = SupressNoRecipeWarning_CheckBox.IsChecked.Value;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			SettingsManager.SaveSettings();
			MainWindow.mainWindow.ClosingSettingsWindow();
		}

		private void SelectApp_Button_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Multiselect = false;
			ofd.Title = "Select an Image App";
			ofd.Filter = "Executable | *.exe";
			if (ofd.ShowDialog() == true)
			{
				SettingsManager.ImageAppPath = ofd.FileName;
				AppName_Label.Content = System.IO.Path.GetFileNameWithoutExtension(ofd.FileName);
			}
		}
	}
}
