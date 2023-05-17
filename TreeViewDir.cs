using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CyubeBlockMaker
{
	public class TreeViewDir : TreeViewItem
	{
		private int sortPriority = 0;
		public int SortPriority{ get {return sortPriority; }}
	}
}
