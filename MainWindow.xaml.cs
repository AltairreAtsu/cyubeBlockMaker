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
using Newtonsoft.Json.Bson;
using System.Security.Cryptography;
using System.IO.Enumeration;
using System.Threading;
using CyubeBlockMaker;

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
		private FileSystemWatcher workSpaceWatcher;
		private TreeNode fileTree;

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
		private int textureMode = 0;
		private int similiarTo = 0;
		private int animationSpeed = 0;
		private int yield = 0;

		private string saveDestination = "";

		private List<TexturePanel> texturePanels = new List<TexturePanel>();
		private BlockRecipe recipe;

		public MainWindow()
		{
			InitializeComponent();
			mainWindow = this;
			InitializeTextureTab();
			InitializeWorkspace();
			InitializeWatcher();
		}

		private void InitializeTextureTab()
		{
			AddTexturePanel("all", TexturePanelType.Albedo);
			AddTexturePanel("all_small", TexturePanelType.Albedo_Small);
			AddTexturePanel("RecipePreview", TexturePanelType.RecipePreview);
		}
		private void InitializeWorkspace()
		{
			if (!Directory.Exists(WORKSPACE_ROOT+"\\"))
			{
				Directory.CreateDirectory(WORKSPACE_ROOT+"\\");
				// Workspace is empty
				return;
			}
			Dictionary<string, TreeViewItem> outlinerChildren = new Dictionary<string, TreeViewItem>();
			

			DirSearch(WORKSPACE_ROOT+"\\", outlinerChildren);
		}
		private void DirSearch(string sDir, Dictionary<string, TreeViewItem> outlinerChildren)
		{
			try
			{
				TreeNode node;
				TreeViewItem item = new TreeViewItem();
				item.Header = new DirectoryInfo(sDir).Name;

				if (sDir == WORKSPACE_ROOT+"\\")
				{
					// IF Root Start Tree
					fileTree = new TreeNode(new FileNode(WORKSPACE_ROOT, true));
					node = fileTree;


					outlinerChildren.Add(sDir, item);
					Outliner.Items.Add(item);

					node.GetNodeItem().OutlinerEntry = item;
				}
				else
				{
					// Else Find Parent Node and Add node as child
					var dirInfo = new DirectoryInfo(sDir);

					if (dirInfo.Name == "Textures") return;

					var parentNode = fileTree.GetNodeFromPath(dirInfo.Parent.FullName);
					node = parentNode.AddChild(new FileNode(sDir, true));

					// If BLock folder
					bool isBlockFolder = false;
					foreach (string d in Directory.GetDirectories(sDir))
					{
						if (new DirectoryInfo(d).Name == "Textures") isBlockFolder = true;
					}
					if (isBlockFolder)
					{
						// Set Node value and carry on
						node.GetNodeItem().containsBlock = true;
					}
					else
					{
						// Add it to the UI and add a corrosponding ref in the node
						outlinerChildren.Add(sDir + "\\", item);
						outlinerChildren[Directory.GetParent(sDir).FullName + "\\"].Items.Add(item);

						node.GetNodeItem().OutlinerEntry = item;
					}
				}

				foreach (string d in Directory.GetDirectories(sDir))
				{
					foreach (string f in Directory.GetFiles(d))
					{
						if (PathIsBLockFile(f))
						{
							// If it's a block file add it to the node tree
							var child = node.AddChild(new FileNode(f, false));

							BlockLabel label = new BlockLabel();
							label.filePath = f;
							label.SetBlockName(System.IO.Path.GetFileNameWithoutExtension(f));

							TreeViewItem parentItem = outlinerChildren[Directory.GetParent(d).FullName + "\\"];
							parentItem.Items.Add(label);
							label.parent = parentItem;
							// And set a ref to it's element in the UI
							child.GetNodeItem().OutlinerEntry = label;
						}
					}
					DirSearch(d, outlinerChildren);
				}
			}
			catch (System.Exception excpt)
			{
				MessageBox.Show("Error Reading Workspace DirectorY! " + excpt.Message);
				return;
			}
		}
		private bool PathIsBLockFile(string path)
		{
			return System.IO.Path.GetExtension(path) == ".block";
		}
		public bool TryDeleteBlockLabel(BlockLabel label)
		{
			return SearchTreeViewItem(Outliner.Items, label);
		}
		private bool SearchTreeViewItem(ItemCollection itemCollection, BlockLabel label)
		{
			foreach(Object obj in itemCollection)
			{
				if(obj is BlockLabel)
				{
					BlockLabel bl = (BlockLabel)obj;
					itemCollection.Remove(bl);
					return true;
				}
				if(obj is TreeViewItem)
				{
					TreeViewItem treeViewItem = (TreeViewItem)obj;
					SearchTreeViewItem(treeViewItem.Items, label);
				}
			}
			return false;
		}

		private void InitializeWatcher()
		{
			workSpaceWatcher = new FileSystemWatcher(WORKSPACE_ROOT+"\\");
			workSpaceWatcher.IncludeSubdirectories = true;
			workSpaceWatcher.Created += OnCreated;
			workSpaceWatcher.Renamed += OnRenamed;
			workSpaceWatcher.Deleted += OnDeleted;
			workSpaceWatcher.EnableRaisingEvents = true;
		}
		private void OnCreated(object sender, FileSystemEventArgs e)
		{
			this.Dispatcher.BeginInvoke(UIOnCreated, System.Windows.Threading.DispatcherPriority.Normal, e);
		}
		private void OnRenamed(object sender, RenamedEventArgs e)
		{
			this.Dispatcher.BeginInvoke(UIOnRenamed, System.Windows.Threading.DispatcherPriority.Normal, e);
		}
		private void OnDeleted(object sender, FileSystemEventArgs e)
		{
			// If a node was deleted, Search the tree for the path.
			// If it was a dir: remove the dir from the node tree and Outliner and remove any children it had
			// If it was a file: remove the corrosponding block label from the tree
		}

		private void UIOnCreated(FileSystemEventArgs e)
		{
			var dirPath = System.IO.Path.GetDirectoryName(e.FullPath);
			var dirInfo = new DirectoryInfo(dirPath);
			var parrentNode = fileTree.GetNodeFromPath(dirPath);
			TreeViewItem parentOutlinerItem = (TreeViewItem)parrentNode.GetNodeItem().OutlinerEntry;
			bool isDir = System.IO.Path.GetExtension(e.FullPath) == string.Empty;


			if (isDir)
			{
				if (new DirectoryInfo(e.FullPath).Name == "Textures") return;
 				var child = parrentNode.AddChild(new FileNode(e.FullPath, true));
				TreeViewItem newHeaderItem = new TreeViewItem();
				newHeaderItem.Header = dirInfo.Name;
				parentOutlinerItem.Items.Add(newHeaderItem);
				child.GetNodeItem().OutlinerEntry = newHeaderItem;
			}
			else
			{
				if(System.IO.Path.GetExtension(e.FullPath) == ".block")
				{
					if (fileTree.GetNodeFromPath(e.FullPath) != null) return;
					TreeViewItem parentParent = (TreeViewItem)parentOutlinerItem.Parent;
					parentParent.Items.Remove(parentOutlinerItem);
					parrentNode.GetNodeItem().containsBlock = true;
					var child = parrentNode.AddChild(new FileNode(e.FullPath,true));

					BlockLabel newLabel = new BlockLabel();
					newLabel.filePath = e.FullPath;
					newLabel.SetBlockName(System.IO.Path.GetFileNameWithoutExtension(e.FullPath));
					parentParent.Items.Add((newLabel));
					child.GetNodeItem().OutlinerEntry = newLabel;
				}
			}

		}
		private void UIOnRenamed(RenamedEventArgs e)
		{
			var node = fileTree.GetNodeFromPath(e.OldFullPath);
			var nodeItem = node.GetNodeItem();
			var item = (TreeViewItem)nodeItem.OutlinerEntry;

			nodeItem.path = e.FullPath;
			if (nodeItem.isDirectory)
			{
				TreeViewItem items = (TreeViewItem)nodeItem.OutlinerEntry;
				var dirInfo = new DirectoryInfo(e.FullPath);
				items.Header = dirInfo.Name;
			}
			else
			{
				BlockLabel label = (BlockLabel)node.GetNodeItem().OutlinerEntry;
				label.filePath = e.FullPath;
				label.SetBlockName(System.IO.Path.GetFileNameWithoutExtension(e.FullPath));
			}
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
		}

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

			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "block | *.block";
			saveFileDialog.InitialDirectory = WORKSPACE_ROOT+"\\";
			if(saveFileDialog.ShowDialog() == true)
			{
				WriteData(saveFileDialog.FileName, block);
			}
			dataHasChanged = false;
		}
		private void WriteData(string path, CustomBlock block)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();

			JsonManager.WriteCustomBlockJson(path, block);
			path = System.IO.Path.GetDirectoryName(path);

			string textureDir = path + System.IO.Path.DirectorySeparatorChar + "Textures";
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
			if (block.CategoryName != null) Category_TextBox.Text = block.CategoryName;

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
				if (export && MessageBox.Show("Warning: blocks without a recipe can only be used in VoxelAPI mods. Continue Export?", "Exprot Warning", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
				{
					recipe = new BlockRecipe();
					recipe.Array = Array.Empty<int>();
					recipe.SizeZ = 0;
					recipe.SizeY = 0;
					recipe.SizeX = 0;
					return true;
				}
				else
				{
					validationErrors.Add("No Recipe detected! Please add one!");
					return false;
				}
			}
			return true;
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

		private CustomBlock CompileBlock()
		{
			CustomBlock block = new CustomBlock();
			block.Name = Name_TextBox.Text;
			block.CreatorName = CreatorName_TextBox.Text;
			block.CategoryName = Category_TextBox.Text;
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
		private void Category_TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			dataHasChanged= true;
			if(Category_TextBox.Text != string.Empty)
			{
				CategorySuggestionLabel.Visibility = Visibility.Hidden;
			}
			else
			{
				CategorySuggestionLabel.Visibility = Visibility.Visible;
			}
		}

		// Texture Panel Mangement
		private void TextureMode_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//dataHasChanged = textureMode == TextureMode_ComboBox.SelectedIndex;
			textureMode = TextureMode_ComboBox.SelectedIndex;

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

		// DDS Conversion
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
			// Open Settings Window
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

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if(!DiscardUnsavedData()) 
			{
				e.Cancel = true;
			}
		}
	}
}

public struct ValidationResult
{
	public List<string> validationErrors;
	public bool validationSucess;

	public ValidationResult(bool validationSucess, List<string> validationErrors)
	{
		this.validationErrors = validationErrors;
		this.validationSucess = validationSucess;
	}
}

public class TreeNode
{
	TreeNode Parent;
	List<TreeNode> Children = new List<TreeNode>();

	FileNode Item { get; set; }

	public TreeNode(FileNode item)
	{
		Item = item;
	}
	public TreeNode(FileNode item, TreeNode parent)
	{
		Item = item;
		Parent = parent;
	}

	public TreeNode AddChild(FileNode item)
	{
		TreeNode nodeItem = new TreeNode(item, this);
		Children.Add(nodeItem);
		return nodeItem;
	}

	public bool TreeContains(FileNode item)
	{
		if(this.Item.Equals(item) ) return true;
		foreach(TreeNode node in Children)
		{
			if(node.TreeContains(item)) return true;
		}
		return false;
	}

	public TreeNode GetNodeFromPath(string path)
	{
		if (Item.path == path)
		{
			return this;
		}
		foreach(TreeNode node in Children)
		{
			var childNode = node.GetNodeFromPath(path);
			if (childNode != null)
			{
				return childNode;
			}
		}
		return null;
	}

	public List<string> GetAllPaths(List<string> paths)
	{
		paths.Add(this.Item.path);

		foreach(TreeNode node in Children)
		{
			node.GetAllPaths(paths);
		}
		return paths;
	}

	public TreeNode GetParent()
	{
		return Parent;
	}
	public FileNode GetNodeItem()
	{
		return Item;
	}
}

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