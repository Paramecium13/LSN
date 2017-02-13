using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	public sealed class GlobalVariableDef
	{
		public readonly string Name;
		public readonly TypeId Type;
		public readonly LsnValue InitialValue;

		public GlobalVariableDef(string name, TypeId type, LsnValue initialValue)
		{
			Name = name; Type = type; InitialValue = initialValue;
		}

		public GlobalVariableDef(string name, TypeId type):this(name,type,LsnValue.Nil){}


		internal GlobalVariableDef(string name,LsnType type, LsnValue initialValue) :this(name,type.Id,initialValue){}


		internal GlobalVariableDef(string name, LsnType type) : this(name, type.Id, type.CreateDefaultValue()) { }

	}
}
