using LsnCore.Types;
using LsnCore.Values;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore
{
	public class VectorType : LsnReferenceType, ICollectionType
	{
		static VectorType()
		{
			var vectorInt = VectorGeneric.Instance.GetType(new TypeId[] { Int_.Id }) as VectorType;
			var vectorDouble = VectorGeneric.Instance.GetType(new TypeId[] { Double_.Id }) as VectorType;

			vectorInt._Methods.Add("Sum", new BoundedMethod(vectorInt, Int_,
				(args) =>
				{
					int Σ = 0;
					var vector = (VectorInstance)args[0].Value;
					int length = vector.Length().IntValue;
					for (int i = 0; i < length; i++)
						Σ += (vector[i]).IntValue;
					return new LsnValue(Σ);
				}, "Sum"
			));
			vectorInt._Methods.Add("Mean", new BoundedMethod(vectorInt, Int_,
				(args) =>
				{
					int Σ = 0;
					var vector = (VectorInstance)args[0].Value;
					int length = vector.Length().IntValue;
					for (int i = 0; i < length; i++)
						Σ += (vector[i]).IntValue;
					return new LsnValue(length > 0 ? Σ / length : 0);
				}, "Mean"
			));

			vectorDouble._Methods.Add("Sum", new BoundedMethod(vectorDouble, Double_,
				(args) =>
				{
					double Σ = 0;
					var vector = (VectorInstance)args[0].Value;
					int length = vector.Length().IntValue;
					for (int i = 0; i < length; i++)
						Σ += (vector[i]).DoubleValue;
					return new LsnValue(Σ);
				}, "Sum"
			));
			vectorDouble._Methods.Add("Mean", new BoundedMethod(vectorDouble, Double_,
				(args) =>
				{
					double Σ = 0.0;
					var vector = (VectorInstance)args[0].Value;
					int length = vector.Length().IntValue;
					for (int i = 0; i < length; i++)
						Σ += (vector[i]).DoubleValue;
					return new LsnValue(length > 0 ? Σ / length : 0);
				}, "Mean"
			));
		}

		public LsnType GenericType => GenericId.Type;

		/// <summary>
		/// The type id of the generic type parameter (e.g. int or string).
		/// </summary>
		public readonly TypeId GenericId;

		public LsnType IndexType => Int_;

		public LsnType ContentsType => GenericType;

		internal VectorType(TypeId type, string name)
		{
			if (type == null)
				throw new ApplicationException();
			if (type.Name == null)
				throw new ApplicationException();

			Name = name;
			GenericId = type;
		}

		internal void SetupMethods()
		{
			_Methods.Add("Length", new BoundedMethod(this, Int_,
				(args) => ((VectorInstance)args[0].Value).Length(), "Length"));
			_Methods.Add("ToList",
				new BoundedMethod(this,
					LsnListGeneric.Instance.GetType(new TypeId[] { GenericId }),
					(args) => new LsnValue(((VectorInstance)args[0].Value).ToLsnList())
				, "ToList")
			);
		}

		/// <summary>
		/// Returns a vector of length 0. Not very useful...
		/// </summary>
		/// <returns></returns>
		public override LsnValue CreateDefaultValue()
			=> new LsnValue(new VectorInstance(this, new LsnValue[0]));

		internal override bool LoadAsMember(ILsnDeserializer deserializer, BinaryDataReader reader, Action<LsnValue> setter)
			=> deserializer.LoadReference(reader.ReadUInt32(), setter);

		internal override void WriteAsMember(LsnValue value, ILsnSerializer serializer, BinaryDataWriter writer)
			=> writer.Write(serializer.SaveVector(value.Value));

		internal override void WriteValue(ILsnValue value, ILsnSerializer serializer, BinaryDataWriter writer)
			=> WriteValue((VectorInstance)value, serializer, writer);

		internal void WriteValue(VectorInstance value, ILsnSerializer serializer, BinaryDataWriter writer)
		{
			var len = value.GetLength();
			writer.Write(len);
			for (int i = 0; i < len; i++)
			{
				GenericType.WriteAsMember(value.GetValue(i), serializer, writer);
			}
		}

		internal VectorInstance LoadValue(ILsnDeserializer deserializer, BinaryDataReader reader)
		{
			var len = reader.ReadInt32();
			var vals = new LsnValue[len];
			for (int i = 0; i < len; i++)
			{
				var j = i;
				GenericType.LoadAsMember(deserializer, reader, (x) => vals[i] = x);
			}
			return new VectorInstance(this, vals);
		}
	}

	public class VectorGeneric : GenericType
	{
		public override string Name => "Vector";

		internal static readonly VectorGeneric Instance = new VectorGeneric();

		private VectorGeneric() {}

		protected override LsnType CreateType(TypeId[] types)
		{
			if (types == null)
				throw new ArgumentNullException(nameof(types));
			if (types.Length != 1)
				throw new ArgumentException("Vector types must have exactly one generic parameter.");
			return new VectorType(types[0],GetGenericName(types));
		}

		public override LsnType GetType(TypeId[] types)
		{
			var name = GetGenericName(types);
			LsnType type = null;
			if (Types.TryGetValue(name, out type))
				return type;
			type = CreateType(types);
			if (!Types.ContainsKey(name)) // For some reason this double check is needed to avoid adding duplicate keys.
				Types.Add(name, type);
			(type as VectorType).SetupMethods();
			return Types[name];
		}
	}

}
