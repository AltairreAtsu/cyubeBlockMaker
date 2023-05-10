using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyubeBlockMaker
{
	public class CustomBlock
	{
		public string Name;
		public string CreatorName;
		public string CategoryName;

		public int UniqueID;
		public BlockRecipe Recipe;

		public int Yield;
		public int UniqueIDToDrop;
		public int SimilarTo;
		public bool AllowMove = true;
		public bool AllowCrystalAssistedBlockPlacement = true;

		public TextureSettings Textures;
		public int AnimationSpeed;
	}
}
