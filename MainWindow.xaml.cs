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
using System.IO;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using CyubeBlockMaker.File_Menu_Options;

namespace CyubeBlockMaker
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public static MainWindow mainWindow;
		
		// State Flags
		private bool nameInvalidFlag = false;
		private bool creatorNameInvalidFlag = false;
		private bool uniqueIDInvalidFlag = false;
		private bool animationSpeedInvalidFlag = false;
		private bool uniqueIDToDropInvalidFlag = false;

		private List<TexturePanel> texturePanels = new List<TexturePanel>();
		private BlockRecipe recipe;

		public MainWindow()
		{
			InitializeComponent();
			mainWindow = this;

			InitializeFileMenu();
			InitializeTextureTab();
		}

		private void InitializeFileMenu()
		{
			List<IControlMenuOption> fileMenuOptions = new List<IControlMenuOption>();
			fileMenuOptions.Add(new NewFileOption());
			fileMenuOptions.Add(new OpenFileOption());
			fileMenuOptions.Add(new SaveFileOption());
			fileMenuOptions.Add(new SaveAsOption());
			fileMenuOptions.Add(new ExportFileOption());
			File_MenuItem.InitializeOptions(fileMenuOptions);
		}

		private void InitializeTextureTab()
		{
			AddTexturePanel("all", TexturePanelType.Albedo);
			AddTexturePanel("all_small", TexturePanelType.Albedo_Small);
			AddTexturePanel("RecipePreview", TexturePanelType.RecipePreview);
		}

		public void ResetWindow()
		{
			Name_TextBox.Text = string.Empty;
			Category_TextBox.Text = string.Empty;
			CreatorName_TextBox.Text = string.Empty;
			UniqueID_Textbox.Text = string.Empty;
			Yield_Slider.Value = 0;
			AnimationSpeed_Slider.Value = 0;
			SimliarTo_ComboBox.SelectedIndex = 0;
			AllowCrystalPlace_Checkbox.IsChecked = true;
			AllowMove_Checkbox.IsChecked = true;
			WithNormals_CheckBox.IsChecked = false;
			WithGlowMaps_CheckBox.IsChecked = false;
			TextureMode_ComboBox.SelectedIndex = 0;
			TextureTabWrapPanel.Children.Clear();
			// TODO Prep Texture Wrap according to the selected mode
			
		}
		public void Save()
		{
			List<string> errorMessages = new List<string>();
			CustomBlock block = new CustomBlock();
			int uID = 0;
			int yield = 0;
			int UIDToDrop = -2;
			bool saveFailed = false;

			// Name Validation
			if(Name_TextBox.Text == string.Empty)
			{
				Name_TextBox.BorderBrush = Brushes.Red;
				nameInvalidFlag = true;
				saveFailed = true;
				errorMessages.Add("Name field cannot be empty!");
			}
			if (!nameInvalidFlag) block.Name = Name_TextBox.Text;

			// CreatorName Validation
			if(CreatorName_TextBox.Text == string.Empty)
			{
				CreatorName_TextBox.BorderBrush = Brushes.Red;
				creatorNameInvalidFlag = true;
				saveFailed = true;
				errorMessages.Add("Creator Name field cannot be empty!");
			}
			if(!creatorNameInvalidFlag) block.CreatorName = CreatorName_TextBox.Text;
			
			// Category Validation
			if(Category_TextBox.Text != string.Empty) block.CategoryName = Category_TextBox.Text;

			// UniqueID Validation
			if(UniqueID_Textbox.Text == string.Empty)
			{
				uniqueIDInvalidFlag = true;
				saveFailed = true;
				UniqueID_Textbox.BorderBrush = Brushes.Red;
				errorMessages.Add("Unique ID field cannot be empty!");
			}
			else
			{
				try
				{
					uID = int.Parse(UniqueID_Textbox.Text);
					if(uID < 1)
					{
						uniqueIDInvalidFlag = true;
						saveFailed = true;
						UniqueID_Textbox.BorderBrush = Brushes.Red;
						errorMessages.Add("Unique ID cannot be equal to or less than 1!");
					}
				}
				catch
				{
					uniqueIDInvalidFlag = true;
					saveFailed = true;
					UniqueID_Textbox.BorderBrush = Brushes.Red;
					errorMessages.Add("Failed to Parse Unique ID. Please ensure only numeric characters are used. Do not leave any whitespace between the numbers.");
				}
			}
			if (!uniqueIDInvalidFlag) block.UniqueID = uID;

			// Yield Validation
			block.Yield = (int)Math.Round(Yield_Slider.Value);

			// SimilarTo Validation
			block.SimilarTo = SimliarTo_ComboBox.SelectedIndex+1;

			// AnimationSpeed Validation
			block.AnimationSpeed = (int)Math.Round(AnimationSpeed_Slider.Value);

			// UniqueIDToDrop Validation
			if(UniqueIDToDrop_TextBox.Text == string.Empty)
			{
				block.UniqueIDToDrop = UIDToDrop;
			}
			else
			{
				try
				{
					UIDToDrop = int.Parse(UniqueIDToDrop_TextBox.Text);
					if(UIDToDrop > -2) 
					{
						saveFailed = true;
						uniqueIDToDropInvalidFlag = true;
						UniqueIDToDrop_TextBox.BorderBrush = Brushes.Red;
						errorMessages.Add("Unique ID to Drop cannot be less than -2! Allowed values: -2 Itself, -1 Nothing, 0+ Custom block to drop.");
					}
					else
					{
						block.UniqueIDToDrop = UIDToDrop;
					}
				}
				catch
				{
					uniqueIDToDropInvalidFlag = true;
					saveFailed = true;
					UniqueIDToDrop_TextBox.BorderBrush = Brushes.Red;
					errorMessages.Add("Failed to Parse Unique IDToDrop! Please ensure only numeric characters are used. Do not leave whitespace between the numbers.");
				}
			}


			// Recipe Validation
			if(recipe == null)
			{
				recipe = new BlockRecipe();
				recipe.Array = Array.Empty<int>();
				recipe.SizeZ = 0;
				recipe.SizeY = 0;
				recipe.SizeX = 0;
				block.Recipe = recipe;
			}
			else
			{
				block.Recipe = recipe;
			}

			// Texture Validation
			block.Textures = new TextureSettings();
			block.Textures.Mode = TextureMode_ComboBox.SelectedIndex + 1;
			if(WithNormals_CheckBox.IsChecked != null)
			{
				block.Textures.WithNormals = WithNormals_CheckBox.IsChecked.Value;
			}
			else
			{
				saveFailed = true;
				errorMessages.Add("Failed to parse value of Texture Has Normals.");
			}
			
			if(WithGlowMaps_CheckBox.IsChecked != null)
			{
				block.Textures.WithGlowMap = WithGlowMaps_CheckBox.IsChecked.Value;
			}
			else
			{
				saveFailed = true;
				errorMessages.Add("Failed to parse value of Texture has Glow Maps.");
			}

			// Saving the Data
			if (saveFailed)
			{
				string errorLog = String.Join('\n', errorMessages);
				MessageBox.Show(errorLog);
			}
			else
			{
				VistaFolderBrowserDialog folderBrowserDialog = new VistaFolderBrowserDialog();
				
				if(folderBrowserDialog.ShowDialog() == true)
				{
					JsonManager.WriteCustomBlockJson(folderBrowserDialog.SelectedPath + System.IO.Path.DirectorySeparatorChar + block.Name + ".block", block);

					string textureDir = folderBrowserDialog.SelectedPath + System.IO.Path.DirectorySeparatorChar + "Textures";
					Directory.CreateDirectory(textureDir);
					textureDir = textureDir + System.IO.Path.DirectorySeparatorChar;
					foreach(TexturePanel texturePanel in texturePanels)
					{
						if(texturePanel.TextureURI != null)
						{
							
							File.Copy(texturePanel.TextureURI.LocalPath, textureDir + texturePanel.slotName + ".png");
						}
					}
				}
			}
		}
		public void LoadCustomBlock(CustomBlock block, string path)
		{
			Name_TextBox.Text = block.Name;
			CreatorName_TextBox.Text = block.CreatorName;

			Yield_Slider.Value = Math.Max(0, Math.Min(block.Yield, 50));

			UniqueID_Textbox.Text = block.UniqueID.ToString();
			SimliarTo_ComboBox.SelectedIndex = block.SimilarTo-1;

			AnimationSpeed_Slider.Value = Math.Max(0, Math.Min(block.AnimationSpeed, 255));

			AllowMove_Checkbox.IsChecked = block.AllowMove;
			AllowCrystalPlace_Checkbox.IsChecked = block.AllowCrystalAssistedBlockPlacement;
			if(block.CategoryName != null) Category_TextBox.Text = block.CategoryName;

			recipe = block.Recipe;
			
			TextureMode_ComboBox.SelectedIndex = block.Textures.Mode - 1;
			WithNormals_CheckBox.IsChecked = block.Textures.WithNormals;
			WithGlowMaps_CheckBox.IsChecked = block.Textures.WithGlowMap;

            string dir = System.IO.Path.GetDirectoryName(path);
			dir = dir + System.IO.Path.DirectorySeparatorChar + "Textures";
			if (Directory.Exists(dir))
			{
				string[] files = Directory.GetFiles(dir);
				foreach (string file in files)
				{
					string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
					foreach (TexturePanel panel in texturePanels)
					{
						if (panel.slotName.Equals(fileName))
						{
							panel.SetImageSource(file);
						}
					}
				}
			}
			//TODO Attempt to Load Textures, If Fail setup Texture Slots according to the Texture Mode
		}
		public void Export()
		{
			// Build the Folder Structure in the target directory
			// Export the Json
			// Convert the images to DDS
			// Save the DDS images to the Textures folder
			// Notify user task complete
		}

		// Numeric Enforcement Event Handlers
		private void Yield_TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			NumberboxValidator.PreviewTextInput(e);
		}
		private void UniqueID_Textbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			NumberboxValidator.PreviewTextInput(e);
		}
		private void AnimationSpeed_TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			NumberboxValidator.PreviewTextInput(e);
		}
		private void UniqueIDToDrop_TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			NumberboxValidator.PreviewTextInput(e);
		}
		
		// Text Change Event Handlers
		private void Name_TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (nameInvalidFlag)
			{
				nameInvalidFlag = false;
				Name_TextBox.BorderBrush = SystemColors.ControlDarkBrush;
			}
		}
		private void CreatorName_TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (creatorNameInvalidFlag)
			{
				creatorNameInvalidFlag = false;
				CreatorName_TextBox.BorderBrush = SystemColors.ControlDarkBrush;
			}
		}
		private void UniqueID_Textbox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (uniqueIDInvalidFlag)
			{
				uniqueIDInvalidFlag = false;
				UniqueID_Textbox.BorderBrush = SystemColors.ControlDarkBrush;
			}
		}
		private void UniqueIDToDrop_TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (uniqueIDToDropInvalidFlag)
			{
				uniqueIDToDropInvalidFlag = false;
				UniqueIDToDrop_TextBox.BorderBrush = SystemColors.ControlDarkBrush;
			}
		}

		// Texture Panel Mangement
		private void TextureMode_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (TextureTabWrapPanel == null) return;
			TextureTabWrapPanel.Children.Clear();
			texturePanels.Clear();

			if(TextureMode_ComboBox.SelectedIndex == 0)
			{
				InitializeTextureTab();

				if(WithNormals_CheckBox.IsChecked == true)
					AddNormalMapPanels(TextureMode_ComboBox.SelectedIndex);
				if (WithGlowMaps_CheckBox.IsChecked == true)
					AddGlowMapPanels(TextureMode_ComboBox.SelectedIndex);
			}
			if(TextureMode_ComboBox.SelectedIndex == 1)
			{
				AddTexturePanel("sides", TexturePanelType.Albedo);
				AddTexturePanel("sides_small", TexturePanelType.Albedo_Small);
				AddTexturePanel("updown", TexturePanelType.Albedo);
				AddTexturePanel("updown_smal", TexturePanelType.Albedo_Small);
				AddTexturePanel("RecipePreview", TexturePanelType.RecipePreview);

				if (WithNormals_CheckBox.IsChecked == true)
					AddNormalMapPanels(TextureMode_ComboBox.SelectedIndex);
				if (WithGlowMaps_CheckBox.IsChecked == true)
					AddGlowMapPanels(TextureMode_ComboBox.SelectedIndex);
			}
			if(TextureMode_ComboBox.SelectedIndex == 2)
			{
				AddTexturePanel("sides", TexturePanelType.Albedo);
				AddTexturePanel("sides_small", TexturePanelType.Albedo_Small);
				AddTexturePanel("up", TexturePanelType.Albedo);
				AddTexturePanel("up_small", TexturePanelType.Albedo_Small);
				AddTexturePanel("down", TexturePanelType.Albedo);
				AddTexturePanel("down_small", TexturePanelType.Albedo_Small);
				AddTexturePanel("RecipePreview", TexturePanelType.RecipePreview);

				if (WithNormals_CheckBox.IsChecked == true)
					AddNormalMapPanels(TextureMode_ComboBox.SelectedIndex);
				if (WithGlowMaps_CheckBox.IsChecked == true)
					AddGlowMapPanels(TextureMode_ComboBox.SelectedIndex);
			}
			if(TextureMode_ComboBox.SelectedIndex == 3)
			{
				AddTexturePanel("up", TexturePanelType.Albedo);
				AddTexturePanel("up_small", TexturePanelType.Albedo_Small);
				AddTexturePanel("down", TexturePanelType.Albedo);
				AddTexturePanel("down_small", TexturePanelType.Albedo_Small);
				AddTexturePanel("left", TexturePanelType.Albedo);
				AddTexturePanel("left_small", TexturePanelType.Albedo_Small);
				AddTexturePanel("right", TexturePanelType.Albedo);
				AddTexturePanel("right_small", TexturePanelType.Albedo_Small);
				AddTexturePanel("front", TexturePanelType.Albedo);
				AddTexturePanel("front_small", TexturePanelType.Albedo_Small);
				AddTexturePanel("back", TexturePanelType.Albedo);
				AddTexturePanel("back_small", TexturePanelType.Albedo_Small);
				AddTexturePanel("RecipePreview", TexturePanelType.RecipePreview);

				if (WithNormals_CheckBox.IsChecked == true)
					AddNormalMapPanels(TextureMode_ComboBox.SelectedIndex);
				if (WithGlowMaps_CheckBox.IsChecked == true)
					AddGlowMapPanels(TextureMode_ComboBox.SelectedIndex);
			}
		}
		private void AddNormalMapPanels(int textureMode)
		{
			if(textureMode == 0)
			{
				AddTexturePanel("all_normal", TexturePanelType.Normal);
			}
			if (textureMode == 1)
			{
				AddTexturePanel("sides_normal", TexturePanelType.Normal);
				AddTexturePanel("updown_normal", TexturePanelType.Normal);
			}
			if (textureMode == 2)
			{
				AddTexturePanel("sides_normal", TexturePanelType.Normal);
				AddTexturePanel("up_normal", TexturePanelType.Normal);
				AddTexturePanel("down_normal", TexturePanelType.Normal);
			}
			if (textureMode == 3)
			{
				AddTexturePanel("up_normal", TexturePanelType.Normal);
				AddTexturePanel("down_normal", TexturePanelType.Normal);
				AddTexturePanel("left_normal", TexturePanelType.Normal);
				AddTexturePanel("right_normal", TexturePanelType.Normal);
				AddTexturePanel("front_normal", TexturePanelType.Normal);
				AddTexturePanel("back_normal", TexturePanelType.Normal);
			}
		}
		private void AddGlowMapPanels(int textureMode)
		{
			if(textureMode == 0)
			{
				AddTexturePanel("all_glow", TexturePanelType.Glow);
			}
			if (textureMode == 1)
			{
				AddTexturePanel("sides_glow", TexturePanelType.Glow);
				AddTexturePanel("updown_glow", TexturePanelType.Glow);
			}
			if(textureMode == 2) 
			{
				AddTexturePanel("sides_glow", TexturePanelType.Glow);
				AddTexturePanel("up_glow", TexturePanelType.Glow);
				AddTexturePanel("down_glow", TexturePanelType.Glow);
			}
			if (textureMode == 3)
			{
				AddTexturePanel("up_glow", TexturePanelType.Glow);
				AddTexturePanel("down_glow", TexturePanelType.Glow);
				AddTexturePanel("left_glow", TexturePanelType.Glow);
				AddTexturePanel("right_glow", TexturePanelType.Glow);
				AddTexturePanel("front_glow", TexturePanelType.Glow);
				AddTexturePanel("back_glow", TexturePanelType.Glow);
			}
		}
		private void RemoveAllPanelsOfType(TexturePanelType type)
		{
			List<TexturePanel> removaltargets = new List<TexturePanel>();

			foreach(TexturePanel panel in texturePanels)
			{
				if(panel.textureType == type)
				{
					removaltargets.Add(panel);
				}
			}
			foreach(TexturePanel panel in removaltargets)
			{
				texturePanels.Remove(panel);
				TextureTabWrapPanel.Children.Remove(panel);
			}
		}

		private void AddTexturePanel(string slotName, TexturePanelType textureType)
		{
			TexturePanel texturePanel = new TexturePanel();
			texturePanel.TextureName_Label.Content = slotName;
			texturePanel.Margin = new Thickness(10, 10, 0, 0);

			texturePanel.slotName = slotName;
			texturePanel.textureType = textureType;

			texturePanels.Add(texturePanel);
			TextureTabWrapPanel.Children.Add(texturePanel);
		}

		private void WithNormals_CheckBox_Checked(object sender, RoutedEventArgs e)
		{

				AddNormalMapPanels(TextureMode_ComboBox.SelectedIndex);
				
		}
		private void WithGlowMaps_CheckBox_Checked(object sender, RoutedEventArgs e)
		{
				AddGlowMapPanels(TextureMode_ComboBox.SelectedIndex);
		}
		private void WithNormals_CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			RemoveAllPanelsOfType(TexturePanelType.Normal);
		}
		private void WithGlowMaps_CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			RemoveAllPanelsOfType(TexturePanelType.Glow);
		}
		
		// Recipe Import
		private void ImportRecipe_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Text File | *.txt";
			openFileDialog.Title = "Select the exported cyube recipe.";
			if(openFileDialog.ShowDialog() == true)
			{
				BlockRecipe br = JsonManager.ReadRecipe(openFileDialog.FileName);
				if (br != null)
				{
					MessageBox.Show("Imported Sucessfully.");
					recipe = br;
				}
				else
				{
					MessageBox.Show("Failed to read the file! Please ensure the exported cyube recipe has not been modified in anyway!");
				}
			}
		}
		private void Button_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				if (System.IO.Path.GetExtension(files[0]).Equals(".txt"))
				{
					BlockRecipe br = JsonManager.ReadRecipe(files[0]);
					if(br != null)
					{
						MessageBox.Show("Imported Sucessfully.");
						recipe = br;
					}
					else
					{
						MessageBox.Show("Failed to read the file! Please ensure the exported cyube recipe has not been modified in anyway!");
					}
				}
			}
		}
	}
}
