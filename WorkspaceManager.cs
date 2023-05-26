using Newtonsoft.Json.Bson;
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
	public class WorkspaceManager
	{
		public OutlinerManager OutlinerManager;
		public BlockRegistry BlockRegistry;


		public WorkspaceManager(TreeView Outliner) 
		{ 
			OutlinerManager = new OutlinerManager(Outliner);
			BlockRegistry = new BlockRegistry();
		}

		public void BuildWorkspace()
		{
			string[] blockFolders = Directory.GetDirectories(MainWindow.WORKSPACE_ROOT);


			foreach (string blockFolder in blockFolders)
			{
				string[] files = Directory.GetFiles(blockFolder);
				foreach (string file in files)
				{
					if (PathIsBlockFile(file))
					{
						var block = JsonManager.ReadJson(file);
						if (block == null)
						{
							MessageBox.Show("Failed to read block at: " + file);
							continue;
						}
						AddBlock(file, block);
					}
				}
			}
		}

		public void AddBlock(string path, CustomBlock block)
		{
			if(!OutlinerManager.BlockExists(path))
			{
				OutlinerManager.AddBlock(path, block.CategoryName);
				BlockRegistry.AddBlock(path, block);
			}
		}
		public void RemoveBlock(string path, CustomBlock block)
		{
			OutlinerManager.RemoveBlock(path);
			BlockRegistry.RemoveBlock(path, block);
		}
		
		public void SortOutliner()
		{
			OutlinerManager.SortOutliner();
		}

		public bool UniqueIDInUse(int uid)
		{
			return BlockRegistry.ContainsUniqueID(uid);
		}
		public bool UniqueIDInUse(int uid, string filePath)
		{
			return BlockRegistry.ContainsUniqueID(uid, filePath);
		}
		public bool BlockExists(string path)
		{
			return OutlinerManager.BlockExists(path);
		}
		public CustomBlock GetBlock(string path)
		{
			return BlockRegistry.GetBlock(path);
		}
	
		private bool PathIsBlockFile(string path)
		{
			return System.IO.Path.GetExtension(path) == ".block";
		}
	}	
}
