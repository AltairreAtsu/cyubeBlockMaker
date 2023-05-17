using Microsoft.Win32;
using SharpVectors.Renderers.Gdi;
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
		private static int TEXTURE_ALBEDO_SIZE = 2048;
		private static int TEXTURE_SMALL_ALBEDO_SIZE = 512;
		private static int TEXTURE_GLOW_SIZE = 1024;

		public BitmapImage image;
		
		public Uri TextureURI;
		public TexturePanelType textureType = TexturePanelType.Albedo;
		public string slotName;

		public bool imageSizeInvalidFlag = false;
		
		public TexturePanel()
		{
			InitializeComponent();
		}

		private void Grid_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				List<string> imageFiles = new List<string>();
				for(int i = 0; i < files.Length; i++)
				{
					if (System.IO.Path.GetExtension(files[0]).Equals(".png"))
					{
						imageFiles.Add(files[i]);
					}
				}
				if(imageFiles.Count == 1)
				{
					SetImageSource(imageFiles[0]);
				}
				else if(imageFiles.Count > 1)
				{
					MainWindow.mainWindow.PopulateTexturePanels(imageFiles);
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
				SetImageSource(filePath);
			}
		}

		public bool SetImageSource(string filePath)
		{
			BorderBrush = null;
			imageSizeInvalidFlag = false;

			if (System.IO.Path.GetExtension(filePath).Equals(".png"))
			{
				TextureURI = new Uri(filePath);
				image = new BitmapImage();
				image.BeginInit();
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.UriSource = TextureURI;
				image.EndInit();
				TexturePreview_Image.Source = image;

				if (!ValidateImageSize()) WarnUserInvalidImageSize();

				MainWindow.mainWindow.dataHasChanged = true;
				return true;
			}
			return false;
		}

		private void WarnUserInvalidImageSize()
		{
			BorderBrush = Brushes.Red;
			BorderThickness = new Thickness(1);
			imageSizeInvalidFlag = true;

			string correctSize="";
			if(textureType == TexturePanelType.Albedo || textureType == TexturePanelType.Normal) correctSize = TEXTURE_ALBEDO_SIZE.ToString();
			if(textureType == TexturePanelType.Albedo_Small || textureType == TexturePanelType.RecipePreview) correctSize = TEXTURE_SMALL_ALBEDO_SIZE.ToString();
			if(textureType == TexturePanelType.Glow) correctSize = TEXTURE_GLOW_SIZE.ToString();

			MessageBox.Show("Warning! The image in slot " + slotName + "is not the correct size! The correct size for that slot is " + correctSize + "x" + correctSize + ".");
		}

		private bool ValidateImageSize()
		{

			if      (textureType == TexturePanelType.Albedo || textureType == TexturePanelType.Normal)
			{
				return image.PixelHeight == TEXTURE_ALBEDO_SIZE && image.PixelWidth == TEXTURE_ALBEDO_SIZE;
			}
			else if (textureType == TexturePanelType.Albedo_Small || textureType == TexturePanelType.RecipePreview)
			{
				return image.PixelHeight == TEXTURE_SMALL_ALBEDO_SIZE && image.PixelWidth == TEXTURE_SMALL_ALBEDO_SIZE;
			}
			else
			{
				return image.PixelHeight == TEXTURE_GLOW_SIZE && image.PixelWidth == TEXTURE_GLOW_SIZE;
			}
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
