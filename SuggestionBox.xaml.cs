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
	/// Interaction logic for SuggestionBox.xaml
	/// </summary>
	public partial class SuggestionBox : UserControl
	{
		public List<string> suggestionStrings { get; set; }
		public int targetIndex = 0;
		public bool doAutoComplete = false;

		public static readonly DependencyProperty hintTextProperty = DependencyProperty.Register(
		"HintText", typeof(string),
		typeof(SuggestionBox)
		);

		public string HintText
		{
			get => (string)GetValue(hintTextProperty);
			set => SetValue(hintTextProperty, value);
		}

		public bool SugestionStackIsEmpty
		{
			get { return SuggestionsStack.Children.Count == 0; }
		}

		public SuggestionBox()
		{
			InitializeComponent();
		}

		public void OnWindowMove(object sender, EventArgs args)
		{
			CloseSuggestionBox();
		}

		private void TextBox_KeyUp(object sender, KeyEventArgs e)
		{
			if(SuggestionTextField.Text.Length > 0)
			{
				HintLabel.Visibility = Visibility.Collapsed;
			}
			else
			{
				HintLabel.Visibility = Visibility.Visible;
			}

			if (suggestionStrings == null || doAutoComplete) return;
			if(e.Key == Key.Enter && !SugestionStackIsEmpty)
			{
				TextBlock target = (TextBlock)SuggestionsStack.Children[targetIndex];
				if (target == null) return;
				SuggestionTextField.Text = target.Text;
				SuggestionTextField.CaretIndex = target.Text.Length;
				CloseSuggestionBox();
				targetIndex = 0;
				return;
			}
			if(e.Key == Key.Up && !SugestionStackIsEmpty)
			{
				if (targetIndex == 0) return;
				ClearPreviousTarget();
				targetIndex--;
				SetTarget((TextBlock)SuggestionsStack.Children[targetIndex]);
				ScrollTotarget();
				return;
			}
			if(e.Key == Key.Down && !SugestionStackIsEmpty)
			{
				if (targetIndex == SuggestionsStack.Children.Count -1) return;
				ClearPreviousTarget();
				targetIndex++;
				SetTarget((TextBlock)SuggestionsStack.Children[targetIndex]);
				ScrollTotarget();
				return;
			}
			string query = SuggestionTextField.Text;
			
			SuggestionsStack.Children.Clear();
			SuggestionsStack.Height = 100;
			SuggestionScroller.ScrollToHome();

			if (query.Length == 0)
			{
				CloseSuggestionBox();
				return;
			}
			else
			{
				OpenSuggestionBox();
			}

			int foundSuggestions = 0;
			foreach (string suggestion in suggestionStrings)
			{
				if (suggestion.ToLower().StartsWith(query.ToLower())){
					addItem(suggestion);
					foundSuggestions++;
				}
			}
			if (!SugestionStackIsEmpty)
			{
				TextBlock tb = (TextBlock)SuggestionsStack.Children[0];
				SuggestionsStack.Height = SuggestionsStack.Children.Count * tb.Height;
				SetTarget((TextBlock)SuggestionsStack.Children[0]);
			}
			targetIndex = 0;
		}

		private void ScrollTotarget()
		{
			var target = SuggestionsStack.Children[targetIndex];
			var point = target.TranslatePoint(new Point(), SuggestionsStack);
			SuggestionScroller.ScrollToVerticalOffset(point.Y);
		}

		private void SetTarget(TextBlock newTarget)
		{
			newTarget.Background = (Brush)FindResource("Suggestion_SelectedBackground");
		}
		private void ClearPreviousTarget()
		{
			TextBlock oldTarget = (TextBlock)SuggestionsStack.Children[targetIndex];
			if (oldTarget != null)
			{
				oldTarget.Background = (Brush)FindResource("Suggestion_StaticBackground");
			}
		}

		private void addItem(string text)
		{
			TextBlock block = new TextBlock();

			// Add the text   
			block.Text = text;

			// A little style...   
			block.Margin = new Thickness(2, 3, 2, 3);
			block.Cursor = Cursors.Hand;

			// Mouse events   
			block.MouseLeftButtonUp += (sender, e) =>
			{
				SuggestionTextField.Text = (sender as TextBlock).Text;
				CloseSuggestionBox();
			};

			// Add to the panel   
			SuggestionsStack.Children.Add(block);
		}

		private void OpenSuggestionBox()
		{
			SuggestionPopup.IsOpen = true;
		}

		public void CloseSuggestionBox()
		{
			SuggestionPopup.IsOpen = false;
		}

		private void SuggestionTextField_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if (SuggestionsStack.IsMouseOver)
			{
				return;
			}
			CloseSuggestionBox();
		}
	}
}
