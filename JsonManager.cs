using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
	}
}
