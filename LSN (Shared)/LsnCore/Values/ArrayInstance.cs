﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LsnCore.Types;
using Syroot.BinaryData;

namespace LsnCore.Values
{
	/// <summary>
	/// A readonly collection passed by value.
	/// </summary>
	public class ArrayInstance : LsnValueB, ICollectionValue
	{
		public readonly int Size;

		private readonly LsnValue[] Values;

		/// <inheritdoc/>
		public override bool BoolValue => true;

		//private ArrayType _Type;

		/// <summary>
		/// The type of its contents
		/// </summary>
		public readonly TypeId GenericId;

		/// <summary>
		/// Get the value at an index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public LsnValue this [int index] => Values[index].Clone();

		public ArrayInstance(ArrayType type, LsnValue[] values)
			:this(type.Id,type.GenericId,values) {}

		public ArrayInstance(TypeId type, TypeId genericType, LsnValue[] values)
		{
			Type = type; GenericId = genericType; Values = values; Size = Values.Length;
		}

		public override ILsnValue Clone() => this;

		/// <summary>
		/// Get the length of this vector.
		/// </summary>
		/// <returns></returns>
		public LsnValue Length() => new LsnValue(Values.Length);

		/// <summary>
		/// ...
		/// </summary>
		/// <returns></returns>
		public LsnList ToLsnList()
		{
			var vals = new LsnValue[Size];
			for (int i = 0; i < Size; i++) vals[i] = Values[i].Clone();
			return new LsnList(LsnListGeneric.Instance.GetType(new TypeId[] { GenericId })
				as LsnListType, vals);
		}

		public LsnValue GetValue(int index)
			=> Values[index];

		public int GetLength() => Size;

		public override void Serialize(BinaryStream writer)
		{
			writer.Write((byte)ConstantCode.Vector);
			writer.Write(Type.Name);
			writer.Write((ushort)Values.Length);
			for (int i = 0; i < Values.Length; i++)
				Values[i].Serialize(writer);
		}

		public void Serialize(BinaryStream writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.TabledConstant);
			writer.Write(resourceSerializer.TableConstant(this));
		}

		public ILsnEnumerator GetLsnEnumerator() => new LsnCollectionEnumerator(this);
	}

	public sealed class ObjectArrayInstance : LsnValueB,ICollectionValue
	{


		/// <inheritdoc />
		public override bool BoolValue => true;

		public ObjectArrayInstance(ArrayType type, LsnValue[] values) { }

		/// <inheritdoc />
		public override ILsnValue Clone()
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override void Serialize(BinaryStream writer)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public ILsnEnumerator GetLsnEnumerator()
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public LsnValue GetValue(int index)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public int GetLength()
		{
			throw new NotImplementedException();
		}
	}
}
