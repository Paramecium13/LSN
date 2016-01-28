using LSN_Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace LSN_Core
{

	/// <summary>
	/// Is bounded to a type in the underlying engine or the .NET Framework
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	public class LSN_BoundedType<T> : LSN_Type
	{
		/// <summary>
		/// The .NET type that this type is bound to
		/// </summary>
		public Type Type { get { return this.GetType().GetGenericArguments()[0]; } }
		public override bool IsBounded { get { return true; } }
		protected int Size;

		public Func<ILSN_Value> CreateDefault { get; set; }

		public LSN_BoundedType(string name, Func<ILSN_Value> createDefault, params string[] args)
		{
			Name = name;
			CreateDefault = createDefault;
		}

		public override ILSN_Value CreateDefaultValue() => CreateDefault();
	}
}