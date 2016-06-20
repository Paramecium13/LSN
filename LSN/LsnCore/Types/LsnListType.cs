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
	[Serializable]
	public class LsnListType : LsnReferenceType, ICollectionType
	{

		static LsnListType()
		{
			// Set up methods
			var listInt = LsnListGeneric.Instance.GetType(new List<LsnType>() { int_ }) as LsnListType;
			var listDouble = LsnListGeneric.Instance.GetType(new List<LsnType>() { double_ }) as LsnListType;

			listInt._Methods.Add("Sum", new BoundedMethod(listInt, int_,
				(args) =>
				{
					int Σ = 0;
					var list = (LSN_List)args["self"];
					int length = list.Length().Value;
					for (int i = 0; i < length; i++)
						Σ += ((IntValue)list[i]).Value;
					return new IntValue(Σ);
				}
			));
			listInt._Methods.Add("Mean", new BoundedMethod(listInt, int_,
				(args) =>
				{
					int Σ = 0;
					var list = (LSN_List)args["self"];
					int length = list.Length().Value;
					for (int i = 0; i < length; i++)
						Σ += ((IntValue)list[i]).Value;
					return new IntValue(length > 0 ? Σ / length : 0);
				}
			));

			listDouble._Methods.Add("Sum", new BoundedMethod(listDouble, double_,
				(args) =>
				{
					double Σ = 0;
					var list = (LSN_List)args["self"];
					int length = list.Length().Value;
					for (int i = 0; i < length; i++)
						Σ += ((DoubleValue)list[i]).Value;
					return new DoubleValue(Σ);
				}
			));
			listDouble._Methods.Add("Mean", new BoundedMethod(listDouble, double_,
				(args) =>
				{
					double Σ = 0.0;
					var list = (LSN_List)args["self"];
					int length = list.Length().Value;
					for (int i = 0; i < length; i++)
						Σ += ((DoubleValue)list[i]).Value;
					return new DoubleValue(length > 0 ? Σ / length : 0);
				}
			));
		}

		private LsnType _Generic;
		public LsnType GenericType { get { return _Generic; } private set { _Generic = value; } }

		public LsnType IndexType => int_;

		public LsnType ContentsType => GenericType;

		internal LsnListType(LsnType type)
		{
			GenericType = type;
			// Set up methods
			_Methods.Add("Add", new BoundedMethod(this, null,
				(args) =>
				{
					((LSN_List)args["self"]).Add(args["value"]);
					return null;
				},
				new List<Parameter>() { new Parameter("self",this,null,0), new Parameter("value",type,null,1)}
			));
			_Methods.Add("Length", new BoundedMethod(this, int_, (args) => ((LSN_List)args["self"]).Length()));
			List<int> x = new List<int>();
			
		}

		public override ILsnValue CreateDefaultValue()
			=> new LSN_List(this);

	}

	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class LsnListGeneric : GenericType
	{
		public override string Name => "List";

		internal static readonly LsnListGeneric Instance = new LsnListGeneric();

		private LsnListGeneric() { }

		protected override LsnType CreateType(List<LsnType> types)
		{
			if (types.Count != 1) throw new ArgumentException("List types must have exactly one generic parameter.");
			return new LsnListType(types[0]);
		}
	}
}
