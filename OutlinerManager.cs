using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CyubeBlockMaker
{
	public class OutlinerManager
	{
		private TreeView Outliner;
		Dictionary<string, TreeViewDir> categories = new Dictionary<string, TreeViewDir>();
		Dictionary<string, BlockLabel> blocks = new Dictionary<string, BlockLabel>();

		public OutlinerManager(TreeView Outliner)
		{
			this.Outliner = Outliner;
		}

		public void SortOutliner()
		{
			Outliner.Items.SortDescriptions.Clear();
			Outliner.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("SortPriority", System.ComponentModel.ListSortDirection.Ascending));
			Outliner.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Header", System.ComponentModel.ListSortDirection.Ascending));
			Outliner.Items.Refresh();

			foreach (Object obj in Outliner.Items)
			{
				if (obj is TreeViewDir)
				{
					TreeViewDir treeViewDir = (TreeViewDir)obj;
					treeViewDir.Items.SortDescriptions.Clear();
					treeViewDir.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("SortPriority", System.ComponentModel.ListSortDirection.Ascending));
					treeViewDir.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Header", System.ComponentModel.ListSortDirection.Ascending));
					treeViewDir.Items.Refresh();
				}
			}
		}

		public void AddBlock(string path, string category)
		{
			if (category == string.Empty)
			{
				var label = CreateBlockLabel(path);
				Outliner.Items.Add(label);
			}
			else
			{
				if (categories.ContainsKey(category))
				{
					var label = CreateBlockLabel(path);
					categories[category].Items.Add(label);
				}
				else
				{
					TreeViewDir categoryNode = new TreeViewDir();
					categoryNode.Header = category;
					categories.Add(category, categoryNode);
					Outliner.Items.Add(categoryNode);

					var label = CreateBlockLabel(path);
					categoryNode.Items.Add(label);
				}
			}
		}
		public void AddBlock(string path)
		{
			AddBlock(path, "");
		}

		public bool RemoveBlock(string path)
		{
			if (blocks.ContainsKey(path))
			{
				TreeViewDir viewDir = (TreeViewDir)blocks[path].Parent;
				viewDir.Items.Remove(blocks[path]);
				blocks.Remove(path);

				if(viewDir.Items.Count == 0)
				{
					Outliner.Items.Remove(viewDir);
				}
				return true;
			}
			return false;
		}
		
		public bool RenameBlock(string path, string newPath)
		{
			if (blocks.ContainsKey(path))
			{
				blocks[path].filePath = newPath;
				blocks[path].SetBlockName(Path.GetFileNameWithoutExtension(newPath));
				blocks.Add(newPath, blocks[path]);
				blocks.Remove(path);
				return true;
			}
			return false;
		}

		private BlockLabel CreateBlockLabel(string path)
		{
			BlockLabel label = new BlockLabel();
			label.SetBlockName(Path.GetFileNameWithoutExtension(path));
			label.filePath = path;
			blocks.Add(path, label);
			return label;
		}

		public bool BlockExists(string path)
		{
			return blocks.ContainsKey(path);
		}


	}
}
