using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	/// <summary>
	/// A(n) LSN type that has fields.
	/// </summary>
	public interface IHasFieldsType
	{
		/// <summary>
		/// The <see cref="TypeId"/> of this <see cref="LsnType"/>.
		/// </summary>
		TypeId Id { get; }

		/// <summary>
		/// Gets the fields.
		/// </summary>
		IReadOnlyCollection<Field> FieldsB { get; }

		/// <summary>
		/// Gets the index of the field <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		int GetIndex(string name);
	}

	/// <summary>
	/// A(n) LSN object that has fields.
	/// </summary>
	public interface IHasFieldsValue
	{
		/// <summary>
		/// Gets the field value for the field at position <paramref name="index"/>.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		LsnValue GetFieldValue(int index);
	}

	/// <summary>
	/// A(n) LSN object with mutable fields.
	/// </summary>
	public interface IHasMutableFieldsValue : IHasFieldsValue
	{
		/// <summary>
		/// Sets the value of the field at <paramref name="index"/> to <paramref name="value"/>.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="value">The value.</param>
		void SetFieldValue(int index, LsnValue value);
	}

}
