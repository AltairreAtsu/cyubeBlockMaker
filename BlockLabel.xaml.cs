using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CyubeBlockMaker
{
	/// <summary>
	/// Interaction logic for BlockLabel.xaml
	/// </summary>
	public partial class BlockLabel : UserControl
	{
		private string blockName;
		public string filePath;
		
		public BlockLabel()
		{
			InitializeComponent();
		}


		// Button Event Handling
		private void UserControl_MouseEnter(object sender, MouseEventArgs e)
		{
			Background = MainWindow.mouseOverButtonBackground;
        }
		private void UserControl_MouseLeave(object sender, MouseEventArgs e)
		{
			Background = MainWindow.defaultButtonBackground;
        }
		private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Background = MainWindow.mouseDownButtonBackground;
			if(e.ClickCount == 2)
				MainWindow.mainWindow.OpenBlock(filePath);
        }
		private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Background = MainWindow.mouseOverButtonBackground;
        }

		// Context Menu Event Handling
		private void Conext_Open_Click(object sender, RoutedEventArgs e)
		{

		}
		private void Conext_Export_Click(object sender, RoutedEventArgs e)
		{

		}
		private void Conext_Duplicate_Click(object sender, RoutedEventArgs e)
		{

		}
		private void Conext_Delete_Click(object sender, RoutedEventArgs e)
		{

		}

		// Getters and Setters
		public void SetBlockName(string blockName)
		{
			this.blockName = blockName;
			BlockNameLabel.Content = blockName;
		}
		public string GetBlockName()
		{
			return blockName;
		}
	}
}
