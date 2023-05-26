using Newtonsoft.Json.Bson;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CyubeBlockMaker
{
	/// <summary>
	/// Interaction logic for BlockLabel.xaml
	/// </summary>
	public partial class BlockLabel : UserControl
	{
		private string blockName;
		public string filePath;
		
		private int sortPriority = 1;
		public int SortPriority { get { return sortPriority; } }
		
		public BlockLabel()
		{
			InitializeComponent();
		}


		// Button Event Handling
		private void UserControl_MouseEnter(object sender, MouseEventArgs e)
		{
			Background = MainWindow.mouseOverButtonBackground;
        }
		private void UserControl_MouseLeave(object sender, MouseEventArgs e)
		{
			Background = MainWindow.defaultButtonBackground;
        }
		private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Background = MainWindow.mouseDownButtonBackground;
			if(e.ClickCount == 2)
				MainWindow.mainWindow.OpenBlock(filePath);
        }
		private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Background = MainWindow.mouseOverButtonBackground;
        }

		// Context Menu Event Handling
		private void Conext_Open_Click(object sender, RoutedEventArgs e)
		{
			MainWindow.mainWindow.OpenBlock(filePath);
		}

		private void Conext_OpenFolder_Click(object sender, RoutedEventArgs e)
		{
			MainWindow.mainWindow.OpenFileExplorer(System.IO.Path.GetDirectoryName(filePath));
		}

		private void Conext_Duplicate_Click(object sender, RoutedEventArgs e)
		{
			DirectoryInfo dirInfo = new DirectoryInfo(System.IO.Path.GetDirectoryName(filePath));
			string dirName = dirInfo.Name;
			string newDirName = dirInfo.Parent.FullName + "\\" + dirName + " Copy";
			string newBlockPath = "";
			int i = 0;
			while (Directory.Exists(newDirName))
			{
				newDirName = dirInfo.Parent.FullName + "\\" + dirName + " Copy" + i.ToString();
				i++;
			}
			Directory.CreateDirectory(newDirName);

			string[] files = Directory.GetFiles(dirInfo.FullName);
			foreach (string f in files)
			{
				File.Copy(f, newDirName+"\\" + System.IO.Path.GetFileName(f), true);
				if(System.IO.Path.GetFileNameWithoutExtension(f) == ".block")
				{
					newBlockPath = f;
				}
			}

			Directory.CreateDirectory(newDirName+"\\Textures");
			files = Directory.GetFiles(dirInfo.FullName + "\\Textures");
			foreach (string f in files)
			{
				File.Copy(f, newDirName + "\\Textures\\" + System.IO.Path.GetFileName(f));
			}

			var newBlock = MainWindow.mainWindow.workspaceManager.GetBlock(filePath);
			if( newBlock != null)
			{
				MainWindow.mainWindow.workspaceManager.AddBlock(newBlockPath, newBlock);
			}
		}

		private void Conext_Delete_Click(object sender, RoutedEventArgs e)
		{
			if(MessageBox.Show("Really delete " + blockName + "?", "Delete file confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			{
				try
				{
					if (MainWindow.mainWindow.BlockIsCurrentlyOpen(filePath))
					{
						MainWindow.mainWindow.ResetWindow();
						GC.Collect();
						GC.WaitForPendingFinalizers();
					}
					Directory.Delete(System.IO.Path.GetDirectoryName(filePath), true);
					var block = MainWindow.mainWindow.workspaceManager.GetBlock(filePath);
					if (block != null)
					{
						MainWindow.mainWindow.workspaceManager.RemoveBlock(filePath, block);
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
				}
			}
		}

		private void Conext_Rename_Click(object sender, RoutedEventArgs e)
		{
			//Open a rename window
			//Capture the result and apply it to the block label and it's underlying file
			TextPrompt prompt = new TextPrompt("Rename " + blockName, "Enter a new name:");
			var result = prompt.ShowDialog();
			if (result == true ){
				if (prompt.UserText != string.Empty)
				{
					string newPath = System.IO.Path.GetDirectoryName(filePath) + "\\" + prompt.UserText + ".block";
					File.Move(filePath, newPath);
					MainWindow.mainWindow.workspaceManager.OutlinerManager.RenameBlock(filePath, newPath);
				}
			}

		}

		// Getters and Setters
		public void SetBlockName(string blockName)
		{
			this.blockName = blockName;
			BlockNameLabel.Content = blockName;
		}
		public string GetBlockName()
		{
			return blockName;
		}
	}
}
