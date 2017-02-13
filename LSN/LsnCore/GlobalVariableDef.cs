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
		public readonly bool Watched;


		public GlobalVariableDef(string name, TypeId type, LsnValue initialValue, bool watched = false)
		{
			Name = name; Type = type; InitialValue = initialValue;
		}

		public GlobalVariableDef(string name, TypeId type, bool watched = false)
			:this(name,type,LsnValue.Nil, watched) {}


		internal GlobalVariableDef(string name,LsnType type, LsnValue initialValue, bool watched = false) 
			:this(name,type.Id,initialValue,watched){}


		internal GlobalVariableDef(string name, LsnType type, bool watched = false) 
			:this(name, type.Id, type.CreateDefaultValue(), watched) { }

	}
}
