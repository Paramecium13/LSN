using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	[Serializable]
	public class RecordType : LsnType, IHasFieldsType
	{

		private readonly Dictionary<string, LsnType> _Fields;
		public IReadOnlyDictionary<string, LsnType> Fields { get { return _Fields; } }

		public RecordType(string name, Dictionary<string,LsnType> members)
		{
			Name = name; _Fields = members;
		}

		public override ILsnValue CreateDefaultValue()
		{
			throw new NotImplementedException();
		}

	}
}
