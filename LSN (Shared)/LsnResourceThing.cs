using LsnCore;

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
	/// <summary>
	/// A container of <see cref="TypeId"/>s. Used in serialization and deserialization of object files. 
	/// </summary>
	public interface ITypeIdContainer
	{
		TypeId GetTypeId(string name);
		TypeId GetTypeId(ushort index);
	}

	/// <summary>
	/// A container of <see cref="TypeId"/>s. Used in serialization and deserialization of object files. 
	/// </summary>
	public class TypeIdContainer : ITypeIdContainer
	{
		private readonly IDictionary<string, TypeId> TypeIdDictionary;
		private readonly IDictionary<string, TypeId> GenericInstances = new Dictionary<string, TypeId>();
		private readonly IReadOnlyDictionary<string, GenericType> Generics;
		private readonly TypeId[] TypeIds;

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeIdContainer"/> class.
		/// </summary>
		/// <param name="typeIds">The type ids.</param>
		public TypeIdContainer(TypeId[] typeIds)
		{
			TypeIds = typeIds;
			TypeIdDictionary = typeIds.ToDictionary(i => i.Name);
			Generics = LsnType.GetBaseGenerics().ToDictionary(g => g.Name);
			/*
			TypeIds.Add(LsnType.Bool_.Id.Name,LsnType.Bool_.Id);
			TypeIds.Add(LsnType.double_.Id.Name, LsnType.double_.Id);
			TypeIds.Add(LsnType.int_.Id.Name, LsnType.int_.Id);
			TypeIds.Add(LsnType.string_.Id.Name, LsnType.string_.Id);*/
		}

		/// <inheritdoc/>
		public TypeId GetTypeId(string name)
		{
			if (GenericInstances.ContainsKey(name))
				return GenericInstances[name];
			if (!name.Contains('`')) return TypeIdDictionary[name];
			var i = name.IndexOf('`');
			var names = name.Split('`');
			if (names.Any(n => int.TryParse(n, out int x)))
				throw new NotImplementedException();
			var genericTypeName = name.Substring(0,i);
			if (!Generics.ContainsKey(genericTypeName))
				throw new ApplicationException();
			var generic = Generics[genericTypeName];

			TypeId Bar(GenericType genericType, string contentNames, out string remainingNames, string fullName)
			{
				if (genericType.HasConstNumberOfTypeParams && genericType.NumberOfTypeParams == 1)
				{
					var contentsId = GetTypeId(contentNames);
					var ty = genericType.GetType(new TypeId[] { contentsId });

					remainingNames = null;
					GenericInstances.Add(fullName, ty.Id);
					return ty.Id;
				}
				var contentIds = new TypeId[genericType.NumberOfTypeParams];
				var contentIdsIndex = 0;
				while (contentIdsIndex < genericType.NumberOfTypeParams)
				{
					if (string.IsNullOrEmpty(contentNames))
						throw new ApplicationException();
					var j = contentNames.IndexOf('`');
					var fName = contentNames;
					var tyName = contentNames.Substring(0, j);
					contentNames = contentNames.Substring(j + 1);
					if (Generics.ContainsKey(tyName))
					{
						contentIds[contentIdsIndex++] = GenericInstances.ContainsKey(fullName)
							? GenericInstances[fullName]
							: Bar(Generics[tyName], contentNames, out contentNames, fName);
					}
					else contentIds[contentIdsIndex++] = TypeIdDictionary[tyName];
				}
				remainingNames = contentNames;
				var id = genericType.GetType(contentIds).Id;
				GenericInstances.Add(fullName, id);
				return id;
			}

			return Bar(generic, name.Substring(i + 1), out string z, name);
		}

		/// <inheritdoc/>
		public TypeId GetTypeId(ushort index)
		{
			if (index < TypeIds.Length)
				return TypeIds[index];
			throw new IndexOutOfRangeException();
		}
	}

	/// <summary>
	/// Contains <see cref="LsnType"/>s...
	/// </summary>
	public interface ITypeContainer
	{
		/// <summary>
		/// Gets the <see cref="LsnType"/> named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		LsnType GetType(string name);

		/// <summary>
		/// Gets the <see cref="TypeId"/> for the <see cref="LsnType"/> named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		TypeId GetTypeId(string name);

		/// <summary>
		/// Does an <see cref="LsnType"/> named <paramref name="name"/> exist?
		/// </summary>
		/// <param name="name">The name.</param>
		bool TypeExists(string name);

		/// <summary>
		/// Does a <see cref="GenericType"/> named <paramref name="name"/> exist?
		/// </summary>
		/// <param name="name">The name.</param>
		bool GenericTypeExists(string name);

		/// <summary>
		/// Gets the <see cref="GenericType"/> named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		GenericType GetGenericType(string name);

		/// <summary>
		/// Marks the bound generic type as being used.
		/// </summary>
		/// <param name="typeId">The type identifier.</param>
		void GenericTypeUsed(TypeId typeId);
	}

	/// <summary>
	/// Contains functions, structs, constants, and macros/inlines.
	/// </summary>
	/*public class LsnResourceThing : LsnScriptBase
	{
		private LsnEnvironment Environment;

		private readonly TypeId[] TypeIds;

		public LsnResourceThing(TypeId[] typeIds)
		{
			TypeIds = typeIds;
		}

		public LsnEnvironment GetEnvironment(IResourceManager resourceManager)
		{
			return Environment ?? (Environment = new LsnEnvironment(this, resourceManager));
		}

		public void Serialize(Stream stream)
		{
			using (var writer = new BinaryDataWriter(stream, new UTF8Encoding(false),true))
			{
				// Header
				writer.Write(0x5f3759df); // Signature.

				writer.Write((byte)1);
				writer.Write((ulong)0);

				writer.Write((ushort)Usings.Count); // Usings
				foreach (var u in Usings)
					writer.Write(u);

				writer.Write((ushort)TypeIds.Length); // TypeIds
				foreach (var id in TypeIds)
					writer.Write(id.Name);
				// End Header
				var resourceSerializer = new ResourceSerializer(TypeIds);

				WriteGameValues(writer, resourceSerializer);

				var types = WriteTypes(resourceSerializer);

				var functions = WriteFunctions(resourceSerializer);

				resourceSerializer.WriteConstantTable(writer); // Constants
				writer.Write(types); // Types
				writer.Write(functions); // Functions
			}
		}

		private void WriteGameValues(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((ushort)(GameValues?.Count ?? 0));
			if (GameValues == null) return;
			foreach (var gameValue in GameValues.Values)
				gameValue.Serialize(writer, resourceSerializer);
		}

		private byte[] WriteTypes(ResourceSerializer resourceSerializer)
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

					writer.Write((ushort)HandleTypes.Count);
					foreach (var handle in HandleTypes)
						handle.Serialize(writer, resourceSerializer);

					writer.Write((ushort)HostInterfaces.Count);
					foreach (var type in HostInterfaces.Values)
						type.Serialize(writer, resourceSerializer);

					writer.Write((ushort)ScriptClassTypes.Count);
					foreach (var type in ScriptClassTypes.Values)
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

		public static LsnResourceThing Read(Stream stream, string path, Func<string, LsnResourceThing> resourceLoader)
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

				var nUsings = reader.ReadUInt16();
				var usings = new List<string>(nUsings);
				for (int i = 0; i < nUsings; i++)
					usings.Add(reader.ReadString());

				foreach (var u in usings)
				{
					var r = resourceLoader(u);
					resourceDeserializer.LoadFunctions(r.Functions.Values);
					resourceDeserializer.LoadTypes(r.HandleTypes);
					resourceDeserializer.LoadTypes(r.HostInterfaces.Values);
					resourceDeserializer.LoadTypes(r.RecordTypes.Values);
					resourceDeserializer.LoadTypes(r.ScriptClassTypes.Values);
					resourceDeserializer.LoadTypes(r.StructTypes.Values);
				}

				var typeIds = resourceDeserializer.LoadTypeIds(reader);
				res = new LsnResourceThing(typeIds)
				{
					Usings = usings
				};
				var typeIdContainer = new TypeIdContainer(typeIds);
				// End Header

				var nGameValues = reader.ReadUInt16();
				var gameValues = new Dictionary<string, GameValue>();
				for (int i = 0; i < nGameValues; i++)
				{
					var gameValue = GameValue.Read(reader, resourceDeserializer);
					gameValues.Add(gameValue.Name, gameValue);
				}

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

				var nHandleTypes = reader.ReadUInt16();
				var handleTypes = new HandleType[nHandleTypes];
				for (int i = 0; i < nHandleTypes; i++)
					handleTypes[i] = HandleType.Read(reader, typeIdContainer);
				res.HandleTypes = handleTypes.ToList();
				resourceDeserializer.LoadTypes(handleTypes);

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
				var scriptObjectTypes = new Dictionary<string, ScriptClass>();
				for (int i = 0; i < nScriptObjectTypes; i++)
				{
					var s = ScriptClass.Read(reader, typeIdContainer, path, resourceDeserializer);
					scriptObjectTypes.Add(s.Name, s);
				}
				res.ScriptClassTypes = scriptObjectTypes;
				resourceDeserializer.LoadTypes(scriptObjectTypes.Values);
				// End Types Part 2

				// Functions
				var nFunctions = reader.ReadUInt16();
				var functions = new Dictionary<string, Function>(nFunctions);
				for (int i = 0; i < nFunctions; i++)
				{
					var fn = LsnFunction.Read(reader, typeIdContainer, path, resourceDeserializer);
					functions.Add(fn.Name, fn);
				}
				res.Functions = functions;
			}
			resourceDeserializer.ResolveCodeBlocks();
			return res;
		}
	}*/
}
