using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyubeBlockMaker
{
	public class BlockRegistry
	{
		private Dictionary<string, CustomBlock> blocks;
		private List<int> uniqueIDs;
		private List<string> categories;


		public BlockRegistry()
		{
			categories = new List<string>();
			uniqueIDs = new List<int>();
			blocks = new Dictionary<string, CustomBlock>();
		}

		public void AddBlock(string filePath, CustomBlock block)
		{
			uniqueIDs.Add(block.UniqueID);
			categories.Add(block.CategoryName);
			blocks.Add(filePath, block);
		}
		public void RemoveBlock(string filePath, CustomBlock block)
		{
			uniqueIDs.Remove(block.UniqueID);
			categories.Remove(block.CategoryName);
			blocks.Remove(filePath);
		}
		public CustomBlock GetBlock(string filePath)
		{
			if (blocks.ContainsKey(filePath)) return blocks[filePath];
			return null;
		}

		public bool ContainsUniqueID(int uid)
		{
			return uniqueIDs.Contains(uid);
		}
		public bool ContainsUniqueID(int uid, string filePath)
		{
			if (blocks.ContainsKey(filePath))
			{
				// Editing a saved block
				if (blocks[filePath].UniqueID == uid) return false;
			}
			else
			{
				return uniqueIDs.Contains(uid);
			}
			return false;
		}

		public int[] GetUIDS()
		{
			return uniqueIDs.ToArray();
		}
		public string[] GetCategories()
		{
			return categories.ToArray();
		}

	}
}
