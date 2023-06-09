﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System.Security.Cryptography;
using System.Diagnostics;
using draw = System.Drawing;
using System.Linq;

namespace CyubeBlockMaker
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public static MainWindow mainWindow;

		public static Brush defaultButtonBackground = null;
		public static Brush mouseOverButtonBackground = Brushes.Lavender;
		public static Brush mouseDownButtonBackground = Brushes.AliceBlue;

		public static string WORKSPACE_ROOT = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Workspace";
		public static string WORKSPACE_NAME = "Workspace";
		public static string SETTINGS_FILE = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Settings.json";

		// State Flags
		private bool nameInvalidFlag = false;
		private bool creatorNameInvalidFlag = false;
		private bool uniqueIDInvalidFlag = false;
		private bool uniqueIDToDropInvalidFlag = false;
		public bool dataHasChanged = false;

		private int uID;
		private int UIDToDrop;
		private bool glowMap;
		private bool normalMap;

		private bool allowMove = true;
		private bool allowCrystalPlacement = true;
		private bool useBlankRecipeImage = false;
		private bool autoGenerateSmallAlbedo = false;
		private int similiarTo = 0;
		private int animationSpeed = 0;
		private int yield = 0;

		private string saveDestination = "";

		private List<TexturePanel> texturePanels = new List<TexturePanel>();
		private BlockRecipe recipe;
		public WorkspaceManager workspaceManager;
		
		public SettingsManager settingsManager;
		private SettingsWindow settingsWindow;
		private bool settingsWindowOpen = false;

		public MainWindow()
		{
			InitializeComponent();
			mainWindow = this;
			settingsManager = new SettingsManager();

			InitializeTextureTab();
			InitializeWorkspace();
			ResetCreatorName();
			ResetAutoGenerateSmallAlbedo();
			CategorySuggestionBox.suggestionStrings = workspaceManager.BlockRegistry.GetCategoriesAsList();
			AttemptLocateCyubeInstall();
		}

		private void InitializeTextureTab()
		{
			useBlankRecipeImage = settingsManager.AlwaysAutoGenerateBlankRecipe;
			BlankRecipeImage_Checkbox.IsChecked = useBlankRecipeImage;

			AddTexturePanel("all", TexturePanelType.Albedo);
			if(!autoGenerateSmallAlbedo)
				AddTexturePanel("all_small", TexturePanelType.Albedo_Small);
			if(!useBlankRecipeImage)
				AddTexturePanel("RecipePreview", TexturePanelType.RecipePreview);
			SortTexturePanel();
		}
		
		private void InitializeWorkspace()
		{
			if (!Directory.Exists(WORKSPACE_ROOT+"\\"))
			{
				Directory.CreateDirectory(WORKSPACE_ROOT+"\\");
				// Workspace is empty
				return;
			}

			workspaceManager = new WorkspaceManager(Outliner);
			workspaceManager.BuildWorkspace();
			workspaceManager.SortOutliner();
		}

		public void ResetWindow()
		{
			Name_TextBox.Text = string.Empty;
			CategorySuggestionBox.SuggestionTextField.Text = string.Empty;
			ResetCreatorName();
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
			ResetAutoGenerateBlankRecipe();
			ResetAutoGenerateSmallAlbedo();
			
			TextureMode_ComboBox_SelectionChanged(null, null);

			ResetStateFlags();
		}
		private void ResetStateFlags()
		{
			dataHasChanged = false;
			nameInvalidFlag = false;
			Name_TextBox.BorderBrush = SystemColors.ControlDarkBrush;
			creatorNameInvalidFlag = false;
			CreatorName_TextBox.BorderBrush = SystemColors.ControlDarkBrush;
			uniqueIDInvalidFlag = false;
			UniqueID_Textbox.BorderBrush = SystemColors.ControlDarkBrush;
			uniqueIDToDropInvalidFlag = false;
			UniqueIDToDrop_TextBox.BorderBrush = SystemColors.ControlDarkBrush;
			saveDestination = "";
		}

		private void ResetCreatorName()
		{
			if (settingsManager.DoPreFillCreatorName)
			{
				CreatorName_TextBox.Text = settingsManager.PreFillCreatorName;
			}
			else
			{
				CreatorName_TextBox.Text = string.Empty;
			}
		}
		private void ResetAutoGenerateBlankRecipe()
		{
			BlankRecipeImage_Checkbox.IsChecked = settingsManager.AlwaysAutoGenerateBlankRecipe;
			useBlankRecipeImage = settingsManager.AlwaysAutoGenerateBlankRecipe;
		}
		private void ResetAutoGenerateSmallAlbedo()
		{
			AutoGenerateSmallAlbedo_Checkbox.IsChecked = settingsManager.AlwaysAutoGenerateSmallAlbedo;
			autoGenerateSmallAlbedo = settingsManager.AlwaysAutoGenerateSmallAlbedo;
		}
		private void AttemptLocateCyubeInstall()
		{
			var cyubeInstallLocator = new CyubeInstallLocator();
			if (!settingsManager.UserHasFoundCyubeInstall)
			{
				var result = cyubeInstallLocator.AttemptToAutoLocate();
				if (result.sucess)
				{
					settingsManager.CyubeInstallLocation = result.path;
				}
				else
				{
					WarnUserInstallNotFound(cyubeInstallLocator);
				}
			}
			else
			{
				if (Directory.Exists(settingsManager.CyubeInstallLocation))
				{
					if (!cyubeInstallLocator.IsCyubeRootFolder(settingsManager.CyubeInstallLocation))
					{
						WarnUserInstallNotFound(cyubeInstallLocator);
					}
				}
				else
				{
					WarnUserInstallNotFound(cyubeInstallLocator);
				}
			}
		}
		private void WarnUserInstallNotFound(CyubeInstallLocator cyubeInstallLocator)
		{
			if (MessageBox.Show("Cyube Install not detected! Would you like to update your install location now? Some app features will be disabled if you don't!", "404 Cyube not found!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
			{
				var result = cyubeInstallLocator.OpenSearchBroswer();
				if (result.sucess)
				{
					settingsManager.CyubeInstallLocation = result.path;
					settingsManager.SaveSettings();
				}
			}
		}

		#region FileOptionMethods
		public void Save(bool forceDialog)
		{
			ValidationResult result = ValidateSaveData();
			if (!result.validationSucess)
			{
				string errorLog = String.Join('\n', result.validationErrors);
				MessageBox.Show(errorLog);
				return;
			}

			CustomBlock block = CompileBlock();
			if(saveDestination != String.Empty && !forceDialog)
			{
				WriteData(saveDestination, block);
				return;
			}

			// Save Destionation was empty and a new location muct be decided
			// Open a text prompt for the folder and just check the folder does not already exist
			TextPrompt textPrompt = new TextPrompt("Save Block", "Please enter a project name for your block.");
			if(textPrompt.ShowDialog() == true)
			{
				if (textPrompt.UserText.Contains('\\'))
				{
					MessageBox.Show("Project name cannot contain symbol: \\");
					Save(true);
					return;
				}
				
				string path = WORKSPACE_ROOT + "\\" + textPrompt.UserText;
				if (Directory.Exists(path))
				{
					MessageBox.Show("A Block already exists with that project name! Please use a different project name!");
					Save(true);
					return;
				}
				else
				{
					string blockPath = path + "\\" + textPrompt.UserText + ".block";
					WriteData(blockPath, block);
				}
			}
		}
		private void WriteData(string path, CustomBlock block)
		{
			dataHasChanged = false;
			GC.Collect();
			GC.WaitForPendingFinalizers();

			JsonManager.WriteCustomBlockJson(path, block);

			string textureDir = System.IO.Path.GetDirectoryName(path) + System.IO.Path.DirectorySeparatorChar + "Textures";
			Directory.CreateDirectory(textureDir);
			textureDir = textureDir + System.IO.Path.DirectorySeparatorChar;
			foreach (TexturePanel texturePanel in texturePanels)
			{
				if (texturePanel.TextureURI != null)
				{
					string sourcePath = texturePanel.TextureURI.LocalPath;
					string destPath = textureDir + texturePanel.slotName + ".png";
					if (sourcePath != destPath)
						File.Copy(sourcePath, destPath, true);
				}
			}

			workspaceManager.AddBlock(path, block);
		}
		public void OpenBlock(string path)
		{
			if (!DiscardUnsavedData())
			{
				return;
			}

			CustomBlock block;
			block = JsonManager.ReadJson(path);
			if (block == null)
			{
				MessageBox.Show("Failed to Parse block JSON. File data may not be formated correctly.");
				return;
			}

			saveDestination = path;

			Name_TextBox.Text = block.Name;
			CreatorName_TextBox.Text = block.CreatorName;

			Yield_Slider.Value = Math.Max(0, Math.Min(block.Yield, 50));

			UniqueID_Textbox.Text = block.UniqueID.ToString();
			SimliarTo_ComboBox.SelectedIndex = block.SimilarTo - 1;

			AnimationSpeed_Slider.Value = Math.Max(0, Math.Min(block.AnimationSpeed, 255));

			AllowMove_Checkbox.IsChecked = block.AllowMove;
			AllowCrystalPlace_Checkbox.IsChecked = block.AllowCrystalAssistedBlockPlacement;
			//if (block.CategoryName != null) Category_TextBox.Text = block.CategoryName;
			if (block.CategoryName != null) CategorySuggestionBox.SuggestionTextField.Text = block.CategoryName;

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
			dataHasChanged = false;
		}
		public void OpenBlock()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Block | *.block";
			openFileDialog.Multiselect = false;
			openFileDialog.Title = "Select a Block to open.";
			openFileDialog.InitialDirectory = WORKSPACE_ROOT+"\\";


			if(openFileDialog.ShowDialog() == true)
			{
				OpenBlock(openFileDialog.FileName);
			}
			else
			{
				return;
			}
		}
		public void Export()
		{
			ValidationResult result = ValidateExportData();
			CustomBlock block = CompileBlock();

			if (!result.validationSucess)
			{
				string errorLog = String.Join('\n', result.validationErrors);
				MessageBox.Show(errorLog);
				return;
			}

			VistaFolderBrowserDialog folderBrowserDialog = new VistaFolderBrowserDialog();
			if (folderBrowserDialog.ShowDialog() == true)
			{
				JsonManager.WriteCustomBlockJson(folderBrowserDialog.SelectedPath + System.IO.Path.DirectorySeparatorChar + "Properties.json", block);

				string textureDir = folderBrowserDialog.SelectedPath + System.IO.Path.DirectorySeparatorChar + "Textures";
				Directory.CreateDirectory(textureDir);
				textureDir = textureDir + System.IO.Path.DirectorySeparatorChar;

				string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
				string exeDir = System.IO.Path.GetDirectoryName(strExeFilePath);
				string texTool = exeDir + System.IO.Path.DirectorySeparatorChar + "PVRTexToolCLI.exe";

				if (File.Exists(texTool))
				{
					string imageTempPath = exeDir + "\\temp\\" + "temp.png";
					string imageTempDest = exeDir + "\\temp\\" + "temp.dds";
					if (!Directory.Exists(exeDir + "\\temp")) Directory.CreateDirectory(exeDir + "\\temp");

					foreach (TexturePanel panel in texturePanels)
					{
						if (panel.textureType == TexturePanelType.Albedo || panel.textureType == TexturePanelType.Albedo_Small)
						{
							string imagePath = panel.TextureURI.LocalPath;

							File.Copy(imagePath, imageTempPath, true);

							string imageDestPath = textureDir + panel.slotName + ".dds";
							var proc = System.Diagnostics.Process.Start(texTool, GetAlbedoExportCLIParams(imageTempPath, imageTempDest));
							proc.WaitForExit();

							File.Copy(imageTempDest, imageDestPath, true);
						}
						if(panel.textureType == TexturePanelType.RecipePreview)
						{
							string imagePath = panel.TextureURI.LocalPath;

							File.Copy(imagePath, imageTempPath, true);

							string imageDestPath = folderBrowserDialog.SelectedPath + "\\" + panel.slotName + ".dds";

							var proc = System.Diagnostics.Process.Start(texTool, GetAlbedoExportCLIParams(imageTempPath, imageTempDest));
							proc.WaitForExit();

							File.Copy(imageTempDest, imageDestPath, true);
						}
						if (panel.textureType == TexturePanelType.Normal)
						{
							string imagePath = panel.TextureURI.LocalPath;

							File.Copy(imagePath, imageTempPath, true);

							string imageDestPath = textureDir + panel.slotName + ".dds";
							var proc = System.Diagnostics.Process.Start(texTool, GetNormalExportCLIParams(imageTempPath, imageTempDest));
							proc.WaitForExit();

							File.Copy(imageTempDest, imageDestPath, true);
						}
						if (panel.textureType == TexturePanelType.Glow)
						{
							string imagePath = panel.TextureURI.LocalPath;

							File.Copy(imagePath, imageTempPath, true);

							string imageDestPath = textureDir + panel.slotName + ".dds";
							var proc = System.Diagnostics.Process.Start(texTool, GetGlowExportCLIParams(imageTempPath, imageTempDest));
							proc.WaitForExit();

							File.Copy(imageTempDest, imageDestPath, true);
						}
					}
					if(useBlankRecipeImage)
					{
						draw.Bitmap bmp = new draw.Bitmap(512, 512);
						var gpr = draw.Graphics.FromImage(bmp);
						gpr.FillRectangle(draw.Brushes.White, 0, 0, bmp.Width, bmp.Height);
						bmp.Save(imageTempPath);

						string imageDestPath = folderBrowserDialog.SelectedPath + "\\RecipePreview.dds";

						var proc = System.Diagnostics.Process.Start(texTool, GetAlbedoExportCLIParams(imageTempPath, imageTempDest));
						proc.WaitForExit();

						File.Copy(imageTempDest, imageDestPath, true);
					}
					if (autoGenerateSmallAlbedo)
					{
						foreach(TexturePanel panel in texturePanels)
						{
							if(panel != null && panel.textureType == TexturePanelType.Albedo)
							{
								string imagePath = panel.TextureURI.LocalPath;

								File.Copy(imagePath, imageTempPath, true);

								string imageDestPath = textureDir + panel.slotName + "_small.dds";
								var proc = System.Diagnostics.Process.Start(texTool, GetAlbedoResizeExportCLIParams(imageTempPath, imageTempDest));
								proc.WaitForExit();

								File.Copy(imageTempDest, imageDestPath, true);
							}
						}
					}

					File.Delete(imageTempPath);
					File.Delete(imageTempDest);
				}
				else
				{
					MessageBox.Show("Critical Error! Could not locate PVRTexTool, your is installation corrupted. Please re-install the application.");
					return;
				}
				
				MessageBox.Show("Export Complete");
			}

		}
		private bool DiscardUnsavedData()
		{
			if (dataHasChanged)
			{
				if (MessageBox.Show("Close without saving?", "Warning", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
				{
					return false;
				}
			}
			return true;
		}
		private CustomBlock CompileBlock()
		{
			CustomBlock block = new CustomBlock();
			block.Name = Name_TextBox.Text;
			block.CreatorName = CreatorName_TextBox.Text;
			//block.CategoryName = Category_TextBox.Text;
			block.CategoryName = CategorySuggestionBox.SuggestionTextField.Text;
			block.UniqueID = uID;
			block.Yield = (int)Math.Round(Yield_Slider.Value);
			block.SimilarTo = SimliarTo_ComboBox.SelectedIndex + 1;
			block.AnimationSpeed = (int)Math.Round(AnimationSpeed_Slider.Value);
			block.UniqueIDToDrop = UIDToDrop;
			block.Recipe = recipe;
			block.Textures = new TextureSettings();
			block.Textures.Mode = TextureMode_ComboBox.SelectedIndex + 1;
			block.Textures.WithGlowMap = glowMap;
			block.Textures.WithNormals = normalMap;

			return block;
		}

		public bool BlockIsCurrentlyOpen(string path)
		{
			return path == saveDestination;
		}
		#endregion

		#region DataValidation
		private ValidationResult ValidateExportData()
		{
			bool validationSucess = true;
			List<string> validationErrors = new List<string>();


			if (!ValidateName(validationErrors)) validationSucess = false;
			if (!ValidateCreatorName(validationErrors)) validationSucess = false;
			if (!ValidateUniqueID(validationErrors)) validationSucess = false;
			if(!ValidateUniqueIDToDrop(validationErrors)) validationSucess = false;
			if(!ValidateRecipe(validationErrors, true)) validationSucess = false;
			if(!ValidateTexturePanels(validationErrors)) validationSucess = false;

			return new ValidationResult(validationSucess, validationErrors);

		}
		private ValidationResult ValidateSaveData()
		{
			bool validationSucess = true;
			List<string> validationErrors = new List<string>();
			
			if(!PreValidateUniqueID(validationErrors)) validationSucess = false;
			if(!PreValidateUniqueIDToDrop(validationErrors)) validationSucess = false;
			ValidateRecipe(validationErrors, false);

			return new ValidationResult(validationSucess, validationErrors);
		}

		private bool ValidateName(List<string> validationErrors)
		{
			if (Name_TextBox.Text == string.Empty)
			{
				Name_TextBox.BorderBrush = Brushes.Red;
				nameInvalidFlag = true;
				validationErrors.Add("Name field cannot be empty!");
				return false;
			}
			else
			{
				return true;
			}
		}
		private bool ValidateCreatorName(List<string> validationErrors)
		{
			if (CreatorName_TextBox.Text == string.Empty)
			{
				CreatorName_TextBox.BorderBrush = Brushes.Red;
				creatorNameInvalidFlag = true;
				validationErrors.Add("Creator Name field cannot be empty!");
				return false;
			}
			else
			{
				return true;
			}
		}
		private bool ValidateUniqueID(List<string> validationErrors)
		{
			if (UniqueID_Textbox.Text == string.Empty)
			{
				uniqueIDInvalidFlag = true;
				UniqueID_Textbox.BorderBrush = Brushes.Red;
				validationErrors.Add("Unique ID field cannot be empty!");
				return false;
			}
			else
			{
				try
				{
					uID = int.Parse(UniqueID_Textbox.Text);
					if (uID < 1)
					{
						uniqueIDInvalidFlag = true;
						UniqueID_Textbox.BorderBrush = Brushes.Red;
						validationErrors.Add("Unique ID cannot be equal to or less than 1!");
						return false;
					}
					bool uIDInUse = false;
					if(saveDestination != null)
					{
						uIDInUse = workspaceManager.UniqueIDInUse(uID, saveDestination);
					}
					else
					{
						uIDInUse = workspaceManager.UniqueIDInUse(uID);
					}
					if(uIDInUse)
					{
						uniqueIDInvalidFlag = true;
						UniqueID_Textbox.BorderBrush = Brushes.Red;
						validationErrors.Add("Unique ID already in use by another block in the workspace!");
						return false;
					}
					
				}
				catch
				{
					uniqueIDInvalidFlag = true;
					UniqueID_Textbox.BorderBrush = Brushes.Red;
					validationErrors.Add("Failed to Parse Unique ID. Please ensure only numeric characters are used. Do not leave any whitespace between the numbers.");
					return false;
				}
			}
			return true;
		}
		private bool PreValidateUniqueID(List<string> validationErrors)
		{
			if (UniqueID_Textbox.Text == string.Empty) return true;
			try
			{
				uID = int.Parse(UniqueID_Textbox.Text);
				return true;
			}
			catch
			{
				uniqueIDInvalidFlag = true;
				UniqueID_Textbox.BorderBrush = Brushes.Red;
				validationErrors.Add("Failed to Parse Unique ID. Please ensure only numeric characters are used. Do not leave any whitespace between the numbers.");
				return false;
			}
		}
		private bool ValidateUniqueIDToDrop(List<string> validationErrors)
		{
			if (UniqueIDToDrop_TextBox.Text == string.Empty)
			{
				UIDToDrop = -2;
				return true;
			}
			else
			{
				try
				{
					UIDToDrop = int.Parse(UniqueIDToDrop_TextBox.Text);
					if (UIDToDrop > -2)
					{
						uniqueIDToDropInvalidFlag = true;
						UniqueIDToDrop_TextBox.BorderBrush = Brushes.Red;
						validationErrors.Add("Unique ID to Drop cannot be less than -2! Allowed values: -2 Itself, -1 Nothing, 0+ Custom block to drop.");
						return false;
					}
					else
					{
						return true;
					}
				}
				catch
				{
					uniqueIDToDropInvalidFlag = true;
					UniqueIDToDrop_TextBox.BorderBrush = Brushes.Red;
					validationErrors.Add("Failed to Parse Unique IDToDrop! Please ensure only numeric characters are used. Do not leave whitespace between the numbers.");
					return false;
				}
			}
		}
		private bool PreValidateUniqueIDToDrop(List<string> validationErrors)
		{
			if (UniqueIDToDrop_TextBox.Text == string.Empty) return true;
			try
			{
				UIDToDrop = int.Parse(UniqueIDToDrop_TextBox.Text);
				return true;
			}
			catch
			{
				uniqueIDToDropInvalidFlag = true;
				UniqueIDToDrop_TextBox.BorderBrush = Brushes.Red;
				validationErrors.Add("Failed to Parse Unique IDToDrop! Please ensure only numeric characters are used. Do not leave whitespace between the numbers.");
				return false;
			}
		}
		private bool ValidateRecipe(List<string> validationErrors, bool export)
		{
			if (recipe == null || recipe.SizeX == 0 || recipe.SizeY == 0 || recipe.SizeZ == 0)
			{
				if (export)
				{
					if(settingsManager.SupressNoRecipeWarning)
					{
						CreateBlankRecipe();
						return true;
					}
					else
					{
						if(MessageBox.Show("Warning: blocks without a recipe can only be used in VoxelAPI mods. Continue Export?", "Exprot Warning", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							CreateBlankRecipe();
							return true;
						}
						else
						{
							validationErrors.Add("No Recipe detected! Please add one!");
							return false;
						}
					}
				}
			}
			return true;
		}
		private void CreateBlankRecipe()
		{
			recipe = new BlockRecipe();
			recipe.Array = Array.Empty<int>();
			recipe.SizeZ = 0;
			recipe.SizeY = 0;
			recipe.SizeX = 0;
		}
		private bool ValidateTexturePanels(List<string> validationErrors)
		{
			foreach (var panel in texturePanels)
			{
				if(panel.TextureURI == null)
				{
					validationErrors.Add("All Texture Slots must be filled before exporting!");
					return false;
				}
				if(panel.imageSizeInvalidFlag == true)
				{
					validationErrors.Add("All Textures must be the correct size before exporting!");
					return false;
				}
			}
			return true;
		}
		#endregion

		#region TextBoxEventHandling
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
			dataHasChanged = true;
			if (nameInvalidFlag)
			{
				nameInvalidFlag = false;
				Name_TextBox.BorderBrush = SystemColors.ControlDarkBrush;
			}
			if(Name_TextBox.Text != string.Empty)
			{
				NameSuggestionLabel.Visibility = Visibility.Hidden;
			}
			else
			{
				NameSuggestionLabel.Visibility = Visibility.Visible;
			}
		}
		private void CreatorName_TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			dataHasChanged = true;
			if (creatorNameInvalidFlag)
			{
				creatorNameInvalidFlag = false;
				CreatorName_TextBox.BorderBrush = SystemColors.ControlDarkBrush;
			}
			if(CreatorName_TextBox.Text != string.Empty)
			{
				CreatorNameSuggestionLabel.Visibility = Visibility.Hidden;
			}
			else
			{
				CreatorNameSuggestionLabel.Visibility = Visibility.Visible;
			}
		}
		private void UniqueID_Textbox_TextChanged(object sender, TextChangedEventArgs e)
		{
			dataHasChanged = true;
			if (uniqueIDInvalidFlag)
			{
				uniqueIDInvalidFlag = false;
				UniqueID_Textbox.BorderBrush = SystemColors.ControlDarkBrush;
			}
			if(UniqueID_Textbox.Text != string.Empty)
			{
				UniqueIDSuggestionLabel.Visibility = Visibility.Hidden;
			}
			else
			{
				UniqueIDSuggestionLabel.Visibility= Visibility.Visible;
			}
		}
		private void UniqueIDToDrop_TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			dataHasChanged = true;
			if (uniqueIDToDropInvalidFlag)
			{
				uniqueIDToDropInvalidFlag = false;
				UniqueIDToDrop_TextBox.BorderBrush = SystemColors.ControlDarkBrush;
			}
			if(UniqueIDToDrop_TextBox.Text != string.Empty)
			{
				UniqueIDToDropSuggestionLabel.Visibility = Visibility.Hidden;
			}
			else
			{
				UniqueIDToDropSuggestionLabel.Visibility = Visibility.Visible;
			}
		}
		#endregion

		#region TexturePanelMangement
		private void TextureMode_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//dataHasChanged = textureMode == TextureMode_ComboBox.SelectedIndex;

			if (TextureTabWrapPanel == null) return;
			ClearTextureWrapPanel();

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
				AddTexturePanel("updown", TexturePanelType.Albedo);
				if (!autoGenerateSmallAlbedo)
				{
					AddTexturePanel("sides_small", TexturePanelType.Albedo_Small);
					AddTexturePanel("updown_small", TexturePanelType.Albedo_Small);
				}
				if (!useBlankRecipeImage)
					AddTexturePanel("RecipePreview", TexturePanelType.RecipePreview);

				if (WithNormals_CheckBox.IsChecked == true)
					AddNormalMapPanels(TextureMode_ComboBox.SelectedIndex);
				if (WithGlowMaps_CheckBox.IsChecked == true)
					AddGlowMapPanels(TextureMode_ComboBox.SelectedIndex);
			}
			if(TextureMode_ComboBox.SelectedIndex == 2)
			{
				AddTexturePanel("sides", TexturePanelType.Albedo);
				AddTexturePanel("up", TexturePanelType.Albedo);
				AddTexturePanel("down", TexturePanelType.Albedo);
				
				if (!autoGenerateSmallAlbedo)
				{
					AddTexturePanel("sides_small", TexturePanelType.Albedo_Small);
					AddTexturePanel("up_small", TexturePanelType.Albedo_Small);
					AddTexturePanel("down_small", TexturePanelType.Albedo_Small);
				}
				if (!useBlankRecipeImage)
					AddTexturePanel("RecipePreview", TexturePanelType.RecipePreview);


				if (WithNormals_CheckBox.IsChecked == true)
					AddNormalMapPanels(TextureMode_ComboBox.SelectedIndex);
				if (WithGlowMaps_CheckBox.IsChecked == true)
					AddGlowMapPanels(TextureMode_ComboBox.SelectedIndex);
			}
			if(TextureMode_ComboBox.SelectedIndex == 3)
			{
				AddTexturePanel("up", TexturePanelType.Albedo);
				AddTexturePanel("down", TexturePanelType.Albedo);
				AddTexturePanel("left", TexturePanelType.Albedo);
				AddTexturePanel("right", TexturePanelType.Albedo);
				AddTexturePanel("front", TexturePanelType.Albedo);
				AddTexturePanel("back", TexturePanelType.Albedo);

				if (!autoGenerateSmallAlbedo)
				{
					AddTexturePanel("up_small", TexturePanelType.Albedo_Small);
					AddTexturePanel("down_small", TexturePanelType.Albedo_Small);
					AddTexturePanel("left_small", TexturePanelType.Albedo_Small);
					AddTexturePanel("right_small", TexturePanelType.Albedo_Small);
					AddTexturePanel("front_small", TexturePanelType.Albedo_Small);
					AddTexturePanel("back_small", TexturePanelType.Albedo_Small);
				}
				if (!useBlankRecipeImage)
					AddTexturePanel("RecipePreview", TexturePanelType.RecipePreview);

				if (WithNormals_CheckBox.IsChecked == true)
					AddNormalMapPanels(TextureMode_ComboBox.SelectedIndex);
				if (WithGlowMaps_CheckBox.IsChecked == true)
					AddGlowMapPanels(TextureMode_ComboBox.SelectedIndex);
			}
			SortTexturePanel();
		}

		private void ClearTextureWrapPanel()
		{
			foreach(TexturePanel panel in texturePanels)
			{
				panel.TextureURI = null;
				panel.TexturePreview_Image.Source = null;
				panel.TexturePreview_Image = null;
				panel.image = null;
			}
			texturePanels.Clear();
			TextureTabWrapPanel.Children.Clear();
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

		private void AddSmallTextureMapPanels(int textureMode)
		{
			if (textureMode == 0)
			{
				AddTexturePanel("all_small", TexturePanelType.Albedo_Small);
			}
			if (textureMode == 1)
			{
				AddTexturePanel("sides_small", TexturePanelType.Albedo_Small);
				AddTexturePanel("updown_small", TexturePanelType.Albedo_Small);
			}
			if(textureMode == 2)
			{
				AddTexturePanel("sides_small", TexturePanelType.Albedo_Small);
				AddTexturePanel("up_small", TexturePanelType.Albedo_Small);
				AddTexturePanel("down_small", TexturePanelType.Albedo_Small);
			}
			if(textureMode == 3)
			{
				AddTexturePanel("up_small", TexturePanelType.Albedo_Small);
				AddTexturePanel("down_small", TexturePanelType.Albedo_Small);
				AddTexturePanel("left_small", TexturePanelType.Albedo_Small);
				AddTexturePanel("right_small", TexturePanelType.Albedo_Small);
				AddTexturePanel("front_small", TexturePanelType.Albedo_Small);
				AddTexturePanel("back_small", TexturePanelType.Albedo_Small);
			}
		}

		private void WithNormals_CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			dataHasChanged = normalMap != WithNormals_CheckBox.IsChecked;
			normalMap = WithNormals_CheckBox.IsChecked.Value;
			AddNormalMapPanels(TextureMode_ComboBox.SelectedIndex);
		}
		private void WithGlowMaps_CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			dataHasChanged = glowMap != WithGlowMaps_CheckBox.IsChecked;
			glowMap = WithGlowMaps_CheckBox.IsChecked.Value;
			AddGlowMapPanels(TextureMode_ComboBox.SelectedIndex);
		}
		private void WithNormals_CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			dataHasChanged = normalMap != WithNormals_CheckBox.IsChecked;
			normalMap = WithNormals_CheckBox.IsChecked.Value;
			RemoveAllPanelsOfType(TexturePanelType.Normal);
		}
		private void WithGlowMaps_CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			dataHasChanged = glowMap != WithGlowMaps_CheckBox.IsChecked;
			glowMap = WithGlowMaps_CheckBox.IsChecked.Value;
			RemoveAllPanelsOfType(TexturePanelType.Glow);
		}

		public void PopulateTexturePanels(List<string> images)
		{
			foreach (string image in images)
			{
				foreach(TexturePanel texturePanel in texturePanels)
				{
					if(System.IO.Path.GetFileNameWithoutExtension(image) == texturePanel.slotName)
					{
						texturePanel.TextureURI = new Uri(image);
						texturePanel.TexturePreview_Image.Source = new BitmapImage(texturePanel.TextureURI);
					}
				}
			}
		}

		public void SortTexturePanel()
		{
			//var children = TextureTabWrapPanel.Children.Cast<TexturePanel>().OrderBy(e => e.textureType);
			TextureTabWrapPanel.Children.Clear();
			texturePanels.Sort((x, y) => 
			{ 
				if (x == null || y == null) return 0;
				else
				{
					return x.textureType.CompareTo(y.textureType);
				}
			}) ;
			foreach (TexturePanel texturePanel in texturePanels)
			{
				TextureTabWrapPanel.Children.Add(texturePanel);
			}
		}
		#endregion

		#region RecipeButtonHandling
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
					dataHasChanged = true;
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
						dataHasChanged = true;
					}
					else
					{
						MessageBox.Show("Failed to read the file! Please ensure the exported cyube recipe has not been modified in anyway!");
					}
				}
			}
		}

		private void ImportLastRecipe_Click(object sender, RoutedEventArgs e)
		{
			var fileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "cyubeVR\\Saved\\Dev\\Recipe.txt");
			if (File.Exists(fileName))
			{
				BlockRecipe br = JsonManager.ReadRecipe(fileName);
				if (br != null)
				{
					MessageBox.Show("Imported Sucessfully.");
					recipe = br;
					dataHasChanged = true;
				}
			}
			else
			{
				MessageBox.Show("Could no locate the last exported recipe file.");
			}
		}
		#endregion

		#region DDSConversionParams
		private string GetAlbedoExportCLIParams(string sourceImagePath, string targetImagePath)
		{
			return "-i " + sourceImagePath + " -o " + targetImagePath + " -nout -m -f BC3,UBN,sRGB -q pvrtcbest";
		}
		private string GetNormalExportCLIParams(string sourceImagePath, string targetImagePath)
		{
			return "-i " + sourceImagePath + " -o " + targetImagePath + " -nout -m -f BC5,UBN,sRGB -q pvrtcbest";
		}
		private string GetGlowExportCLIParams(string sourceImagePath, string targetImagePath)
		{
			return "-i " + sourceImagePath + " -o " + targetImagePath + " -nout -m -f BC1,UBN,sRGB -q pvrtcbest";
		}
		private string GetAlbedoResizeExportCLIParams(string sourceImagePath, string targetImagePath)
		{
			return "-i " + sourceImagePath + " -o " + targetImagePath + " -nout -m -f BC3,UBN,sRGB -q pvrtcbest -r 512,512";
		}
		#endregion

		#region ButtonEventHandling
		// New Block Button Handling
		private void NewBlockButton_MouseEnter(object sender, MouseEventArgs e)
		{
			NewBlockButton.Background = mouseOverButtonBackground;
		}
		private void NewBlockButton_MouseLeave(object sender, MouseEventArgs e)
		{
			NewBlockButton.Background = defaultButtonBackground;
		}
		private void NewBlockButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			ResetWindow();
			NewBlockButton.Background = mouseDownButtonBackground;
		}
		private void NewBlockButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			NewBlockButton.Background = mouseOverButtonBackground;
		}

		// Open Block Button Hnadling
		private void OpenBlockButton_MouseEnter(object sender, MouseEventArgs e)
		{
			OpenBlockButton.Background = mouseOverButtonBackground;
		}
		private void OpenBlockButton_MouseLeave(object sender, MouseEventArgs e)
		{
			OpenBlockButton.Background = defaultButtonBackground;
		}
		private void OpenBlockButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			OpenBlockButton.Background = mouseDownButtonBackground;
			OpenBlock();
		}
		private void OpenBlockButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			OpenBlockButton.Background = mouseOverButtonBackground;
		}

		// Save Block Button Handling
		private void SaveBlockButton_MouseEnter(object sender, MouseEventArgs e)
		{
			SaveBlockButton.Background = mouseOverButtonBackground;
		}
		private void SaveBlockButton_MouseLeave(object sender, MouseEventArgs e)
		{
			SaveBlockButton.Background = defaultButtonBackground;
		}
		private void SaveBlockButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			SaveBlockButton.Background = mouseDownButtonBackground;
			Save(false);
		}
		private void SaveBlockButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SaveBlockButton.Background = mouseOverButtonBackground;
		}

		// Save Block As Button Handling
		private void SaveAsButton_MouseEnter(object sender, MouseEventArgs e)
		{
			SaveAsButton.Background = mouseOverButtonBackground;
		}
		private void SaveAsButton_MouseLeave(object sender, MouseEventArgs e)
		{
			SaveAsButton.Background= defaultButtonBackground;
		}
		private void SaveAsButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			SaveAsButton.Background = mouseDownButtonBackground;
			Save(true);
		}
		private void SaveAsButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SaveAsButton.Background=(mouseOverButtonBackground);
		}
		
		// Export Block Button Handling
		private void ExportButton_MouseEnter(object sender, MouseEventArgs e)
		{
			ExportButton.Background = mouseOverButtonBackground;
		}
		private void ExportButton_MouseLeave(object sender, MouseEventArgs e)
		{
			ExportButton.Background = defaultButtonBackground;
		}
		private void ExportButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			ExportButton.Background = mouseDownButtonBackground;
			Export();
		}
		private void ExportButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			ExportButton.Background = mouseOverButtonBackground;
		}
		
		// Settings Button Handling
		private void SettingsButton_MouseEnter(object sender, MouseEventArgs e)
		{
			SettingsButton.Background = mouseOverButtonBackground;
		}
		private void SettingsButton_MouseLeave(object sender, MouseEventArgs e)
		{
			SettingsButton.Background = defaultButtonBackground;
		}
		private void SettingsButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			SettingsButton.Background = mouseDownButtonBackground;
			if(!settingsWindowOpen)
			{
				settingsWindow = new SettingsWindow(settingsManager);
				settingsWindow.Show();
				settingsWindowOpen = true;
			}
			
		}
		private void SettingsButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SettingsButton.Background = mouseOverButtonBackground;
		}

		private void RandomButton_Click(object sender, RoutedEventArgs e)
		{
			int rand = RandomNumberGenerator.GetInt32(0, 214748364);
			UniqueID_Textbox.Text = rand.ToString();
		}

		public void ClosingSettingsWindow()
		{
			settingsWindowOpen = false;
		}
		#endregion

		#region DataChangeDetectionEvents
		// Misc Event Handlers
		private void Yield_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			dataHasChanged = yield != Yield_Slider.Value;
			yield = (int)Math.Round(Yield_Slider.Value);
		}
		private void SimliarTo_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			dataHasChanged = similiarTo != SimliarTo_ComboBox.SelectedIndex;
			similiarTo = SimliarTo_ComboBox.SelectedIndex;
		}
		private void AnimationSpeed_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			dataHasChanged= animationSpeed != AnimationSpeed_Slider.Value;
			animationSpeed = (int)Math.Round(AnimationSpeed_Slider.Value);
		}
		private void AllowMove_Checkbox_Checked(object sender, RoutedEventArgs e)
		{
			dataHasChanged = allowMove != AllowMove_Checkbox.IsChecked;
			allowMove = AllowMove_Checkbox.IsChecked.Value;
		}
		private void AllowMove_Checkbox_Unchecked(object sender, RoutedEventArgs e)
		{
			dataHasChanged = allowMove != AllowMove_Checkbox.IsChecked;
			allowMove = AllowMove_Checkbox.IsChecked.Value;
		}
		private void AllowCrystalPlace_Checkbox_Checked(object sender, RoutedEventArgs e)
		{
			dataHasChanged = allowCrystalPlacement != AllowCrystalPlace_Checkbox.IsChecked;
			allowCrystalPlacement = AllowCrystalPlace_Checkbox.IsChecked.Value;
		}
		private void AllowCrystalPlace_Checkbox_Unchecked(object sender, RoutedEventArgs e)
		{
			dataHasChanged = allowCrystalPlacement != AllowCrystalPlace_Checkbox.IsChecked;
			allowCrystalPlacement = AllowCrystalPlace_Checkbox.IsChecked.Value;
		}

		private void BlankRecipeImage_Checkbox_Checked(object sender, RoutedEventArgs e)
		{
			useBlankRecipeImage = BlankRecipeImage_Checkbox.IsChecked.Value;
			RemoveAllPanelsOfType(TexturePanelType.RecipePreview);
		}
		private void BlankRecipeImage_Checkbox_Unchecked(object sender, RoutedEventArgs e)
		{
			useBlankRecipeImage = BlankRecipeImage_Checkbox.IsChecked.Value;
			AddTexturePanel("RecipePreview", TexturePanelType.RecipePreview);
			SortTexturePanel();
		}

		private void AutoGenerateSmallAlbedo_Checkbox_Checked(object sender, RoutedEventArgs e)
		{
			autoGenerateSmallAlbedo = true;
			RemoveAllPanelsOfType(TexturePanelType.Albedo_Small);
		}
		private void AutoGenerateSmallAlbedo_Checkbox_Unchecked(object sender, RoutedEventArgs e)
		{
			autoGenerateSmallAlbedo = false;
			AddSmallTextureMapPanels(TextureMode_ComboBox.SelectedIndex);
			SortTexturePanel();
		}
		#endregion

		// Move to a location that makes more sense
		public string GetWorkspaceRelativePath(string originalPath)
		{
			if (originalPath == WORKSPACE_ROOT) return "Workspace";

			string newPath = "Workspace\\";
			return newPath += System.IO.Path.GetRelativePath(WORKSPACE_ROOT, originalPath);
		}

		public void DisplayErrorMessage(string message)
		{
			MessageBox.Show(message);
		}
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if(!DiscardUnsavedData()) 
			{
				e.Cancel = true;
			}
		}
		public void OpenFileExplorer(string folderPath)
		{
			if (Directory.Exists(folderPath))
			{
				ProcessStartInfo startInfo = new ProcessStartInfo();
				startInfo.Arguments = folderPath;
				startInfo.FileName = "explorer.exe";

				Process.Start(startInfo);
			}
			else
			{
				MessageBox.Show(string.Format("{0} Directory does not exist!", folderPath));
			}
		}
	}
}