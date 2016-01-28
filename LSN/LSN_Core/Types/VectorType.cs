using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core
{
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

		private VectorType(LSN_Type type)
		{
			GenericType = type;
		}


		public override ILSN_Value CreateDefaultValue()
		{
			throw new NotImplementedException();
		}

	}
}
