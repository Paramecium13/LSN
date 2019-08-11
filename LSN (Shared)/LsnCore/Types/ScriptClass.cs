using LsnCore.Serialization;
using LsnCore.Values;
using Syroot.BinaryData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LsnCore.Types
{
	public sealed class ScriptClass : LsnReferenceType, IHasFieldsType
	{
		public readonly bool Unique;

		// Host Interface
		public readonly TypeId HostInterface;

		// Properties
		private readonly IList<Property> _Properties = new List<Property>();

		public IReadOnlyList<Property> Properties => (IReadOnlyList<Property>)_Properties;

		// Fields
		public readonly IReadOnlyList<Field> Fields;

		// Methods
		internal IReadOnlyDictionary<string,ScriptClassMethod> ScriptObjectMethods;

		// Events
		internal IReadOnlyDictionary<string, EventListener> EventListeners;

		// States
		internal IReadOnlyDictionary<int,ScriptClassState> _States;

		public readonly int DefaultStateId;

		public IReadOnlyCollection<Field> FieldsB => Fields;

		public readonly ScriptClassConstructor Constructor;

		public int NumberOfProperties => _Properties.Count;

		public int NumberOfFields => Fields.Count;

		public readonly string Metadata;

		public ScriptClass(TypeId id, TypeId host, IReadOnlyList<Field> fields,	IReadOnlyDictionary<string,ScriptClassMethod> methods,
			IReadOnlyDictionary<string,EventListener> eventListeners, IReadOnlyDictionary<int,ScriptClassState> states,
			int defaultStateIndex, bool unique, string meta, ScriptClassConstructor constructor = null)
		{
			Name = id.Name;
			Id = id;
			HostInterface = host;
			Unique = unique;
			Metadata = meta;
			Fields = fields;
			ScriptObjectMethods = methods;
			EventListeners = eventListeners;
			_States = states;
			DefaultStateId = defaultStateIndex;
			Constructor = constructor;

			id.Load(this);

			foreach (var method in ScriptObjectMethods.Values)
			{
				if (method.IsVirtual)
					_Methods.Add(method.Name, method.ToVirtualMethod());
				else _Methods.Add(method.Name, method);
				// Calculate properties...
			}
		}

		public ScriptClassState GetDefaultState()
			=> _States[DefaultStateId];

		public override LsnValue CreateDefaultValue()
			=> LsnValue.Nil;

		int IHasFieldsType.GetIndex(string name)
		{
			if (Fields.Any(f => f.Name == name))
				return Fields.First(f => f.Name == name).Index;
			throw new ApplicationException($"The ScriptObject type {Name} does not have a field named {name}.");
		}

		public int GetFieldIndex(string name)
			=> (this as IHasFieldsType).GetIndex(name);

#if LSNR
		public object GetPropertyInfo(string name)
		{
			throw new NotImplementedException();
		}
#endif

		public bool HasMethod(string name)
			=> ScriptObjectMethods.ContainsKey(name);

		public ScriptClassMethod GetMethod(string name)
		{
			if (ScriptObjectMethods.ContainsKey(name))
				return ScriptObjectMethods[name];
			throw new ArgumentException($"The ScriptObject type \"{Name}\" does not have a method named \"{name}\".", nameof(name));
		}

		public bool HasEventListener(string name) => EventListeners.ContainsKey(name);

		public EventListener GetEventListener(string name) => EventListeners[name];

		public ScriptClassState GetState(int id) => _States[id];

#if CORE
		internal ScriptObject Construct(LsnValue[] arguments, IInterpreter i, IHostInterface host = null)
		{
			var fields = new LsnValue[FieldsB.Count];
			if (Constructor != null)
			{
				var obj = new ScriptObject(fields, this, DefaultStateId, host);
				var args = new LsnValue[arguments.Length + 1];
				arguments.CopyTo(args, 1);
				args[0] = new LsnValue(obj);
				Constructor.Run(i, args);
				return obj;
			}
			for (int j = 0; j < fields.Length; j++)
				fields[j] = arguments[j];
			return new ScriptObject(fields, this, DefaultStateId, host);
		}
#endif

		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(Name);
			writer.Write(Unique);
			writer.Write(HostInterface?.Name ?? "");
			writer.Write(Metadata ?? "");
			writer.Write(DefaultStateId);

			/*writer.Write((ushort)_Properties.Count);
			foreach (var prop in _Properties)
				prop.Write(writer);*/

			writer.Write((ushort)Fields.Count);
			var bv = new BitArray(Fields.Select(f => f.Mutable).ToArray());
			var count = Fields.Count / 8 + (Fields.Count % 8 == 0 ? 0 : 1);
			var buffer = new byte[count];
			bv.CopyTo(buffer, 0);
			writer.Write(buffer);
			foreach (var field in Fields)
			{
				writer.Write(field.Name);
				resourceSerializer.WriteTypeId(field.Type, writer);
			}

			writer.Write((ushort)ScriptObjectMethods.Count);
			foreach (var method in ScriptObjectMethods.Values)
				method.Serialize(writer, resourceSerializer);

			writer.Write((ushort)EventListeners.Count);
			foreach (var listener in EventListeners.Values)
				listener.Serialize(writer, resourceSerializer);

			writer.Write((ushort)_States.Count);
			foreach (var state in _States.Values)
				state.Serialize(writer, resourceSerializer);

			writer.Write(Constructor != null);
			Constructor?.Serialize(writer, resourceSerializer);
		}

		public static ScriptClass Read(BinaryDataReader reader, ITypeIdContainer typeContainer, string resourceFilePath, ResourceDeserializer resourceDeserializer)
		{
			var name = reader.ReadString();
			var unique = reader.ReadBoolean();
			var hostInterfaceTypeName = reader.ReadString();
			var meta = reader.ReadString();
			var defaultStateId = reader.ReadInt32();

			var type = typeContainer.GetTypeId(name);

			/*var nProperties = reader.ReadUInt16();
			var props = new List<Property>(nProperties);
			for (int i = 0; i < nProperties; i++)
				props.Add(Property.Read(reader, typeContainer));*/

			var nFields = reader.ReadUInt16();
			var count = nFields / 8 + (nFields % 8 == 0 ? 0 : 1);
			var bv = new BitArray(reader.ReadBytes(count));
			var fields = new List<Field>();
			for (int i = 0; i < nFields; i++)
			{
				var fName = reader.ReadString();
				var t = resourceDeserializer.LoadTypeId(reader);
				fields.Add(new Field(i, fName, t,bv[i]));
			}

			var nMethods = reader.ReadUInt16();
			var methods = new Dictionary<string, ScriptClassMethod>(nMethods);
			for (int i = 0; i < nMethods; i++)
			{
				var m = ScriptClassMethod.Read(reader, typeContainer, type, resourceFilePath, resourceDeserializer);
				methods.Add(m.Name, m);
			}

			var nListeners = reader.ReadUInt16();
			var listeners = new Dictionary<string, EventListener>();
			for (int i = 0; i < nListeners; i++)
			{
				var listener = EventListener.Read(reader, typeContainer, resourceFilePath, resourceDeserializer);
				listeners.Add(listener.Definition.Name, listener);
			}

			var nStates = reader.ReadUInt16();
			var states = new Dictionary<int, ScriptClassState>(nStates);
			for (int i = 0; i < nStates; i++)
			{
				var state = ScriptClassState.Read(reader, typeContainer, type, resourceFilePath, resourceDeserializer);
				states.Add(state.Id, state);
			}
			ScriptClassConstructor constructor = null;
			if (reader.ReadBoolean())
				constructor = ScriptClassConstructor.Read(reader, resourceFilePath, resourceDeserializer);
			TypeId h = null;
			if(!string.IsNullOrEmpty(hostInterfaceTypeName))
				h = typeContainer.GetTypeId(hostInterfaceTypeName);
			return new ScriptClass(type, h, fields, methods, listeners,
				states, defaultStateId, unique, meta, constructor);
		}

		internal override bool LoadAsMember(ILsnDeserializer deserializer, BinaryDataReader reader, Action<LsnValue> setter)
			=> deserializer.LoadScriptObjectReference(reader.ReadUInt32(), setter);

		internal override void WriteAsMember(LsnValue value, ILsnSerializer serializer, BinaryDataWriter writer)
		{
			writer.Write(serializer.SaveScriptObject(value.Value as ScriptObject));
		}

		internal override void WriteValue(ILsnValue value, ILsnSerializer serializer, BinaryDataWriter writer)
			=> WriteValue((ScriptObject)value, serializer, writer);

		/// <summary>
		/// Does not have a host...
		/// </summary>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		/// <param name="writer"></param>
		internal void WriteValue(ScriptObject value, ILsnSerializer serializer, BinaryDataWriter writer)
		{
			writer.Write(value.CurrentState);
			for (int i = 0; i < Fields.Count; i++)
				Fields[i].Type.Type.WriteAsMember(value.GetFieldValue(i), serializer, writer);
		}

		internal ScriptObject LoadValue(ILsnDeserializer deserializer, BinaryDataReader reader, IHostInterface host)
		{
			var state = reader.ReadInt32();
			var fields = deserializer.GetArray(Fields.Count);
			for(int i = 0; i< Fields.Count; i++)
			{
				var j = i;
				Fields[i].Type.Type.LoadAsMember(deserializer, reader, (v) => fields[j] = v);
			}
			return new ScriptObject(fields, this, state, host);
		}
	}
}
