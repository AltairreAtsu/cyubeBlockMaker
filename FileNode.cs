using System;
using System.Windows.Controls;
using CyubeBlockMaker;

public class FileNode
{
	public string name;
	public bool isDirectory = false;
	public bool containsBlock = false;
	public Object OutlinerEntry;

	public FileNode(string name, bool isDirectory)
	{
		this.name = name;
		this.isDirectory = isDirectory;
	}

	public bool Equals(FileNode node)
	{
		return this.name.Equals(node.name);
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
