using System.Collections.Generic;

public struct TreePrinter
{
	public List<string> nodePaths;
	public int depth;

	public TreePrinter()
	{
		nodePaths = new List<string>();
		depth = 0;
	}
}