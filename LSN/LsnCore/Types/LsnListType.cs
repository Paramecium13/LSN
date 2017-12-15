using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LsnCore
{
	/// <summary>
	/// The type of a generic LSN_List.
	/// </summary>
	public class LsnListType : LsnReferenceType, ICollectionType
	{
		static LsnListType()
		{
			// Set up methods
			var listInt = LsnListGeneric.Instance.GetType(new List<TypeId>() { int_.Id }) as LsnListType;
			var listDouble = LsnListGeneric.Instance.GetType(new List<TypeId>() { double_.Id }) as LsnListType;

			listInt._Methods.Add("Sum", new BoundedMethod(listInt, int_,
				(args) =>
				{
					int Σ = 0;
					var list = (LsnList)args[0].Value;
					int length = list.Length().IntValue;
					for (int i = 0; i < length; i++)
						Σ += (list[i]).IntValue;
					return new LsnValue(Σ);
				}, "Sum"
			));
			listInt._Methods.Add("Mean", new BoundedMethod(listInt, int_,
				(args) =>
				{
					int Σ = 0;
					var list = (LsnList)args[0].Value;
					int length = list.Length().IntValue;
					for (int i = 0; i < length; i++)
						Σ += (list[i]).IntValue;
					return new LsnValue(length > 0 ? Σ / length : 0);
				}, "Mean"
			));

			listDouble._Methods.Add("Sum", new BoundedMethod(listDouble, double_,
				(args) =>
				{
					double Σ = 0;
					var list = (LsnList)args[0].Value;
					int length = list.Length().IntValue;
					for (int i = 0; i < length; i++)
						Σ += (list[i]).DoubleValue;
					return new LsnValue(Σ);
				}, "Sum"
			));
			listDouble._Methods.Add("Mean", new BoundedMethod(listDouble, double_,
				(args) =>
				{
					double Σ = 0.0;
					var list = (LsnList)args[0].Value;
					int length = list.Length().IntValue;
					for (int i = 0; i < length; i++)
						Σ += (list[i]).DoubleValue;
					return new LsnValue(length > 0 ? Σ / length : 0);
				}, "Mean"
			));
		}

		[NonSerialized]
		private LsnType _Generic;
		public LsnType GenericType {
			get
			{
				if (_Generic == null)
					_Generic = GenericId.Type;
				return _Generic;
			}
			private set { _Generic = value; }
		}

		public readonly TypeId GenericId;

		public LsnType IndexType => int_;

		public LsnType ContentsType => GenericType;

		internal LsnListType(TypeId type)
		{
			Id = new TypeId("List`" + type.Name);
			GenericType = type.Type;
			GenericId = type;
			// Set up methods
			_Methods.Add("Add", new BoundedMethod(this, null,
				(args) =>
				{
					((LsnList)args[0].Value).Add(args[1]);
					return LsnValue.Nil;
				}, "Add",
				new List<Parameter> { new Parameter("self",this, LsnValue.Nil, 0), new Parameter("value",type, LsnValue.Nil, 1)}
			));
			_Methods.Add("Length", new BoundedMethod(this, int_, (args) => ((LsnList)args[0].Value).Length(), "Length"));
			//var vtype = VectorGeneric.Instance.GetType(new List<TypeId> { GenericId }) as VectorType;
			//_Methods.Add("ToVector", new BoundedMethod(this, vtype, (args) => new LsnValue(new VectorInstance(vtype, ((LsnList)args[0].Value).GetValues())), "ToVector"));
			Id.Load(this);
		}

		public override LsnValue CreateDefaultValue()
			=> new LsnValue(new LsnList(this));
	}

	/// <summary>
	/// ...
	/// </summary>
	[Serializable]
	public class LsnListGeneric : GenericType
	{
		public override string Name => "List";

		internal static readonly LsnListGeneric Instance = new LsnListGeneric();

		private LsnListGeneric() { }

		protected override LsnType CreateType(List<TypeId> types)
		{
			if (types.Count != 1) throw new ArgumentException("List types must have exactly one generic parameter.");
			return new LsnListType(types[0]);
		}
	}
}
