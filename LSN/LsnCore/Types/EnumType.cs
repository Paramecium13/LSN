using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	public class EnumType : LsnType
	{
		private readonly IReadOnlyDictionary<string, int> StoIDict;
		private readonly IReadOnlyDictionary<int, string> ItoSDict;
		private readonly int DefaultValue;

		public EnumType(string name, IEnumerable<KeyValuePair<string,int>> values, int defaultValue)
		{
			Id = new TypeId(name);
			DefaultValue = defaultValue;
			StoIDict = values.ToDictionary(p => p.Key, p => p.Value);
			ItoSDict = values.ToDictionary(p => p.Value, p => p.Key);

			//_Methods.Add(new BoundedMethod(this,LsnType.string_, (a) => new LsnValue(new StringValue(ItoSDict[a[0].IntValue])),"GetName",null))

			Id.Load(this);
		}

		public override LsnValue CreateDefaultValue()
		{
			return new LsnValue(0);
		}
	}
}
