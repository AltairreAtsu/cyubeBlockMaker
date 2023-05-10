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
	/// Interaction logic for MenuItem.xaml
	/// </summary>
	public partial class ControlMenuItem : UserControl
	{
		private Dictionary<string, IControlMenuOption> controlOptions;
		
		public ControlMenuItem()
		{
			InitializeComponent();
			Popup.PlacementTarget = this;
		}

		public void InitializeOptions(List<IControlMenuOption> options)
		{
			controlOptions = new Dictionary<string, IControlMenuOption>();
			foreach(IControlMenuOption option in options)
			{
				controlOptions.Add(option.GetName(), option);

				Label controlLabel = new Label();
				controlLabel.Content = option.GetName();
				controlLabel.Name = option.GetName().Replace(" ", "_");
				controlLabel.MouseEnter += (x, y) => ControlLabelMouseEnter(x, y);
				controlLabel.MouseLeave += (x, y) => ControlLabelMouseLeave(x, y);
				controlLabel.MouseDown += (x, y) => ControlLabelMouseDown(x, y);
				Stack_Panel.Children.Add(controlLabel);
			}
		}

		private void ClosePopup()
		{
			Popup.IsOpen = false;
			Background = (Brush)FindResource("Static_Background");
		}


		// Control Label Event Handlers
		public void ControlLabelMouseEnter(object sender, MouseEventArgs e)
		{
			Label label = (Label)sender;
			label.Background = (Brush)FindResource("Item_Selected");
		}
		public void ControlLabelMouseLeave(object sender, MouseEventArgs e)
		{
			Label label = (Label)sender;
			label.Background = (Brush)FindResource("Static_Background");
		}
		public void ControlLabelMouseDown(object sender, MouseButtonEventArgs e)
		{
			Label label = (Label)sender;
			IControlMenuOption option;

			controlOptions.TryGetValue(label.Name.Replace("_", " "), out option);
			option.Execute();
			ClosePopup();
			label.Background = (Brush)FindResource("Static_Background");
		}
		
		// Event Handlers
		private void Label_MouseEnter(object sender, MouseEventArgs e)
		{
			this.Background = (Brush)FindResource("Item_Selected");
		}

		private void Label_MouseLeave(object sender, MouseEventArgs e)
		{
			if (Popup.IsOpen)
			{
				if (!Stack_Panel.IsMouseOver)
				{
					ClosePopup();
				}
			}
			else
			{
				Background = (Brush)FindResource("Static_Background");
			}
			
		}

		private void Label_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Popup.IsOpen = true;
		}

		private void StackPanel_MouseLeave(object sender, MouseEventArgs e)
		{
			if (Popup.IsOpen)
			{
				if (!IsMouseOver)
				{
					ClosePopup();
				}
			}
		}
	}
}
