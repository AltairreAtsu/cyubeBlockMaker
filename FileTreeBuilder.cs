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
	public class FileTreeBuilder
	{
		public static TreeNode CompileFileTree(string rootDir, string WORKSPACE_NAME, string WORKSPACE_ROOT, TreeView outliner)
		{
			Dictionary<string, TreeViewDir> outlinerChildren = new Dictionary<string, TreeViewDir>();
			TreeViewDir item = new TreeViewDir(WORKSPACE_ROOT);

			TreeNode fileTree = new TreeNode(new FileNode(WORKSPACE_NAME, true));

			outlinerChildren.Add(WORKSPACE_ROOT + "\\", item);
			item.Header = "Workspace";
			item.IsExpanded = true;
			outliner.Items.Add(item);

			fileTree.Item.OutlinerEntry = item;

			// Begin Dir Search
			DirSearch(WORKSPACE_ROOT + "\\", outlinerChildren, fileTree);

			return fileTree;
		}
		private static void DirSearch(string sDir, Dictionary<string, TreeViewDir> outlinerChildren, TreeNode fileTree)
		{
			try
			{
				TreeNode node = null;

				foreach (string d in Directory.GetDirectories(sDir))
				{
					TreeViewDir item = new TreeViewDir(d);
					var dirInfo = new DirectoryInfo(d);
					if (dirInfo.Name == "Textures") return;

					// Dependant on Main Window
					var parentNode = fileTree.GetNodeFromPath(MainWindow.mainWindow.GetWorkspaceRelativePath(dirInfo.Parent.FullName));
					node = parentNode.AddChild(new FileNode(dirInfo.Name, true));

					bool isBlockFolder = false;
					foreach (string dd in Directory.GetDirectories(d))
					{
						if (new DirectoryInfo(dd).Name == "Textures") isBlockFolder = true;
					}
					if (isBlockFolder)
					{
						node.Item.containsBlock = true;
					}
					else
					{
						item.Header = dirInfo.Name;
						
						outlinerChildren.Add(d + "\\", item);
						outlinerChildren[Directory.GetParent(d).FullName + "\\"].Items.Add(item);

						node.Item.OutlinerEntry = item;
					}

					foreach (string f in Directory.GetFiles(d))
					{
						if (PathIsBlockFile(f))
						{
							var child = node.AddChild(new FileNode(Path.GetFileName(f), false));

							BlockLabel label = new BlockLabel();
							label.filePath = f;
							label.SetBlockName(Path.GetFileNameWithoutExtension(f));

							TreeViewDir parentItem = outlinerChildren[Directory.GetParent(d).FullName + "\\"];
							parentItem.Items.Add(label);
							child.Item.OutlinerEntry = label;
						}
					}
					DirSearch(d, outlinerChildren, fileTree);
				}
			}
			catch (System.Exception excpt)
			{
				// Dependant on Main Window
				MainWindow.mainWindow.DisplayErrorMessage(excpt.Message);
				return;
			}
		}

		private static bool PathIsBlockFile(string path)
		{
			return System.IO.Path.GetExtension(path) == ".block";
		}
	}
}
