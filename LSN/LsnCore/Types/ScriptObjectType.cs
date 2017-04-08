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
		private readonly IReadOnlyList<Property> Properties;

		// Fields
		private readonly IReadOnlyList<Field> Fields;

		// Methods
		private readonly IReadOnlyDictionary<string,ScriptObjectMethod> ScriptObjectMethods;

		// Events
		private readonly IReadOnlyDictionary<string, EventListener> EventListeners;
		
		// States
		private readonly IReadOnlyDictionary<int,ScriptObjectState> _States;

		private readonly int DefaultStateIndex;


		public IReadOnlyCollection<Field> FieldsB => Fields;


		public ScriptObjectType(TypeId id, TypeId host, IReadOnlyList<Property> properties, IReadOnlyList<Field> fields,
			IReadOnlyDictionary<string,ScriptObjectMethod> methods, IReadOnlyDictionary<string,EventListener> eventListeners,
			IReadOnlyDictionary<int,ScriptObjectState> states, int defaultStateIndex)
		{
			Name = id.Name; Id = id; HostInterface = host; Properties = properties; Fields = fields; ScriptObjectMethods = methods;
			EventListeners = eventListeners; _States = states; DefaultStateIndex = defaultStateIndex;
		}


		public ScriptObjectState GetDefaultState()
			=> _States[DefaultStateIndex];


		public override LsnValue CreateDefaultValue()
			=> LsnValue.Nil;


		public int GetIndex(string name)
		{
			if (Fields.Any(f => f.Name == name))
				return Fields.First(f => f.Name == name).Index;
			else throw new ApplicationException($"The ScriptObject type {Name} does not have a field named {name}.");
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

	}
}
