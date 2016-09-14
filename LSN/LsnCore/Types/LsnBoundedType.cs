using LsnCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace LsnCore
{

	/// <summary>
	/// Is bounded to a type in the underlying engine or the .NET Framework
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	public class LsnBoundedType<T> : LsnType
	{
		/// <summary>
		/// The .NET type that this type is bound to
		/// </summary>
		public Type Type { get { return this.GetType().GetGenericArguments()[0]; } }
		public override bool IsBounded { get { return true; } }
		protected int Size;

		public Func<ILsnValue> CreateDefault { get; set; }

		public LsnBoundedType(string name, Func<ILsnValue> createDefault, params string[] args)
		{
			Name = name;
			CreateDefault = createDefault;
		}

		public override ILsnValue CreateDefaultValue() => CreateDefault();
	}
}