using System.Collections.Generic;

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
		if (Item.path == path)
		{
			return this;
		}
		foreach(TreeNode node in Children)
		{
			var childNode = node.GetNodeFromPath(path);
			if (childNode != null)
			{
				return childNode;
			}
		}
		return null;
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
		treePrinter.nodePaths.Add(path + Item.path);
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
