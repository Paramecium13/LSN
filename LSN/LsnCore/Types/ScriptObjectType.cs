using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	[Serializable]
	public sealed class ScriptObjectType : LsnType
	{
		public readonly bool Unique;

		// Host Interface
		public readonly TypeId HostInterface;

		// Properties
		public readonly IReadOnlyList<Property> Properties;

		// Fields
		public readonly IReadOnlyList<Field> Fields;

		// Methods
		public readonly IReadOnlyList<ScriptObjectMethod> ScriptObjectMethods;

		// Events
		public readonly IReadOnlyList<object> EventListeners;
		
		// States
		private readonly Collection<object> _States;

		private readonly int DefaultStateIndex;

		public object GetDefaultState()
			=> _States[DefaultStateIndex];


		public override LsnValue CreateDefaultValue()
			=> LsnValue.Nil;




	}
}
