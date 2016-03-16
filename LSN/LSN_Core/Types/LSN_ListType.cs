using LSN_Core.Types;
using LSN_Core.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSN_Core
{
	/// <summary>
	/// The type of a generic LSN_List.
	/// </summary>
	[Serializable]
	public class LSN_ListType : LSN_ReferenceType, ICollectionType
	{

		static LSN_ListType()
		{
			// Set up methods
			var listInt = LSN_ListGeneric.Instance.GetType(new List<LSN_Type>() { int_ }) as LSN_ListType;
			var listDouble = LSN_ListGeneric.Instance.GetType(new List<LSN_Type>() { double_ }) as LSN_ListType;

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

		private LSN_Type _Generic;
		public LSN_Type GenericType { get { return _Generic; } private set { _Generic = value; } }

		public LSN_Type IndexType => int_;

		public LSN_Type ContentsType => GenericType;

		internal LSN_ListType(LSN_Type type)
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

		public override ILSN_Value CreateDefaultValue()
			=> new LSN_List(this);

	}

	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class LSN_ListGeneric : GenericType
	{
		public override string Name => "List";

		internal static readonly LSN_ListGeneric Instance = new LSN_ListGeneric();

		private LSN_ListGeneric() { }

		protected override LSN_Type CreateType(List<LSN_Type> types)
		{
			if (types.Count != 1) throw new ArgumentException("List types must have exactly one generic parameter.");
			return new LSN_ListType(types[0]);
		}
	}
}
