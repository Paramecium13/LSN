using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{

	public delegate void OnGlobalVariableValueChanged(LsnValue oldValue, LsnValue newValue);

	[Serializable]
	public sealed class GlobalVariableValueWatched : GlobalVariableValue
	{

		public event OnGlobalVariableValueChanged OnValueChanged;

		public override LsnValue Value
		{
			get { return _Value; }
			set
			{
				if(!value.Equals(_Value))
				{
					var old = _Value;
					_Value = value;
					OnValueChanged?.Invoke(old, value);
				}
			}
		}



		public GlobalVariableValueWatched(string name, LsnValue value) : base(name, value){}


		

	}
}
