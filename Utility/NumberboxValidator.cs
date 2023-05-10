using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CyubeBlockMaker
{
	static class NumberboxValidator
	{
		private static readonly Regex _regex = new Regex("[^0-9-]"); //regex that matches disallowed text
		private static readonly Regex _floatRegex = new Regex("[^0-9.]");
		
		private static bool IsTextAllowed(string text)
		{
			return !_regex.IsMatch(text);
		}
		private static bool IsTextAllowedFloat(string text, string wholeText)
		{
			wholeText = wholeText + text;
			return IsTextAllowedFloat(wholeText);

		}
		private static bool IsTextAllowedFloat(string text)
		{
			bool isMatch = !_floatRegex.IsMatch(text);
			int decimalCount = 0;
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == '.') decimalCount++;
			}

			return (isMatch && decimalCount <= 1);
		}

		public static void PreviewTextInputFloat(TextCompositionEventArgs e, string wholeText)
		{
			e.Handled = !IsTextAllowedFloat(e.Text, wholeText);
		}
		public static void Pasting_EventFloat(DataObjectPastingEventArgs e)
		{
			if (e.DataObject.GetDataPresent(typeof(String)))
			{
				String text = (String)e.DataObject.GetData(typeof(String));
				if (!IsTextAllowedFloat(text))
				{
					e.CancelCommand();
				}
			}
			else
			{
				e.CancelCommand();
			}
		}

		public static void PreviewTextInput(TextCompositionEventArgs e)
		{
			e.Handled = !IsTextAllowed(e.Text);
		}
		public static void Pasting_Event(DataObjectPastingEventArgs e)
		{
			if (e.DataObject.GetDataPresent(typeof(String)))
			{
				String text = (String)e.DataObject.GetData(typeof(String));
				if (!IsTextAllowed(text))
				{
					e.CancelCommand();
				}
			}
			else
			{
				e.CancelCommand();
			}
		}
		
	}
}
