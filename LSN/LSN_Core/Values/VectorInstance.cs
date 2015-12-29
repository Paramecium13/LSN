﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core
{
	/// <summary>
	/// A readonly collection passed by value.
	/// </summary>
	public class VectorInstance : LSN_Value
	{

		public readonly int Size;

		private ILSN_Value[] Values;

		public ILSN_Value this [int index] { get { return Values[index].Clone(); } }

		public VectorInstance(VectorType type, ILSN_Value[] values)
		{
			Type = type;
			Size = values.Length;
			Values = values;
		}

		public override ILSN_Value Clone()
		{
			var vals = new ILSN_Value[Size];
			for(int i = 0; i < Size; i++) vals[i] = Values[i].Clone();
			return new VectorInstance((VectorType)Type, vals);
		}

		public override int GetSize()
		{
			throw new NotImplementedException();
		}

		public override string TranslateUniversal()
		{
			var str = new StringBuilder("[");
			foreach (var val in Values) str.Append(val.TranslateUniversal() + ",");
			str.Append("]");
			return str.ToString();	
		}
	}
}
