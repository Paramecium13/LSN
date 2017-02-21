using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	[Serializable]
	public sealed class ScriptObjectType : LsnType
	{
		public readonly bool Unique;

		public readonly IReadOnlyList<Property> Properties;

		//Fields
		public readonly IReadOnlyList<Field> Fields;

		//Methods


		//States



		public override LsnValue CreateDefaultValue()
			=> LsnValue.Nil;

	}
}
