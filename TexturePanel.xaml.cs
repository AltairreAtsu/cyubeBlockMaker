using Microsoft.Win32;
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
	/// Interaction logic for TexturePanel.xaml
	/// </summary>
	public partial class TexturePanel : UserControl
	{
		public Uri TextureURI;
		public TexturePanelType textureType = TexturePanelType.Albedo;
		public string slotName;
		
		public TexturePanel()
		{
			InitializeComponent();
		}

		private void Grid_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

				if (System.IO.Path.GetExtension(files[0]).Equals(".png"))
				{
					TextureURI = new Uri(files[0]);
					TexturePreview_Image.Source = new BitmapImage(TextureURI);
				}
			}

		}

		private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.Filter = "PNG (*.png)|*.png";
			fileDialog.Title = "Select an image to open";

			if (fileDialog.ShowDialog() == true)
			{
				string filePath = fileDialog.FileName;
				if(System.IO.Path.GetExtension(filePath).Equals(".png"))
				{
					TextureURI = new Uri(filePath);
					TexturePreview_Image.Source = new BitmapImage(TextureURI);
				}
			}
		}

		public bool SetImageSource(string filePath)
		{
			if (System.IO.Path.GetExtension(filePath).Equals(".png"))
			{
				TextureURI = new Uri(filePath);
				TexturePreview_Image.Source = new BitmapImage(TextureURI);
				return true;
			}
			return false;
		}
	}

	public enum TexturePanelType
	{
		Albedo,
		Albedo_Small,
		Normal,
		Glow,
		RecipePreview
	}
}
