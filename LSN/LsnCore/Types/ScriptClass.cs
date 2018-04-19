using LsnCore.Serialization;
using LsnCore.Values;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LsnCore.Types
{
	public sealed class ScriptClass : LsnType, IHasFieldsType
	{
		public readonly bool Unique;

		// Host Interface
		public readonly TypeId HostInterface;

		// Properties
		private readonly IList<Property> _Properties;

		public IReadOnlyList<Property> Properties => (IReadOnlyList<Property>)_Properties;

		// Fields
		private readonly IReadOnlyList<Field> _Fields;

		// Methods
		internal IReadOnlyDictionary<string,ScriptClassMethod> ScriptObjectMethods;

		// Events
		internal IReadOnlyDictionary<string, EventListener> EventListeners;

		// States
		internal IReadOnlyDictionary<int,ScriptClassState> _States;

		public readonly int DefaultStateId;

		public IReadOnlyCollection<Field> FieldsB => _Fields;

		public readonly ScriptClassConstructor Constructor;

		public int NumberOfProperties => _Properties.Count;

		public int NumberOfFields => _Fields.Count;

		public ScriptClass(TypeId id, TypeId host, IList<Property> properties, IReadOnlyList<Field> fields,
			IReadOnlyDictionary<string,ScriptClassMethod> methods, IReadOnlyDictionary<string,EventListener> eventListeners,
			IReadOnlyDictionary<int,ScriptClassState> states, int defaultStateIndex, bool unique, ScriptClassConstructor constructor = null)
		{
			Name = id.Name;
			Id = id;
			HostInterface = host;
			Unique = unique;
			_Properties = properties;
			_Fields = fields;
			ScriptObjectMethods = methods;
			EventListeners = eventListeners;
			_States = states;
			DefaultStateId = defaultStateIndex;
			Constructor = constructor;

			id.Load(this);
		}

		public ScriptClassState GetDefaultState()
			=> _States[DefaultStateId];

		public override LsnValue CreateDefaultValue()
			=> LsnValue.Nil;

		int IHasFieldsType.GetIndex(string name)
		{
			if (_Fields.Any(f => f.Name == name))
				return _Fields.First(f => f.Name == name).Index;
			else throw new ApplicationException($"The ScriptObject type {Name} does not have a field named {name}.");
		}

		public int GetFieldIndex(string name)
			=> (this as IHasFieldsType).GetIndex(name);

		public int GetPropertyIndex(string name)
		{
			if (_Properties.Any(f => f.Name == name))
			{
				var prop = _Properties.First(f => f.Name == name);
				return _Properties.IndexOf(prop);
			}
			else throw new ApplicationException($"The ScriptObject type {Name} does not have a property named {name}.");
		}

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

		internal ScriptObject Construct(LsnValue[] properties, LsnValue[] arguments, IInterpreter i, IHostInterface host = null)
		{
			var fields = new LsnValue[FieldsB.Count];
			if (Constructor != null)
			{
				var obj = new ScriptObject(properties, fields, this, DefaultStateId, host);
				var args = new LsnValue[arguments.Length + 1];
				arguments.CopyTo(args, 1);
				args[0] = new LsnValue(obj);
				Constructor.Run(i, args);
				return obj;
			}
			else // Copy args over to fields
			{
				for (int j = 0; j < fields.Length; j++)
					fields[j] = arguments[j];
				var obj = new ScriptObject(properties, fields, this, DefaultStateId, host);
				return obj;
			}
		}

		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(Name);
			writer.Write(Unique);
			writer.Write(HostInterface?.Name ?? "");
			writer.Write(DefaultStateId);

			writer.Write((ushort)_Properties.Count);
			foreach (var prop in _Properties)
				prop.Write(writer);

			writer.Write((ushort)_Fields.Count);
			foreach (var field in _Fields)
			{
				writer.Write(field.Name);
				writer.Write(field.Type.Name);
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
			var hostInterfaceName = reader.ReadString();
			var defaultStateId = reader.ReadInt32();

			var type = typeContainer.GetTypeId(name);

			var nProperties = reader.ReadUInt16();
			var props = new List<Property>(nProperties);
			for (int i = 0; i < nProperties; i++)
				props.Add(Property.Read(reader, typeContainer));

			var nFields = reader.ReadUInt16();
			var fields = new List<Field>();
			for (int i = 0; i < nFields; i++)
			{
				var fName = reader.ReadString();
				var fTypeName = reader.ReadString();
				fields.Add(new Field(i, fName, typeContainer.GetTypeId(fTypeName)));
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

			return new ScriptClass(type, typeContainer.GetTypeId(hostInterfaceName), props, fields, methods, listeners,
				states, defaultStateId, unique, constructor);
		}
	}
}
