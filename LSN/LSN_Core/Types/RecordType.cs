using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.Types
{
	[Serializable]
	public class RecordType : LSN_Type, IHasFieldsType
	{

		private readonly Dictionary<string, LSN_Type> _Fields;
		public IReadOnlyDictionary<string, LSN_Type> Fields { get { return _Fields; } }

		public RecordType(string name, Dictionary<string,LSN_Type> members)
		{
			Name = name; _Fields = members;
		}

		public override ILSN_Value CreateDefaultValue()
		{
			throw new NotImplementedException();
		}

	}
}
