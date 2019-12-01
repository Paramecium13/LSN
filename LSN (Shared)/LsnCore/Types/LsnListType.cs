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
			var listInt = (LsnListType) LsnListGeneric.Instance.GetType(new[] { int_.Id });
			var listDouble = (LsnListType) LsnListGeneric.Instance.GetType(new[] { double_.Id });

			listInt._Methods.Add("Sum", new BoundedMethod(listInt, int_,
				(args) =>
				{
					var Σ = 0;
					var list = (LsnList)args[0].Value;
					var length = list.Length().IntValue;
					for (var i = 0; i < length; i++)
						Σ += (list[i]).IntValue;
					return new LsnValue(Σ);
				}, "Sum"
			));
			listInt._Methods.Add("Mean", new BoundedMethod(listInt, int_,
				(args) =>
				{
					var Σ = 0;
					var list = (LsnList)args[0].Value;
					var length = list.Length().IntValue;
					for (var i = 0; i < length; i++)
						Σ += (list[i]).IntValue;
					return new LsnValue(length > 0 ? Σ / length : 0);
				}, "Mean"
			));

			listDouble._Methods.Add("Sum", new BoundedMethod(listDouble, double_,
				(args) =>
				{
					var Σ = 0.0;
					var list = (LsnList)args[0].Value;
					var length = list.Length().IntValue;
					for (var i = 0; i < length; i++)
						Σ += (list[i]).DoubleValue;
					return new LsnValue(Σ);
				}, "Sum"
			));
			listDouble._Methods.Add("Mean", new BoundedMethod(listDouble, double_,
				(args) =>
				{
					var Σ = 0.0;
					var list = (LsnList)args[0].Value;
					var length = list.Length().IntValue;
					for (var i = 0; i < length; i++)
						Σ += (list[i]).DoubleValue;
					return new LsnValue(length > 0 ? Σ / length : 0);
				}, "Mean"
			));
		}

		private LsnType _Generic;
		public LsnType GenericType {
			get => _Generic ?? (_Generic = GenericId.Type);
			private set => _Generic = value;
		}

		public readonly TypeId GenericId;

		public LsnType IndexType => int_;

		public LsnType ContentsType => GenericType;

		internal LsnListType(TypeId type)
		{
			Id = new TypeId("List`" + type.Name);
			GenericType = type.Type;
			GenericId = type;
		}

		internal void SetUpMethods()
		{
			_Methods.Add("Add", new BoundedMethod(this, null,
				(args) =>
				{
					((LsnList)args[0].Value).Add(args[1]);
					return LsnValue.Nil;
				}, "Add",
				new List<Parameter> { new Parameter("self",this, LsnValue.Nil, 0), new Parameter("value",GenericId, LsnValue.Nil, 1)}
			));
			_Methods.Add("Length", new BoundedMethod(this, int_, (args) => ((LsnList)args[0].Value).Length(), "Length"));
			var vtype = VectorGeneric.Instance.GetType(new TypeId[] { GenericId }) as VectorType;
			_Methods.Add("ToVector", new BoundedMethod(this, vtype, (args) => new LsnValue(new VectorInstance(vtype, ((LsnList)args[0].Value).GetValues())), "ToVector"));
			Id.Load(this);
		}

		public override LsnValue CreateDefaultValue()
			=> new LsnValue(new LsnList(this));
	}

	/// <summary>
	/// ...
	/// </summary>
	public class LsnListGeneric : GenericType
	{
		public override string Name => "List";

		internal static readonly LsnListGeneric Instance = new LsnListGeneric();

		private LsnListGeneric() { }

		protected override LsnType CreateType(TypeId[] types)
		{
			if (types.Length != 1) throw new ArgumentException("List types must have exactly one generic parameter.");
			return new LsnListType(types[0]);
		}

		public override LsnType GetType(TypeId[] types)
		{
			var name = GetGenericName(types);
			if (Types.TryGetValue(name, out var type))
				return type;
			type = CreateType(types);
			if (!Types.ContainsKey(name)) // For some reason this double check is needed to avoid adding duplicate keys.
				Types.Add(name, type);
			((LsnListType) type).SetUpMethods();
			return Types[name];
		}
	}
}
