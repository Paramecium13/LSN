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
		readonly List<string> UsedFiles;

		readonly List<string> TypeNames;

		readonly List<TypeId> TypeIds;

		/// <summary>
		/// Were types added or removed since the last save?
		/// </summary>
		internal bool Changed { get; private set; }

		readonly Dictionary<TypeId, uint> Lookup = new Dictionary<TypeId, uint>();

		SaveFileTypeSegment(string[] files, string[] types)
		{
			UsedFiles = files.ToList(); TypeNames = types.ToList();
			TypeIds = new List<TypeId>();
		}

		void LoadTypes(Func<string, LsnResourceThing> fileLoader)
		{
			var types = new Dictionary<string, TypeId>();
			foreach (var path in UsedFiles)
			{
				var res = fileLoader(path);
				res.LoadTypeIds(types);
			}
			for (int i = 0; i < TypeNames.Count; i++)
			{
				TypeIds.Add(types[TypeNames[i]]);
			}
			// we could clear TypeNames here until it is needed.
		}

		internal TypeId GetTypeId(uint index) => TypeIds[(int)index];

		internal uint GetIndex(TypeId id, bool useLookup = true)
		{
			int index;
			if (useLookup)
			{
				if (!Lookup.ContainsKey(id))
				{
					index = TypeIds.IndexOf(id);
					if (index < 0)
					{
						Lookup.Add(id, (uint)TypeIds.Count);
						// ToDo: Check if its files is in the list...
						TypeIds.Add(id);
						Changed = true;
					}
					else Lookup.Add(id, (uint)index);
					return (uint)index;
				}
				return Lookup[id];
			}
			index = TypeIds.IndexOf(id);
			if (index < 0)
			{
				TypeIds.Add(id);
				// ToDo: Check if its files is in the list...
				Changed = true;
				return ((uint)TypeIds.Count - 1);
			}
			return (uint)index;
		}

		internal void FlushLookup() => Lookup.Clear();

		internal void Save(BinaryDataWriter writer)
		{
			Changed = false;
			writer.Write((ushort)UsedFiles.Count);
			writer.Write(UsedFiles);
			writer.Write(TypeIds.Count);
			writer.Write(TypeIds.Select(t => t.Name));
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
