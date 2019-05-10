using LsnCore.Types;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LsnCore.Serialization
{
	class SaveFileTypeSegment
	{
		readonly string[] UsedFiles;

		readonly string[] TypeNames;

		readonly TypeId[] TypeIds;

		SaveFileTypeSegment(string[] files, string[] types)
		{
			UsedFiles = files; TypeNames = types;
			TypeIds = new TypeId[TypeNames.Length];
		}

		void LoadTypes(Func<string, LsnResourceThing> fileLoader)
		{
			var types = new Dictionary<string, TypeId>();
			foreach (var path in UsedFiles)
			{
				var res = fileLoader(path);
				res.LoadTypeIds(types);
			}
			for (int i = 0; i < TypeNames.Length; i++)
			{
				TypeIds[i] = types[TypeNames[i]];
			}
		}

		internal static SaveFileTypeSegment Load(BinaryDataReader reader, Func<string, LsnResourceThing> fileLoader)
		{
			var nFiles = reader.ReadUInt16();
			var files = reader.ReadStrings(nFiles);
			var nTypes = reader.ReadInt32();
			var types = reader.ReadStrings(nTypes);
			var segment = new SaveFileTypeSegment(files, types);
			segment.LoadTypes(fileLoader);
			return segment;
		}
	}
}
