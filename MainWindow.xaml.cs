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
		private bool yieldInvalidFlag = false;
		private bool animationSpeedInvalidFlag = false;
		private bool uniqueIDToDropInvalidFlag = false;

		private List<TexturePanel> texturePanels = new List<TexturePanel>();

		public MainWindow()
		{
			InitializeComponent();
			mainWindow = this;

			InitializeFileMenu();
			InitializeTextureTab();
		}

		private void CheckBox_Checked(object sender, RoutedEventArgs e)
		{

        }

		private void InitializeFileMenu()
		{
			List<IControlMenuOption> fileMenuOptions = new List<IControlMenuOption>();
			fileMenuOptions.Add(new NewFileOption());
			fileMenuOptions.Add(new OpenFileOption());
			fileMenuOptions.Add(new SaveFileOption());
			fileMenuOptions.Add(new SaveAsOption());
			File_MenuItem.InitializeOptions(fileMenuOptions);
		}

		private void InitializeTextureTab()
		{
			AddTexturePanel("all", TexturePanelType.Albedo);
			AddTexturePanel("all__small", TexturePanelType.Albedo_Small);
			AddTexturePanel("RecipePreview", TexturePanelType.RecipePreview);
		}

		public void ResetWindow()
		{
			Name_TextBox.Text = string.Empty;
			Category_TextBox.Text = string.Empty;
			CreatorName_TextBox.Text = string.Empty;
			UniqueID_Textbox.Text = string.Empty;
			Yield_Textbox.Text = string.Empty;
			AnimationSpeed_TextBox.Text = string.Empty;
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
			int animationSpeed = 0;
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
			if (Yield_Textbox.Text == string.Empty)
			{
				yieldInvalidFlag = true;
				saveFailed = true;
				Yield_Textbox.BorderBrush = Brushes.Red;
				errorMessages.Add("Yield Field cannot be empty!");
			}
			else
			{
				try
				{
					yield = int.Parse(Yield_Textbox.Text);
					if(yield < 1)
					{
						yieldInvalidFlag = true;
						saveFailed = true;
						Yield_Textbox.BorderBrush = Brushes.Red;
						errorMessages.Add("Yield cannot be less than 1!");
					}
					else if(yield > 50)
					{
						yieldInvalidFlag = true;
						saveFailed = true;
						Yield_Textbox.BorderBrush = Brushes.Red;
						errorMessages.Add("Yield cannot be greater than 50!");
					}
				}
				catch
				{
					yieldInvalidFlag = true;
					saveFailed = true;
					Yield_Textbox.BorderBrush = Brushes.Red;
					errorMessages.Add("Failed to Parse Yield. Please ensure only numeric characters are used. Do not leave any whitespace between the numbers.");
				}

			}
			if(!yieldInvalidFlag) block.Yield = yield;

			// SimilarTo Validation
			block.SimilarTo = SimliarTo_ComboBox.SelectedIndex+1;

			// AnimationSpeed Validation
			if (AnimationSpeed_TextBox.Text == string.Empty)
			{
				block.AnimationSpeed = animationSpeed;
			}
			else
			{
				try
				{
					animationSpeed = int.Parse(AnimationSpeed_TextBox.Text);
					block.AnimationSpeed = animationSpeed;
				}
				catch
				{
					animationSpeedInvalidFlag = true;
					saveFailed = true;
					AnimationSpeed_TextBox.BorderBrush = Brushes.Red;
					errorMessages.Add("Failed to Parse AnimationSpeed! Please ensure only numeric characters are used. Do not leave whitespace between the numbers.");
				}
			}

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
					block.UniqueIDToDrop = UIDToDrop;
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
			//TODO
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
				// TODO Save the JSON to the provided path
			}
		}
		public void LoadCustomBlock(CustomBlock block)
		{
			Name_TextBox.Text = block.Name;
			CreatorName_TextBox.Text = block.CreatorName;
			Yield_Textbox.Text = block.Yield.ToString();
			UniqueID_Textbox.Text = block.UniqueID.ToString();
			SimliarTo_ComboBox.SelectedIndex = block.SimilarTo-1;

			AnimationSpeed_TextBox.Text = block.AnimationSpeed.ToString();
			AllowMove_Checkbox.IsChecked = block.AllowMove;
			AllowCrystalPlace_Checkbox.IsChecked = block.AllowCrystalAssistedBlockPlacement;
			if(block.CategoryName != null) Category_TextBox.Text = block.CategoryName;
			//TODO Attempt to Load Textures, If Fail setup Texture Slots according to the Texture Mode
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
		private void Yield_Textbox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if(yieldInvalidFlag){
				yieldInvalidFlag = false;
				Yield_Textbox.BorderBrush = SystemColors.ControlDarkBrush;
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
				AddTexturePanel("sides__small", TexturePanelType.Albedo_Small);
				AddTexturePanel("updown", TexturePanelType.Albedo);
				AddTexturePanel("updown__smal", TexturePanelType.Albedo_Small);
				AddTexturePanel("RecipePreview", TexturePanelType.RecipePreview);

				if (WithNormals_CheckBox.IsChecked == true)
					AddNormalMapPanels(TextureMode_ComboBox.SelectedIndex);
				if (WithGlowMaps_CheckBox.IsChecked == true)
					AddGlowMapPanels(TextureMode_ComboBox.SelectedIndex);
			}
			if(TextureMode_ComboBox.SelectedIndex == 2)
			{
				AddTexturePanel("sides", TexturePanelType.Albedo);
				AddTexturePanel("sides__small", TexturePanelType.Albedo_Small);
				AddTexturePanel("up", TexturePanelType.Albedo);
				AddTexturePanel("up__small", TexturePanelType.Albedo_Small);
				AddTexturePanel("down", TexturePanelType.Albedo);
				AddTexturePanel("down__small", TexturePanelType.Albedo_Small);
				AddTexturePanel("RecipePreview", TexturePanelType.RecipePreview);

				if (WithNormals_CheckBox.IsChecked == true)
					AddNormalMapPanels(TextureMode_ComboBox.SelectedIndex);
				if (WithGlowMaps_CheckBox.IsChecked == true)
					AddGlowMapPanels(TextureMode_ComboBox.SelectedIndex);
			}
			if(TextureMode_ComboBox.SelectedIndex == 3)
			{
				AddTexturePanel("up", TexturePanelType.Albedo);
				AddTexturePanel("up__small", TexturePanelType.Albedo_Small);
				AddTexturePanel("down", TexturePanelType.Albedo);
				AddTexturePanel("down__small", TexturePanelType.Albedo_Small);
				AddTexturePanel("left", TexturePanelType.Albedo);
				AddTexturePanel("left__small", TexturePanelType.Albedo_Small);
				AddTexturePanel("right", TexturePanelType.Albedo);
				AddTexturePanel("right__small", TexturePanelType.Albedo_Small);
				AddTexturePanel("front", TexturePanelType.Albedo);
				AddTexturePanel("front__small", TexturePanelType.Albedo_Small);
				AddTexturePanel("back", TexturePanelType.Albedo);
				AddTexturePanel("back__small", TexturePanelType.Albedo_Small);
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
				AddTexturePanel("all__normal", TexturePanelType.Normal);
			}
			if (textureMode == 1)
			{
				AddTexturePanel("sides__normal", TexturePanelType.Normal);
				AddTexturePanel("updown__normal", TexturePanelType.Normal);
			}
			if (textureMode == 2)
			{
				AddTexturePanel("sides__normal", TexturePanelType.Normal);
				AddTexturePanel("up__normal", TexturePanelType.Normal);
				AddTexturePanel("down__normal", TexturePanelType.Normal);
			}
			if (textureMode == 3)
			{
				AddTexturePanel("up__normal", TexturePanelType.Normal);
				AddTexturePanel("down__normal", TexturePanelType.Normal);
				AddTexturePanel("left__normal", TexturePanelType.Normal);
				AddTexturePanel("right__normal", TexturePanelType.Normal);
				AddTexturePanel("front__normal", TexturePanelType.Normal);
				AddTexturePanel("back__normal", TexturePanelType.Normal);
			}
		}
		private void AddGlowMapPanels(int textureMode)
		{
			if(textureMode == 0)
			{
				AddTexturePanel("all__glow", TexturePanelType.Glow);
			}
			if (textureMode == 1)
			{
				AddTexturePanel("sides__glow", TexturePanelType.Glow);
				AddTexturePanel("updown__glow", TexturePanelType.Glow);
			}
			if(textureMode == 2) 
			{
				AddTexturePanel("sides__glow", TexturePanelType.Glow);
				AddTexturePanel("up__glow", TexturePanelType.Glow);
				AddTexturePanel("down__glow", TexturePanelType.Glow);
			}
			if (textureMode == 3)
			{
				AddTexturePanel("up__glow", TexturePanelType.Glow);
				AddTexturePanel("down__glow", TexturePanelType.Glow);
				AddTexturePanel("left__glow", TexturePanelType.Glow);
				AddTexturePanel("right__glow", TexturePanelType.Glow);
				AddTexturePanel("front__glow", TexturePanelType.Glow);
				AddTexturePanel("back__glow", TexturePanelType.Glow);
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
	}
}
