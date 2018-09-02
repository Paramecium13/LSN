using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	public class MainFile
	{
		public string ObjectFileExtension { get; private set; } = ".obj";
		public IReadOnlyList<string> LibraryDirectories { get; private set; }

		readonly JObject Raw;

		internal MainFile(string path)
		{
			Raw = (JObject)JToken.Parse(File.ReadAllText(path));
		}

		private void Parse()
		{
			JToken token;
			if (Raw.TryGetValue(out token, StringComparison.OrdinalIgnoreCase, "ExternalLibraries", "Libraries", "lib", "libs"))
			{
				if (token.Type == JTokenType.Array)
				{
					var jArray = token as JArray;
					if (!jArray.Values().All(v => v.Type == JTokenType.String))
						throw new ApplicationException("Library paths must be text...");
					LibraryDirectories = jArray.Values<string>().ToList();
				}
				else if(token.Type == JTokenType.String)
					LibraryDirectories = new List<string> { token.Value<string>() };
			}
			if (Raw.TryGetValue(out token, StringComparison.OrdinalIgnoreCase, nameof(ObjectFileExtension), "Object File Extension",
				"ObjectExtension", "Object Extension"))
			{
				if (token.Type != JTokenType.String)
					throw new ApplicationException("Object extension must be a string...");
				ObjectFileExtension = token.Value<string>();
				if (!ObjectFileExtension.StartsWith(".", StringComparison.Ordinal))
					ObjectFileExtension = "." + ObjectFileExtension;
			}
		}
	}
}
