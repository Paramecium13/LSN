using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.Types
{
	public class RecordType : LSN_Type
	{

		private readonly Dictionary<string, LSN_Type> Members;
		public IReadOnlyDictionary<string, LSN_Type> GetMembers() => Members;

		public RecordType(string name, Dictionary<string,LSN_Type> members)
		{
			Name = name; Members = members;
		}

		public override ILSN_Value CreateDefaultValue()
		{
			throw new NotImplementedException();
		}

	}
}
