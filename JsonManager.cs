using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace CyubeBlockMaker
{
	internal class JsonManager
	{
		public static bool WriteCustomBlockJson(string path, CustomBlock block)
		{
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.NullValueHandling = NullValueHandling.Ignore;
			string json = JsonConvert.SerializeObject(block, settings);

			try
			{
				File.WriteAllText(path, json);
				return true;
			}
			catch 
			{
				return false;
			}
		}

		public static CustomBlock? ReadJson(string path)
		{
			try
			{
				string jsonString = File.ReadAllText(path);
				CustomBlock? block = JsonConvert.DeserializeObject<CustomBlock>(jsonString);
				return block;
			}
			catch
			{
				return null;
			}
		}

		public static BlockRecipe? ReadRecipe(string path)
		{
			try
			{
				string jsonString = File.ReadAllText(path);
				jsonString = jsonString.Substring(10);
				jsonString = jsonString.Substring(0, jsonString.Length - 1);
				MessageBox.Show(jsonString);

				BlockRecipe? recipe = JsonConvert.DeserializeObject<BlockRecipe>(jsonString);
				return recipe;
			}
			catch
			{
				return null;
			}
		}
	}
}
