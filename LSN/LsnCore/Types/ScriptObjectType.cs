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
		private readonly IReadOnlyDictionary<string, object> EventListeners;
		
		// States
		private readonly IReadOnlyDictionary<int,ScriptObjectState> _States;

		private readonly int DefaultStateIndex;


		public IReadOnlyCollection<Field> FieldsB => Fields;


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


		public ScriptObjectState GetState(int id) => _States[id];

	}
}
