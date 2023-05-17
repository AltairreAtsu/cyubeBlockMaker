﻿using System;
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
			if (dirPath == MainWindow.WORKSPACE_ROOT) return;
			this.dirPath = dirPath;
			MenuItem renameOption = new MenuItem();
			renameOption.Header = "Rename";
			renameOption.Click += RenameContext_Click;

			MenuItem deleteOption = new MenuItem();
			deleteOption.Header = "Delete";
			deleteOption.Click += DeleteContext_Click;

			ContextMenu contextMenu = new ContextMenu();
			contextMenu.Items.Add(renameOption);
			contextMenu.Items.Add(deleteOption);
			this.ContextMenu = contextMenu;
		}

		public void RenameContext_Click(object sender, RoutedEventArgs e)
		{
			TextPrompt textPrompt = new TextPrompt();
			var result = textPrompt.ShowDialog();
			if(result == true)
			{
				if(textPrompt.UserText != string.Empty)
				{
					//Rename Dir to rename the node and header
					string newPath = Path.GetDirectoryName(dirPath) + "\\" + textPrompt.UserText;
					Directory.Move(dirPath, newPath);
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
	}


}
