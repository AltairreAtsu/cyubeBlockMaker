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
using System.Windows.Shapes;

namespace CyubeBlockMaker
{
	/// <summary>
	/// Interaction logic for TextPrompt.xaml
	/// </summary>
	public partial class TextPrompt : Window
	{
		public string UserText { get { return TextField.Text; } }
		
		public TextPrompt()
		{
			InitializeComponent();
			TextField.Focus();
		}

		public TextPrompt(string title, string message):this()
		{
			Title = title;
			MessageLabel.Content = message;
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Enter)
			{
				DialogResult = true;
				Close();
			}
		}
	}
}
