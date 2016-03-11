using LSN_Core.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core
{
	/// <summary>
	/// The type of a generic LSN_List.
	/// </summary>
	[Serializable]
	public class LSN_ListType : LSN_ReferenceType
	{
		private LSN_Type _Generic;
		public LSN_Type GenericType { get { return _Generic; } private set { _Generic = value; } }

		internal LSN_ListType(LSN_Type type)
		{
			GenericType = type;
		}

		public override ILSN_Value CreateDefaultValue()
		{
			throw new NotImplementedException();
		}

	}

	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class LSN_ListGeneric : GenericType
	{
		public override string Name => "List";

		internal static readonly LSN_ListGeneric Instance = new LSN_ListGeneric();

		private LSN_ListGeneric() { }

		protected override LSN_Type CreateType(List<LSN_Type> types)
		{
			if (types.Count != 1) throw new ArgumentException("List types must have exactly one generic parameter.");
			return new LSN_ListType(types[0]);
		}
	}
}
