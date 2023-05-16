using System.Collections.Generic;
using System.Windows.Forms;

public class TreeNode
{
	public TreeNode Parent { get; }
	public FileNode Item { get; set; }

	List<TreeNode> Children = new List<TreeNode>();

	public TreeNode(FileNode item)
	{
		Item = item;
	}
	public TreeNode(FileNode item, TreeNode parent)
	{
		Item = item;
		Parent = parent;
	}

	public TreeNode AddChild(FileNode item)
	{
		TreeNode nodeItem = new TreeNode(item, this);
		Children.Add(nodeItem);
		return nodeItem;
	}

	public TreeNode GetNodeFromPath(string path)
	{
		string[] pathSegments = path.Split("\\");

		if( (pathSegments.Length == 1) &&
			(pathSegments[0] == Item.name) )
		{
			return this;
		}
		else
		{
			if (pathSegments[0] == Item.name)
			{
				foreach (TreeNode node in Children)
				{
					if (pathSegments[1] == node.Item.name)
					{
						return node.GetNodeFromPath(RecompilePathWithoutCurrentNode(pathSegments));
					}
				}
			}
			else
			{
				return null;
			}
		}
		return null;
	}

	private string RecompilePathWithoutCurrentNode(string[] splitPath)
	{
		string path = "";
		for(int i = 1; i < splitPath.Length; i++)
		{
			path += splitPath[i];
			if(i != splitPath.Length - 1)
				path += "\\";
		}
		return path;
	}

	public bool RemoveNodeFromTree(string path)
	{
		var node = GetNodeFromPath(path);
		if (node != null && node.Parent != null)
		{
			node.Parent.Children.Remove(node);
			return true;
		}
		return false;
	}
	public bool RemoveNodeFromTree(TreeNode node)
	{
		if(node.Parent != null)
		{
			node.Parent.Children.Remove(node);
			return true;
		}
		return false;
	}

	public TreePrinter PrintTree(TreePrinter treePrinter)
	{
		string path = "";
		for (int i = 0; i <= treePrinter.depth; i++)
		{
			path += "_";
		}
		treePrinter.nodePaths.Add(path + Item.name);
		treePrinter.depth++;
		foreach (TreeNode node in Children)
		{
			node.PrintTree(treePrinter);
		}
		treePrinter.depth--;
		return treePrinter;
	}

	public TreeNode GetChildAt(int index)
	{
		return Children[index];
	}
	public int GetChildCount()
	{
		return Children.Count;
	}
}
