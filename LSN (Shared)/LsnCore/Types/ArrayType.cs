using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore
{
	public class ArrayType : LsnType, ICollectionType
	{
		static ArrayType()
		{
			var intArray = (ArrayType) ArrayGeneric.Instance.GetType(new[] { int_.Id });
			var doubleArray = (ArrayType) ArrayGeneric.Instance.GetType(new[] { double_.Id });

			intArray._Methods.Add("Sum", new BoundedMethod(intArray, int_,
				(args) =>
				{
					int Σ = 0;
					var vector = (ArrayInstance)args[0].Value;
					int length = vector.Length().IntValue;
					for (int i = 0; i < length; i++)
						Σ += (vector[i]).IntValue;
					return new LsnValue(Σ);
				}, "Sum"
			));
			intArray._Methods.Add("Mean", new BoundedMethod(intArray, int_,
				(args) =>
				{
					int Σ = 0;
					var vector = (ArrayInstance)args[0].Value;
					int length = vector.Length().IntValue;
					for (int i = 0; i < length; i++)
						Σ += (vector[i]).IntValue;
					return new LsnValue(length > 0 ? Σ / length : 0);
				}, "Mean"
			));

			doubleArray._Methods.Add("Sum", new BoundedMethod(doubleArray, double_,
				(args) =>
				{
					double Σ = 0;
					var vector = (ArrayInstance)args[0].Value;
					var length = vector.Length().IntValue;
					for (int i = 0; i < length; i++)
						Σ += (vector[i]).DoubleValue;
					return new LsnValue(Σ);
				}, "Sum"
			));
			doubleArray._Methods.Add("Mean", new BoundedMethod(doubleArray, double_,
				(args) =>
				{
					var Σ = 0.0;
					var vector = (ArrayInstance)args[0].Value;
					var length = vector.Length().IntValue;
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

		public LsnType IndexType => int_;

		public LsnType ContentsType => GenericType;

		internal ArrayType(TypeId type, string name)
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
			_Methods.Add("Length", new BoundedMethod(this, int_,
				(args) => ((ArrayInstance)args[0].Value).Length(), "Length"));
			_Methods.Add("ToList",
				new BoundedMethod(this,
					LsnListGeneric.Instance.GetType(new[] { GenericId }),
					(args) => new LsnValue(((ArrayInstance)args[0].Value).ToLsnList())
				, "ToList")
			);
		}

		/// <summary>
		/// Returns a vector of length 0. Not very useful...
		/// </summary>
		/// <returns></returns>
		public override LsnValue CreateDefaultValue()
			=> new LsnValue(new ArrayInstance(this, Array.Empty<LsnValue>()));
	}

	public class ArrayGeneric : GenericType
	{
		public override string Name => "Array";

		internal static readonly ArrayGeneric Instance = new ArrayGeneric();

		private ArrayGeneric() {}

		protected override LsnType CreateType(TypeId[] types)
		{
			if (types == null)
				throw new ArgumentNullException(nameof(types));
			if (types.Length != 1)
				throw new ArgumentException("Vector types must have exactly one generic parameter.");
			return new ArrayType(types[0],GetGenericName(types));
		}

		public override LsnType GetType(TypeId[] types)
		{
			var name = GetGenericName(types);
			if (Types.TryGetValue(name, out var type))
				return type;
			type = CreateType(types);
			if (!Types.ContainsKey(name)) // For some reason this double check is needed to avoid adding duplicate keys.
				Types.Add(name, type);
			((ArrayType) type).SetupMethods();
			return Types[name];
		}
	}

}
