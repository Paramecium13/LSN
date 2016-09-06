using LsnCore.Expressions;
using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{

	/// <summary>
	/// A binary operator delegate.
	/// </summary>
	/// <param name="left"></param>
	/// <param name="right"></param>
	/// <returns></returns>
	public delegate ILsnValue BinOp(ILsnValue left, ILsnValue right);

	public enum Operator { Add, Subtract, Multiply, Divide, Mod, Power, LessThan, GreaterThan, Equals, NotEquals, LTE, GTE}

	[Serializable]
	public abstract class LsnType
	{

        public static List<LsnType> BaseTypes;
		/*public static LsnBoundedType<int> int_ { get; private set; }
		public static LsnType double_ { get; private set; }
		public static LsnType string_ { get; private set; }
		public static LsnType Bool_ { get; private set; } = new BoolType("bool","Boolean");
		public static LsnType dynamic_ { get; private set; }
		public static LsnType object_ { get; private set; }*/

		/// <summary>
		/// Get a list of base types.
		/// </summary>
		/// <returns></returns>
		public static List<LsnType> GetBaseTypes()
		{
			BaseTypes = new List<LsnType>();
			var int_ = new LsnBoundedType<int>("int","Integer");
			int_.CreateDefault = () => new IntValue(0,int_);
			var double_ = new LsnBoundedType<double>("double");
			double_.CreateDefault = () => new DoubleValue(0.0, double_);
			var string_ = new LsnBoundedType<string>("string");
			string_.CreateDefault = () => new StringValue("", string_);
			var dynamic_ = new LsnBoundedType<object>("dynamic");
			var object_ = new LsnBoundedType<object>("object");
			var Bool_ = new BoolType("bool","Boolean");

			BaseTypes.Add(int_);
			BaseTypes.Add(double_);
			BaseTypes.Add(string_);
			
			// int 'op' int -> int
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Add, int_),
				new Tuple<BinOp, LsnType>((a, b) => new IntValue(((IntValue)a).Value + ((IntValue)b).Value, int_),int_));
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Divide, int_),
				new Tuple<BinOp, LsnType>((a, b) => new IntValue(((IntValue)a).Value / ((IntValue)b).Value, int_), int_));
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Mod, int_),
				new Tuple<BinOp, LsnType>((a, b) => new IntValue(((IntValue)a).Value % ((IntValue)b).Value, int_), int_));
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Multiply, int_),
				new Tuple<BinOp, LsnType>((a, b) => new IntValue(((IntValue)a).Value * ((IntValue)b).Value, int_), int_));
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Power, int_),
				new Tuple<BinOp, LsnType>((a, b) => new IntValue((int)Math.Pow(((IntValue)a).Value,((IntValue)b).Value), int_), int_));
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Subtract, int_),
				new Tuple<BinOp, LsnType>((a, b) => new IntValue(((IntValue)a).Value - ((IntValue)b).Value, int_), int_));

			// int * string -> string
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Multiply, string_),
				new Tuple<BinOp, LsnType>((a, b) =>
				new StringValue((new StringBuilder()).Append(((StringValue)b).Value, 0, ((IntValue)a).Value).ToString(), string_), string_));

			// Comparisons: int 'op' int -> bool
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.GreaterThan, int_),
				new Tuple<BinOp, LsnType>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value > ((IntValue)b).Value), Bool_));
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.LessThan, int_),
				new Tuple<BinOp, LsnType>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value < ((IntValue)b).Value), Bool_));
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Equals, int_),
				new Tuple<BinOp, LsnType>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value == ((IntValue)b).Value), Bool_));
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.GTE, int_),
				new Tuple<BinOp, LsnType>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value >= ((IntValue)b).Value), Bool_));
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.LTE, int_),
				new Tuple<BinOp, LsnType>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value <= ((IntValue)b).Value), Bool_));
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.NotEquals, int_),
				new Tuple<BinOp, LsnType>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value != ((IntValue)b).Value), Bool_));

			// int 'op' double -> double
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Add, double_),
				new Tuple<BinOp, LsnType>((a, b) => new DoubleValue(((IntValue)a).Value + ((DoubleValue)b).Value, double_),double_));
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Divide, double_),
				new Tuple<BinOp, LsnType>((a, b) => new DoubleValue(((IntValue)a).Value / ((DoubleValue)b).Value, double_),double_));
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Mod, double_),
				new Tuple<BinOp, LsnType>((a, b) => new DoubleValue(((IntValue)a).Value % ((DoubleValue)b).Value, double_),double_));
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Multiply, double_),
				new Tuple<BinOp, LsnType>((a, b) => new DoubleValue(((IntValue)a).Value * ((DoubleValue)b).Value, double_),double_));
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Power, double_),
				new Tuple<BinOp, LsnType>((a, b) => new DoubleValue(Math.Pow(((IntValue)a).Value,((DoubleValue)b).Value), double_),double_));
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Subtract, double_),
				new Tuple<BinOp, LsnType>((a, b) => new DoubleValue(((IntValue)a).Value - ((DoubleValue)b).Value, double_),double_));

			// Comparisons: int 'op' double -> bool
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.GreaterThan, double_),
				new Tuple<BinOp, LsnType>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value > ((DoubleValue)b).Value), Bool_));
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.LessThan, double_),
				new Tuple<BinOp, LsnType>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value < ((DoubleValue)b).Value), Bool_));
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Equals, double_),
				new Tuple<BinOp, LsnType>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value == ((DoubleValue)b).Value), Bool_));
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.LTE, double_),
				new Tuple<BinOp, LsnType>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value <= ((DoubleValue)b).Value), Bool_));
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.GTE, double_),
				new Tuple<BinOp, LsnType>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value >= ((DoubleValue)b).Value), Bool_));
			int_._Operators.Add(new Tuple<Operator, LsnType>(Operator.NotEquals, double_),
				new Tuple<BinOp, LsnType>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value != ((DoubleValue)b).Value), Bool_));

			// double 'op' double -> double
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Add, double_),
				new Tuple<BinOp, LsnType>((a, b) => new DoubleValue(((DoubleValue)a).Value - ((DoubleValue)b).Value, double_),double_));
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Divide, double_),
				new Tuple<BinOp, LsnType>((a, b) => new DoubleValue(((DoubleValue)a).Value / ((DoubleValue)b).Value, double_),double_));
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Mod, double_),
				new Tuple<BinOp, LsnType>((a, b) => new DoubleValue(((DoubleValue)a).Value % ((DoubleValue)b).Value, double_),double_));
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Multiply, double_),
				new Tuple<BinOp, LsnType>((a, b) => new DoubleValue(((DoubleValue)a).Value * ((DoubleValue)b).Value, double_),double_));
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Power, double_),
				new Tuple<BinOp, LsnType>((a, b) => new DoubleValue(Math.Pow(((DoubleValue)a).Value, ((DoubleValue)b).Value), double_),double_));
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Subtract, double_),
				new Tuple<BinOp, LsnType>((a, b) => new DoubleValue(((DoubleValue)a).Value - ((DoubleValue)b).Value, double_),double_));

			// double 'op' double -> bool
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.LessThan, double_),
				new Tuple<BinOp, LsnType>((a, b) => 
				LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value < ((DoubleValue)b).Value), Bool_));
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.GreaterThan, double_),
				new Tuple<BinOp, LsnType>((a, b) =>
				LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value > ((DoubleValue)b).Value), Bool_));
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Equals, double_),
				new Tuple<BinOp, LsnType>((a, b) =>
				LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value == ((DoubleValue)b).Value), Bool_));
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.LTE, double_),
				new Tuple<BinOp, LsnType>((a, b) =>
				LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value <= ((DoubleValue)b).Value), Bool_));
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.GTE, double_),
				new Tuple<BinOp, LsnType>((a, b) =>
				LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value >= ((DoubleValue)b).Value), Bool_));
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.NotEquals, double_),
				new Tuple<BinOp, LsnType>((a, b) =>
				LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value != ((DoubleValue)b).Value), Bool_));

			// double 'op' int -> double
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Add, int_),
				new Tuple<BinOp, LsnType>((a, b) => new DoubleValue(((DoubleValue)a).Value - ((IntValue)b).Value, double_), double_));
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Subtract, int_),
				new Tuple<BinOp, LsnType>((a, b) => new DoubleValue(((DoubleValue)a).Value - ((IntValue)b).Value, double_),double_));
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Divide, int_),
				new Tuple<BinOp, LsnType>((a, b) => new DoubleValue(((DoubleValue)a).Value / ((IntValue)b).Value, double_),double_));
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Mod, int_),
				new Tuple<BinOp, LsnType>((a, b) => new DoubleValue(((DoubleValue)a).Value % ((IntValue)b).Value, double_),double_));
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Multiply, int_),
				new Tuple<BinOp, LsnType>((a, b) => new DoubleValue(((DoubleValue)a).Value * ((IntValue)b).Value, double_), double_));
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Power, int_),
				new Tuple<BinOp, LsnType>((a, b) => new DoubleValue(Math.Pow(((DoubleValue)a).Value, ((IntValue)b).Value), double_), double_));

			// Comparisons: double 'op' int -> bool
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.LessThan, int_),
				new Tuple<BinOp, LsnType>((a, b) => LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value < ((IntValue)b).Value), Bool_));
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.GreaterThan, int_),
				new Tuple<BinOp, LsnType>((a, b) => LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value > ((IntValue)b).Value), Bool_));
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Equals, int_),
				new Tuple<BinOp, LsnType>((a, b) => LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value == ((IntValue)b).Value), Bool_));
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.LTE, int_),
				new Tuple<BinOp, LsnType>((a, b) => LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value <= ((IntValue)b).Value), Bool_));
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.GTE, int_),
				new Tuple<BinOp, LsnType>((a, b) => LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value >= ((IntValue)b).Value), Bool_));
			double_._Operators.Add(new Tuple<Operator, LsnType>(Operator.NotEquals, int_),
				new Tuple<BinOp, LsnType>((a, b) => LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value != ((IntValue)b).Value), Bool_));

			// string + string -> string
			string_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Add, string_),
				new Tuple<BinOp, LsnType>((a, b) => new StringValue(((StringValue)a).Value + ((StringValue)a).Value, string_),string_));

			// string * int -> string
			string_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Multiply, int_),
				new Tuple<BinOp, LsnType>((a, b) => 
				new StringValue((new StringBuilder()).Append(((StringValue)a).Value,0,((IntValue)b).Value).ToString(), string_), string_) );

			// Comparison: string 'op' string -> bool
			string_._Operators.Add(new Tuple<Operator, LsnType>(Operator.Equals, string_),
				new Tuple<BinOp, LsnType>((a, b) => 
				LSN_BoolValue.GetBoolValue(((StringValue)a).Value == ((StringValue)b).Value), Bool_));
			string_._Operators.Add(new Tuple<Operator, LsnType>(Operator.NotEquals, string_),
				new Tuple<BinOp, LsnType>((a, b) =>
				LSN_BoolValue.GetBoolValue(((StringValue)a).Value != ((StringValue)b).Value), Bool_));
			// TODO: Add >, <, >=, and <=.

			int_._Methods.Add("Abs", new BoundedMethod(int_,int_,(args)=>new IntValue(Math.Abs(((IntValue)args["self"]).Value),int_)));


			double_._Methods.Add("Abs", new BoundedMethod(double_, double_, 
				(args) => new DoubleValue
				(
					Math.Abs
					(
						((DoubleValue)args["self"]).Value
					),double_
				)
			));

			double_._Methods.Add("Ceil", new BoundedMethod(double_, int_,
				(args) => new IntValue
				(
					(int)Math.Ceiling
					(
						((DoubleValue)args["self"]).Value
					),int_
				)
			));

			double_._Methods.Add("Floor", new BoundedMethod(double_, int_,
				(args) => new IntValue
				(
					(int)((DoubleValue)args["self"]).Value, int_
				)
			));

			double_._Methods.Add("Round", new BoundedMethod(double_, int_,
				(args) => new IntValue
				(
					(int)Math.Round
					(
						((DoubleValue)args["self"]).Value
					), int_
				)
			));


			string_._Methods.Add("Length", new BoundedMethod(string_, int_,
				(args) => new IntValue
				(
					((StringValue)args["self"]).Value.Length, int_
				)
			));

			string_._Methods.Add("SubString", new BoundedMethod(string_, string_,
				(args) => new StringValue
				(
					((StringValue)args["self"]).Value.Substring(((IntValue)args["start"]).Value,
						((IntValue)args["length"]).Value),string_
				)
			,new List<Parameter>() { new Parameter("start",int_,null,0), new Parameter("length", int_,null,1)}));

			string_._Methods.Add("ToLower", new BoundedMethod(string_, string_,
				(args) => new StringValue
				(
					((StringValue)args["self"]).Value.ToLower(), string_
				)
			));
		
			var types = new List<LsnType>();
			types.Add(int_);
			types.Add(double_);
			types.Add(string_);
			types.Add(Bool_);

			#region Lists
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
					return new IntValue(Σ, int_);
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
					return new IntValue(length > 0 ? Σ / length : 0, int_);
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
					return new DoubleValue(Σ, double_);
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
					return new DoubleValue(length > 0 ? Σ / length : 0, double_);
				}
			));
			#endregion

			return types;
		}


		public static List<GenericType> GetBaseGenerics()
		{
			var generics = new List<GenericType>();
			generics.Add(VectorGeneric.Instance);
			generics.Add(LsnListGeneric.Instance);
			return generics;
        }


		public virtual bool IsBounded { get { return false; } }

		public List<string> Aliases = new List<string>();

		public string Name { get; protected set; }

		protected readonly List<LsnType> SubsumesList = new List<LsnType>();

		protected readonly Dictionary<string, Method> _Methods = new Dictionary<string, Method>();
		public virtual IReadOnlyDictionary<string, Method> Methods => _Methods;


		/// <summary>
		/// Operators...
		/// </summary>
		private readonly Dictionary<Tuple<Operator, LsnType>, Tuple<BinOp, LsnType>> _Operators
			= new Dictionary<Tuple<Operator, LsnType>, Tuple<BinOp,LsnType>>();

		/// <summary>
		/// Operators...
		/// </summary>
		public virtual IReadOnlyDictionary<Tuple<Operator, LsnType>, Tuple<BinOp, LsnType>> Operators
			{ get { return _Operators; } }

		public bool IsName(string name) => Name == name || Aliases.Contains(name);

		public bool Subsumes(LsnType type)
		{
			return this.Equals(type) || SubsumesList.Contains(type);
		}

		public abstract ILsnValue CreateDefaultValue();

	}
}
