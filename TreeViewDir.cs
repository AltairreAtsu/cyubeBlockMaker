using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CyubeBlockMaker
{
	public class TreeViewDir : TreeViewItem
	{
		private int sortPriority = 0;
		public string dirPath = "";
		public int SortPriority{ get {return sortPriority; }}

		public TreeViewDir(string dirPath)
		{
			this.dirPath = dirPath;
			ContextMenu contextMenu = new ContextMenu();
			this.ContextMenu = contextMenu;

			MenuItem createOption = new MenuItem();
			createOption.Header = "New";
			createOption.Click += CreateContext_Click;
			contextMenu.Items.Add(createOption);

			if (dirPath == MainWindow.WORKSPACE_ROOT) return;
			MenuItem renameOption = new MenuItem();
			renameOption.Header = "Rename";
			renameOption.Click += RenameContext_Click;
			contextMenu.Items.Add(renameOption);

			MenuItem deleteOption = new MenuItem();
			deleteOption.Header = "Delete";
			deleteOption.Click += DeleteContext_Click;
			contextMenu.Items.Add(deleteOption);
		}

		public void RenameContext_Click(object sender, RoutedEventArgs e)
		{
			TextPrompt textPrompt = new TextPrompt("Rename " + Header, "Enter a new name:");
			var result = textPrompt.ShowDialog();
			if(result == true)
			{
				if(textPrompt.UserText != string.Empty)
				{
					
					string newPath = Path.GetDirectoryName(dirPath) + "\\" + textPrompt.UserText;
					if (Directory.Exists(newPath))
					{
						MessageBox.Show("Warning, a directory with that name already exists. Please use a different name or delete the other directory.");
					}
					else
					{
						Directory.Move(dirPath, newPath);
					}
				}
			}
		}

		public void DeleteContext_Click(object sender, RoutedEventArgs e)
		{
			DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
			if (MessageBox.Show("Really delete " + dirInfo.Name + "?", "Delete directory confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			{
				try
				{
					Directory.Delete(dirPath, true);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
				}
			}
		}

		public void CreateContext_Click (object sender, RoutedEventArgs e)
		{
			TextPrompt textPrompt = new TextPrompt("Create a Directory", "Enter a name:");
			var result = textPrompt.ShowDialog();
			if(result == true)
			{
				string newDirPath = dirPath + "\\" + textPrompt.UserText;
				if (Directory.Exists(newDirPath))
				{
					MessageBox.Show("Warning, a directory already exists with that name. Please try a different name or delete the other directory.");
				}
				else
				{
					Directory.CreateDirectory(newDirPath);
				}
			}
		}
	}


}
