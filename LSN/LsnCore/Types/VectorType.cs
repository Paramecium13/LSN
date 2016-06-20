using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore
{
	[Serializable]
	public class VectorType : LsnType, ICollectionType
	{

		static VectorType()
		{
			var vectorInt = VectorGeneric.Instance.GetType(new List<LsnType>() { int_ }) as VectorType;
			var vectorDouble = VectorGeneric.Instance.GetType(new List<LsnType>() { double_ }) as VectorType;

			vectorInt._Methods.Add("Sum", new BoundedMethod(vectorInt, int_,
				(args) =>
				{
					int Σ = 0;
					var vector = (VectorInstance)args["self"];
					int length = vector.Length().Value;
					for (int i = 0; i < length; i++)
						Σ += ((IntValue)vector[i]).Value;
					return new IntValue(Σ);
				}
			));
			vectorInt._Methods.Add("Mean", new BoundedMethod(vectorInt, int_,
				(args) =>
				{
					int Σ = 0;
					var vector = (VectorInstance)args["self"];
					int length = vector.Length().Value;
					for (int i = 0; i < length; i++)
						Σ += ((IntValue)vector[i]).Value;
					return new IntValue(length > 0 ? Σ / length : 0);
				}
			));

			vectorDouble._Methods.Add("Sum", new BoundedMethod(vectorDouble, double_,
				(args) =>
				{
					double Σ = 0;
					var vector = (VectorInstance)args["self"];
					int length = vector.Length().Value;
					for (int i = 0; i < length; i++)
						Σ += ((DoubleValue)vector[i]).Value;
					return new DoubleValue(Σ);
				}
			));
			vectorDouble._Methods.Add("Mean", new BoundedMethod(vectorDouble, double_,
				(args) =>
				{
					double Σ = 0.0;
					var vector = (VectorInstance)args["self"];
					int length = vector.Length().Value;
					for (int i = 0; i < length; i++)
						Σ += ((DoubleValue)vector[i]).Value;
					return new DoubleValue(length > 0 ? Σ / length : 0);
				}
			));
		}


		private LsnType _Generic;
		public LsnType GenericType { get { return _Generic; } private set { _Generic = value; } }

		public LsnType IndexType => int_;

		public LsnType ContentsType => GenericType;

		internal VectorType(LsnType type)
		{
			GenericType = type;
			_Methods.Add("Length", new BoundedMethod(this, int_,
				(args) => ((VectorInstance)args["self"]).Length()));
			_Methods.Add("ToList", new BoundedMethod(this, LsnListGeneric.Instance.GetType(new List<LsnType>() { type }),
				(args) => ((VectorInstance)args["self"]).ToLSN_List()));
		}

		/// <summary>
		/// Returns a vector of length 0. Not very useful...
		/// </summary>
		/// <returns></returns>
		public override ILsnValue CreateDefaultValue()
			=> new VectorInstance(this, new ILsnValue[0]);

	}

	public class VectorGeneric : GenericType
	{
		public override string Name => "Vector";

		internal static readonly VectorGeneric Instance = new VectorGeneric();

		private VectorGeneric() {}

		protected override LsnType CreateType(List<LsnType> types)
		{
			if (types.Count != 1) throw new ArgumentException("Vector types must have exactly one generic parameter.");
			return new VectorType(types[0]);
		}
	}

}
