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
		private readonly IReadOnlyDictionary<string, GenericType> Generics;

		public TypeIdContainer(TypeId[] typeNames)
		{
			TypeIds = typeNames.ToDictionary(i => i.Name);
			Generics = LsnType.GetBaseGenerics().ToDictionary(g => g.Name);
			/*
			TypeIds.Add(LsnType.Bool_.Id.Name,LsnType.Bool_.Id);
			TypeIds.Add(LsnType.double_.Id.Name, LsnType.double_.Id);
			TypeIds.Add(LsnType.int_.Id.Name, LsnType.int_.Id);
			TypeIds.Add(LsnType.string_.Id.Name, LsnType.string_.Id);*/
		}

		public TypeId GetTypeId(string name)
		{
			if (name.Contains('`'))
			//	return new TypeId(name); // ToDo: use generic types...
			{
				var names = name.Split('`');
				var genericTypeName = names[0];
				var generics = names.Skip(1).Select(GetTypeId).ToList();
				if (!Generics.ContainsKey(genericTypeName))
					throw new ApplicationException();
				var generic = Generics[genericTypeName];
				var type = generic.GetType(generics);
				return type.Id;
			}
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
			using (var writer = new BinaryDataWriter(stream, new UTF8Encoding(false),true))
			{
				// Header
				writer.Write(0x5f3759df); // Signature.

				writer.Write((byte)1);
				writer.Write((ulong)0);

				writer.Write((ushort)Includes.Count);
				foreach (var inc in Includes)
					writer.Write(inc);
				writer.Write((ushort)Usings.Count);
				foreach (var u in Usings)
					writer.Write(u);

				writer.Write((ushort)TypeIds.Length);
				foreach (var id in TypeIds)
					writer.Write(id.Name);
				// End Header
				var resourceSerializer = new ResourceSerializer(TypeIds);

				var typesPart1 = WriteTypesPart1(resourceSerializer);
				var typesPart2 = WriteTypesPart2(resourceSerializer);

				var functions = WriteFunctions(resourceSerializer);

				resourceSerializer.WriteConstantTable(writer);
				writer.Write(typesPart1);
				writer.Write(typesPart2);
				writer.Write(functions);
			}
		}

		private byte[] WriteTypesPart1(ResourceSerializer resourceSerializer)
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = new BinaryDataWriter(stream, new UTF8Encoding(false), true))
				{
					writer.Write((ushort)StructTypes.Count);
					foreach (var type in StructTypes.Values)
						type.Serialize(writer);

					writer.Write((ushort)RecordTypes.Count);
					foreach (var t in RecordTypes.Values)
						t.Serialize(writer);

					writer.Write((ushort)HostInterfaces.Count);
					foreach (var type in HostInterfaces.Values)
						type.Serialize(writer);
				}
				var p = (int)stream.Position;
				stream.Position = 0;
				var buff = new byte[p];
				stream.Read(buff, 0, p);
				return buff;
			}
		}

		private byte[] WriteTypesPart2(ResourceSerializer resourceSerializer)
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = new BinaryDataWriter(stream, new UTF8Encoding(false), true))
				{
					writer.Write((ushort)ScriptObjectTypes.Count);
					foreach (var type in ScriptObjectTypes.Values)
						type.Serialize(writer, resourceSerializer);
				}
				var p = (int)stream.Position;
				stream.Position = 0;
				var buff = new byte[p];
				stream.Read(buff, 0, p);
				return buff;
			}
		}

		private byte[] WriteFunctions(ResourceSerializer resourceSerializer)
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = new BinaryDataWriter(stream, new UTF8Encoding(false), true))
				{
					var fns = Functions.Values.Where(fn => fn is LsnFunction).ToList();
					writer.Write((ushort)fns.Count);
					foreach (var fn in fns.Cast<LsnFunction>())
						fn.Serialize(writer, resourceSerializer);
				}
				var p = (int)stream.Position;
				stream.Position = 0;
				var buff = new byte[p];
				stream.Read(buff, 0, p);
				return buff;
			}
		}

		public static LsnResourceThing Read(Stream stream, string filePath, Func<string, LsnResourceThing> resourceLoader)
		{
			LsnResourceThing res;
			var resourceDeserializer = new Serialization.ResourceDeserializer();
			using (var reader = new BinaryDataReader(stream, new UTF8Encoding(false),true))
			{
				// Header
				var sig = reader.ReadInt32();
				if (sig != 0x5f3759df)
					throw new ApplicationException(); // Change byte order...

				var version = reader.ReadByte();
				if (version != 1)
					throw new ApplicationException();

				var features = reader.ReadUInt64();
				if (features != 0)
					throw new ApplicationException();

				var nIncludes = reader.ReadUInt16();
				var includes = new List<string>(nIncludes);
				for (int i = 0; i < nIncludes; i++)
					includes.Add(reader.ReadString());

				var nUsings = reader.ReadUInt16();
				var usings = new List<string>(nUsings);
				for (int i = 0; i < nUsings; i++)
					usings.Add(reader.ReadString());

				foreach (var u in usings)
				{
					var r = resourceLoader(u);
					resourceDeserializer.LoadFunctions(r.Functions.Values);
					resourceDeserializer.LoadTypes(r.HostInterfaces.Values);
					resourceDeserializer.LoadTypes(r.RecordTypes.Values);
					resourceDeserializer.LoadTypes(r.ScriptObjectTypes.Values);
					resourceDeserializer.LoadTypes(r.StructTypes.Values);
				}

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
				// End Header

				resourceDeserializer.ReadConstantTable(reader); // Constant Table

				// Types Part 1
				var nStructTypes = reader.ReadUInt16();
				var structTypes = new Dictionary<string, StructType>(nStructTypes);
				for (int i = 0; i < nStructTypes; i++)
				{
					var structType = StructType.Read(reader, typeIdContainer);
					structTypes.Add(structType.Name, structType);
				}
				res.StructTypes = structTypes;
				resourceDeserializer.LoadTypes(structTypes.Values);

				var nRecordTypes = reader.ReadUInt16();
				var recordTypes = new Dictionary<string, RecordType>(nRecordTypes);
				for (int i = 0; i < nRecordTypes; i++)
				{
					var rType = RecordType.Read(reader, typeIdContainer);
					recordTypes.Add(rType.Name, rType);
				}
				res.RecordTypes = recordTypes;
				resourceDeserializer.LoadTypes(recordTypes.Values);

				var nHostInterfaces = reader.ReadUInt16();
				var hostInterfaces = new Dictionary<string, HostInterfaceType>(nHostInterfaces);
				for (int i = 0; i < nHostInterfaces; i++)
				{
					var h = HostInterfaceType.Read(reader, typeIdContainer);
					hostInterfaces.Add(h.Name, h);
				}
				res.HostInterfaces = hostInterfaces;
				resourceDeserializer.LoadTypes(hostInterfaces.Values);
				// End Types Part 1

				// Types Part 2
				var nScriptObjectTypes = reader.ReadUInt16();
				var scriptObjectTypes = new Dictionary<string, ScriptObjectType>();
				for (int i = 0; i < nScriptObjectTypes; i++)
				{
					var s = ScriptObjectType.Read(reader, typeIdContainer, filePath, resourceDeserializer);
					scriptObjectTypes.Add(s.Name, s);
				}
				res.ScriptObjectTypes = scriptObjectTypes;
				resourceDeserializer.LoadTypes(scriptObjectTypes.Values);
				// End Types Part 2

				// Functions
				var nFunctions = reader.ReadUInt16();
				var functions = new Dictionary<string, Function>(nFunctions);
				for (int i = 0; i < nFunctions; i++)
				{
					var fn = LsnFunction.Read(reader, typeIdContainer, filePath, resourceDeserializer);
					functions.Add(fn.Name, fn);
				}
				res.Functions = functions;
			}
			resourceDeserializer.ResolveCodeBlocks();
			return res;
		}

	}
}
