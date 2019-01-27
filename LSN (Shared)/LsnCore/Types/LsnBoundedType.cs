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
		public Type Type { get { return typeof(T); } }
		public override bool IsBounded { get { return true; } }
		protected int Size;

		public Func<LsnValue> CreateDefault { get; set; }

		public LsnBoundedType(string name, Func<LsnValue> createDefault)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			Name = name;
			if (createDefault == null)
				throw new ArgumentNullException(nameof(createDefault));
			CreateDefault = createDefault;
		}

		public override LsnValue CreateDefaultValue() => CreateDefault();
	}
}