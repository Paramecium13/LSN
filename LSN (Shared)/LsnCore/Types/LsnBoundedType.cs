using LsnCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Syroot.BinaryData;

namespace LsnCore
{
	/// <summary>
	/// Is bounded to a type in the underlying engine or the .NET Framework
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class LsnBoundedType<T> : LsnType
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
			Name = name ?? throw new ArgumentNullException(nameof(name));
			CreateDefault = createDefault ?? throw new ArgumentNullException(nameof(createDefault));
		}

		public override LsnValue CreateDefaultValue() => CreateDefault();
	}

	class I32Type : LsnBoundedType<int>
	{
		public static readonly I32Type Instance = new I32Type();
		I32Type() : base("int", () => new LsnValue(0)) { }

		internal override bool LoadAsMember(ILsnDeserializer deserializer, BinaryDataReader reader, Action<LsnValue> setter)
		{
			setter(new LsnValue(reader.ReadInt32()));
			return true;
		}

		internal override void WriteAsMember(LsnValue value, ILsnSerializer serializer, BinaryDataWriter writer)
		{
			writer.Write(value.IntValue);
		}
	}

	class F64Type : LsnBoundedType<double>
	{
		public static readonly F64Type Instance = new F64Type();
		F64Type() : base("double", () => new LsnValue(0.0)) { }

		internal override bool LoadAsMember(ILsnDeserializer deserializer, BinaryDataReader reader, Action<LsnValue> setter)
		{
			setter(new LsnValue(reader.ReadDouble()));
			return true;
		}

		internal override void WriteAsMember(LsnValue value, ILsnSerializer serializer, BinaryDataWriter writer)
		{
			writer.Write(value.DoubleValue);
		}
	}

	class StringType : LsnBoundedType<string>
	{
		public static readonly StringType Instance = new StringType();
		static readonly StringValue Empty = new StringValue("");
		StringType():base("string",() => new LsnValue()){}

		internal override bool LoadAsMember(ILsnDeserializer deserializer, BinaryDataReader reader, Action<LsnValue> setter)
			=> deserializer.LoadString(reader.ReadUInt32(), setter);

		internal override void WriteAsMember(LsnValue value, ILsnSerializer serializer, BinaryDataWriter writer)
		{
			writer.Write(serializer.SaveString((value.Value as StringValue).Value));
		}
	}
}