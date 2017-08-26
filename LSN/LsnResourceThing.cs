using LsnCore;
using LsnCore.Expressions;
using LsnCore.Types;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	public interface ITypeIdContainer
	{
		TypeId GetTypeId(string name);
	}


	public class TypeIdContainer : ITypeIdContainer
	{
		private readonly IDictionary<string, TypeId> TypeIds;

		public TypeIdContainer(TypeId[] typeNames)
		{
			TypeIds = typeNames.ToDictionary(i => i.Name);/*
			TypeIds.Add(LsnType.Bool_.Id.Name,LsnType.Bool_.Id);
			TypeIds.Add(LsnType.double_.Id.Name, LsnType.double_.Id);
			TypeIds.Add(LsnType.int_.Id.Name, LsnType.int_.Id);
			TypeIds.Add(LsnType.string_.Id.Name, LsnType.string_.Id);*/
		}

		public TypeId GetTypeId(string name)
		{
			if (name.Contains('`'))
				return new TypeId(name); // ToDo: use generic types...
			return TypeIds[name];
		}
	}

	public interface ITypeContainer : ITypeIdContainer
	{
		LsnType GetType(string name);
		bool TypeExists(string name);
		bool GenericTypeExists(string name);
		GenericType GetGenericType(string name);
	}

	/// <summary>
	/// Contains functions, structs, constants, and macros/inlines.
	/// </summary>
	[Serializable]
	public class LsnResourceThing : LsnScriptBase
	{
		private LsnEnvironment Environment = null;

		private readonly TypeId[] TypeIds;

		public LsnResourceThing(TypeId[] typeIds)
		{
			TypeIds = typeIds;
		}

		public LsnEnvironment GetEnvironment(IResourceManager resourceManager)
		{
			if (Environment == null) Environment = new LsnEnvironment(this, resourceManager);
			return Environment;
		}


		public void Serialize(Stream stream)
		{
			using (var writer = new BinaryDataWriter(stream))
			{
				writer.Write(0x5f3759df); // Signature.


				writer.Write((ushort)Includes.Count);
				foreach (var inc in Includes)
					writer.Write(inc);
				writer.Write((ushort)Usings.Count);
				foreach (var u in Usings)
					writer.Write(u);

				writer.Write((ushort)TypeIds.Length);
				foreach (var id in TypeIds)
					writer.Write(id.Name);

				writer.Write((ushort)StructTypes.Count);
				foreach (var type in StructTypes.Values)
					type.Serialize(writer);

				writer.Write((ushort)RecordTypes.Count);
				foreach (var t in RecordTypes.Values)
					t.Serialize(writer);

				writer.Write((ushort)HostInterfaces.Count);
				foreach (var type in HostInterfaces.Values)
					type.Serialize(writer);

				writer.Write((ushort)ScriptObjectTypes.Count);
				foreach (var type in ScriptObjectTypes.Values)
					type.Serialize(writer);

				var fns = Functions.Values.Where(fn => fn is LsnFunction).ToList();
				writer.Write((ushort)fns.Count);
				foreach (var fn in fns.Cast<LsnFunction>())
					fn.Serialize(writer);

			}
		}


		public static LsnResourceThing Read(Stream stream, string filePath)
		{
			LsnResourceThing res;
			using (var reader = new BinaryDataReader(stream))
			{
				var sig = reader.ReadInt32();
				if (sig != 0x5f3759df)
					throw new ApplicationException(); // Change byte order...

				var nIncludes = reader.ReadUInt16();
				var includes = new List<string>(nIncludes);
				for (int i = 0; i < nIncludes; i++)
					includes.Add(reader.ReadString());

				var nUsings = reader.ReadUInt16();
				var usings = new List<string>(nUsings);
				for (int i = 0; i < nUsings; i++)
					usings.Add(reader.ReadString());

				var nTypes = reader.ReadUInt16();
				var typeNames = new string[nTypes];
				for (int i = 0; i < nTypes; i++)
					typeNames[i] = reader.ReadString();

				var typeIds = typeNames.Select(n => new TypeId(n)).ToArray();
				res = new LsnResourceThing(typeIds)
				{
					Includes = includes,
					Usings = usings
				};
				var typeIdContainer = new TypeIdContainer(typeIds);

				var nStructTypes = reader.ReadUInt16();
				var structTypes = new Dictionary<string, StructType>(nStructTypes);
				for (int i = 0; i < nStructTypes; i++)
				{
					var structType = StructType.Read(reader, typeIdContainer);
					structTypes.Add(structType.Name, structType);
				}
				res.StructTypes = structTypes;

				var nRecordTypes = reader.ReadUInt16();
				var recordTypes = new Dictionary<string, RecordType>(nRecordTypes);
				for (int i = 0; i < nRecordTypes; i++)
				{
					var rType = RecordType.Read(reader, typeIdContainer);
					recordTypes.Add(rType.Name, rType);
				}
				res.RecordTypes = recordTypes;

				var nHostInterfaces = reader.ReadUInt16();
				var hostInterfaces = new Dictionary<string, HostInterfaceType>(nHostInterfaces);
				for (int i = 0; i < nHostInterfaces; i++)
				{
					var h = HostInterfaceType.Read(reader, typeIdContainer);
					hostInterfaces.Add(h.Name, h);
				}
				res.HostInterfaces = hostInterfaces;

				var nScriptObjectTypes = reader.ReadUInt16();
				var scriptObjectTypes = new Dictionary<string, ScriptObjectType>();
				for (int i = 0; i < nScriptObjectTypes; i++)
				{
					var s = ScriptObjectType.Read(reader, typeIdContainer, filePath);
					scriptObjectTypes.Add(s.Name, s);
				}
				res.ScriptObjectTypes = scriptObjectTypes;

				var nFunctions = reader.ReadUInt16();
				var functions = new Dictionary<string, Function>(nFunctions);
				for (int i = 0; i < nFunctions; i++)
				{
					var fn = LsnFunction.Read(reader, typeIdContainer, filePath);
					functions.Add(fn.Name, fn);
				}
				res.Functions = functions;

			}
			return res;
		}

	}
}
