using System;
using System.Windows.Controls;
using CyubeBlockMaker;

public class FileNode
{
	public string path;
	public bool isDirectory = false;
	public bool containsBlock = false;
	public Object OutlinerEntry;

	public FileNode(string path, bool isDirectory)
	{
		this.path = path;
		this.isDirectory = isDirectory;
	}

	public bool Equals(FileNode node)
	{
		return this.path.Equals(node.path);
	}

	public bool OutlinerEntryIsTreeViewItem()
	{
		return OutlinerEntry is TreeViewItem;
	}
	public bool OutlinerEntryIsBlockLabel()
	{
		return OutlinerEntry is BlockLabel;
	}
}
