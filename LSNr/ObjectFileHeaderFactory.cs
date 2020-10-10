using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSNr.Utilities;

namespace LSNr
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class ObjectFileHeaderFactory
	{
		private readonly TableBuilder<double> DoubleConstTable = new TableBuilder<double>();

		private readonly TableBuilder<string> StringConstTable = new TableBuilder<string>();

		/// <summary>
		/// Adds <paramref name="constant"/> to the constant table if it isn't already in the constant table.
		/// Returns the index of <paramref name="constant"/> in the table.
		/// </summary>
		/// <param name="constant">The constant.</param>
		/// <returns> The index of <paramref name="constant"/> in the constant table. </returns>
		public ushort AddConstant(double constant) => checked((ushort) DoubleConstTable.Add(constant));

		/// <summary>
		/// Adds <paramref name="constant"/> to the constant table if it isn't already in the constant table.
		/// Returns the index of <paramref name="constant"/> in the table.
		/// </summary>
		/// <param name="constant">The constant.</param>
		/// <returns> The index of <paramref name="constant"/> in the constant table. </returns>
		public ushort AddConstant(string constant) => checked((ushort) StringConstTable.Add(constant));
	}
}
