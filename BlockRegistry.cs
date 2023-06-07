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
		private Dictionary<string, int> categoryBlockCounts;
		private List<int> uniqueIDs;
		private List<string> categories;


		public BlockRegistry()
		{
			categories = new List<string>();
			uniqueIDs = new List<int>();
			blocks = new Dictionary<string, CustomBlock>();
			categoryBlockCounts = new Dictionary<string, int>();
		}

		public void AddBlock(string filePath, CustomBlock block)
		{
			uniqueIDs.Add(block.UniqueID);
			if (!categories.Contains(block.CategoryName))
			{
				categories.Add(block.CategoryName);
				categoryBlockCounts.Add(block.CategoryName, 1);
			}
			else
			{
				categoryBlockCounts[block.CategoryName]++;
			}
				
			blocks.Add(filePath, block);
		}
		public void RemoveBlock(string filePath, CustomBlock block)
		{
			uniqueIDs.Remove(block.UniqueID);

			categoryBlockCounts[block.CategoryName]--;
			if (categoryBlockCounts[block.CategoryName] == 0)
			{
				categories.Remove(block.CategoryName);
				categoryBlockCounts.Remove(block.CategoryName);
			}
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
		public List<string> GetCategoriesAsList()
		{
			return categories;
		}
	}
}
