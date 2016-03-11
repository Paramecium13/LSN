using LSN_Core.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core
{
	[Serializable]
	public class VectorType : LSN_Type
	{

		private static Dictionary<LSN_Type, VectorType> Vectors = new Dictionary<LSN_Type, VectorType>();

		/// <summary>
		/// Get the vector type of a type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static VectorType GetVectorType(LSN_Type type)
		{
			if (!Vectors.ContainsKey(type)) Vectors[type] = new VectorType(type);
			return Vectors[type];
		}



		private LSN_Type _Generic;
		public LSN_Type GenericType { get { return _Generic; } private set { _Generic = value; } }

		internal VectorType(LSN_Type type)
		{
			GenericType = type;
		}

		/// <summary>
		/// Returns a vector of length 0. Not very useful...
		/// </summary>
		/// <returns></returns>
		public override ILSN_Value CreateDefaultValue()
			=> new VectorInstance(this, new ILSN_Value[0]);

	}

	public class VectorGeneric : GenericType
	{
		public override string Name => "Vector";

		internal VectorGeneric() { }

		protected override LSN_Type CreateType(List<LSN_Type> types)
		{
			if (types.Count != 1) throw new ArgumentException("Vector types must have exactly one generic parameter.");
			return new VectorType(types[0]);
		}
	}

}
