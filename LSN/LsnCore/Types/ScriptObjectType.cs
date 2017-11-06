using LsnCore.Serialization;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	[Serializable]
	public sealed class ScriptObjectType : LsnType, IHasFieldsType
	{
		public readonly bool Unique;

		// Host Interface
		public readonly TypeId HostInterface;

		// Properties
		private readonly IList<Property> Properties;

		// Fields
		private readonly IReadOnlyList<Field> Fields;

		// Methods
		internal IReadOnlyDictionary<string,ScriptObjectMethod> ScriptObjectMethods;

		// Events
		internal IReadOnlyDictionary<string, EventListener> EventListeners;
		
		// States
		internal IReadOnlyDictionary<int,ScriptObjectState> _States;

		public readonly int DefaultStateId;

		public IReadOnlyCollection<Field> FieldsB => Fields;

		public ScriptObjectType(TypeId id, TypeId host, IList<Property> properties, IReadOnlyList<Field> fields,
			IReadOnlyDictionary<string,ScriptObjectMethod> methods, IReadOnlyDictionary<string,EventListener> eventListeners,
			IReadOnlyDictionary<int,ScriptObjectState> states, int defaultStateIndex, bool unique)
		{
			Name = id.Name;
			Id = id;
			HostInterface = host;
			Unique = unique;
			Properties = properties;
			Fields = fields;
			ScriptObjectMethods = methods;
			EventListeners = eventListeners;
			_States = states;
			DefaultStateId = defaultStateIndex;

			id.Load(this);
		}

		public ScriptObjectState GetDefaultState()
			=> _States[DefaultStateId];

		public override LsnValue CreateDefaultValue()
			=> LsnValue.Nil;


		int IHasFieldsType.GetIndex(string name)
		{
			if (Fields.Any(f => f.Name == name))
				return Fields.First(f => f.Name == name).Index;
			else throw new ApplicationException($"The ScriptObject type {Name} does not have a field named {name}.");
		}

		public int GetFieldIndex(string name)
			=> (this as IHasFieldsType).GetIndex(name);
		
		public int GetPropertyIndex(string name)
		{
			if (Properties.Any(f => f.Name == name))
			{
				var prop = Properties.First(f => f.Name == name);
				return Properties.IndexOf(prop);
			}
			else throw new ApplicationException($"The ScriptObject type {Name} does not have a property named {name}.");
		}

		public bool HasMethod(string name)
			=> ScriptObjectMethods.ContainsKey(name);


		public ScriptObjectMethod GetMethod(string name)
		{
			if (ScriptObjectMethods.ContainsKey(name))
				return ScriptObjectMethods[name];
			throw new ArgumentException($"The ScriptObject type \"{Name}\" does not have a method named \"{name}\".", nameof(name));
		}


		public bool HasEventListener(string name) => EventListeners.ContainsKey(name);


		public EventListener GetEventListener(string name) => EventListeners[name];
		
		
		public ScriptObjectState GetState(int id) => _States[id];



		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(Name);
			writer.Write(Unique);
			writer.Write(HostInterface?.Name ?? "");
			writer.Write(DefaultStateId);

			writer.Write((ushort)Properties.Count);
			foreach (var prop in Properties)
				prop.Write(writer);

			writer.Write((ushort)Fields.Count);
			foreach (var field in Fields)
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
			
		}



		public static ScriptObjectType Read(BinaryDataReader reader, ITypeIdContainer typeContainer, string resourceFilePath, ResourceDeserializer resourceDeserializer)
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
			var methods = new Dictionary<string, ScriptObjectMethod>(nMethods);
			for (int i = 0; i < nMethods; i++)
			{
				var m = ScriptObjectMethod.Read(reader, typeContainer, type, resourceFilePath, resourceDeserializer);
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
			var states = new Dictionary<int, ScriptObjectState>(nStates);
			for (int i = 0; i < nStates; i++)
			{
				var state = ScriptObjectState.Read(reader, typeContainer, type, resourceFilePath, resourceDeserializer);
				states.Add(state.Id, state);
			}

			return new ScriptObjectType(type, typeContainer.GetTypeId(hostInterfaceName), props, fields, methods, listeners,
				states, defaultStateId, unique);
		}

	}
}
