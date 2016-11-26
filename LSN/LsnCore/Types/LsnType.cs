using LsnCore.Expressions;
using LsnCore.Types;
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
		public static LsnType int_ { get; private set; }
		public static LsnType double_ { get; private set; }
		public static LsnType string_ { get; private set; }
		public static LsnType Bool_ { get; private set; } = new BoolType("bool","Boolean");
		public static LsnType dynamic_ { get; private set; }
		public static LsnType object_ { get; private set; }
		
		static LsnType()
		{
			BaseTypes = new List<LsnType>();
			int_ = new LsnBoundedType<int>("int",()=> new IntValue(0),"Integer");
			double_ = new LsnBoundedType<double>("double", () => new DoubleValue(0.0));
			string_ = new LsnBoundedType<string>("string", () => new StringValue(""));
			dynamic_ = new LsnBoundedType<Object>("dynamic",()=> null);
			object_ = new LsnBoundedType<object>("object", () => null);

			BaseTypes.Add(int_);
			BaseTypes.Add(double_);
			BaseTypes.Add(string_);
			
			SetUpOperators(); SetUpMethods();
        }

		private static void SetUpOperators()
		{
			// int 'op' int -> int
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Add, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new IntValue(((IntValue)a).Value + ((IntValue)b).Value),int_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Divide, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new IntValue(((IntValue)a).Value / ((IntValue)b).Value), int_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Mod, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new IntValue(((IntValue)a).Value % ((IntValue)b).Value), int_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Multiply, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new IntValue(((IntValue)a).Value * ((IntValue)b).Value), int_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Power, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new IntValue((int)Math.Pow(((IntValue)a).Value,((IntValue)b).Value)), int_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Subtract, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new IntValue(((IntValue)a).Value - ((IntValue)b).Value), int_.Id));

			// int * string -> string
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Multiply, string_.Id),
				new Tuple<BinOp, TypeId>((a, b) =>
				new StringValue((new StringBuilder()).Append(((StringValue)b).Value, 0, ((IntValue)a).Value).ToString()), string_.Id));

			// Comparisons: int 'op' int -> bool
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.GreaterThan, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(((IntValue)a).Value > ((IntValue)b).Value), Bool_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.LessThan, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(((IntValue)a).Value < ((IntValue)b).Value), Bool_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Equals, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(((IntValue)a).Value == ((IntValue)b).Value), Bool_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.GTE, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(((IntValue)a).Value >= ((IntValue)b).Value), Bool_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.LTE, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(((IntValue)a).Value <= ((IntValue)b).Value), Bool_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.NotEquals, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(((IntValue)a).Value != ((IntValue)b).Value), Bool_.Id));

			// int 'op' double -> double
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Add, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new DoubleValue(((IntValue)a).Value + ((DoubleValue)b).Value),double_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Divide, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new DoubleValue(((IntValue)a).Value / ((DoubleValue)b).Value),double_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Mod, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new DoubleValue(((IntValue)a).Value % ((DoubleValue)b).Value),double_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Multiply, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new DoubleValue(((IntValue)a).Value * ((DoubleValue)b).Value),double_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Power, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new DoubleValue(Math.Pow(((IntValue)a).Value,((DoubleValue)b).Value)),double_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Subtract, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new DoubleValue(((IntValue)a).Value - ((DoubleValue)b).Value),double_.Id));

			// Comparisons: int 'op' double -> bool
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.GreaterThan, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(((IntValue)a).Value > ((DoubleValue)b).Value), Bool_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.LessThan, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(((IntValue)a).Value < ((DoubleValue)b).Value), Bool_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Equals, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(((IntValue)a).Value == ((DoubleValue)b).Value), Bool_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.LTE, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(((IntValue)a).Value <= ((DoubleValue)b).Value), Bool_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.GTE, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(((IntValue)a).Value >= ((DoubleValue)b).Value), Bool_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.NotEquals, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(((IntValue)a).Value != ((DoubleValue)b).Value), Bool_.Id));

			// double 'op' double -> double
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Add, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new DoubleValue(((DoubleValue)a).Value - ((DoubleValue)b).Value),double_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Divide, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new DoubleValue(((DoubleValue)a).Value / ((DoubleValue)b).Value),double_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Mod, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new DoubleValue(((DoubleValue)a).Value % ((DoubleValue)b).Value),double_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Multiply, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new DoubleValue(((DoubleValue)a).Value * ((DoubleValue)b).Value),double_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Power, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new DoubleValue(Math.Pow(((DoubleValue)a).Value, ((DoubleValue)b).Value)),double_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Subtract, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new DoubleValue(((DoubleValue)a).Value - ((DoubleValue)b).Value),double_.Id));

			// double 'op' double -> bool
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.LessThan, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => 
				LsnBoolValue.GetBoolValue(((DoubleValue)a).Value < ((DoubleValue)b).Value), Bool_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.GreaterThan, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) =>
				LsnBoolValue.GetBoolValue(((DoubleValue)a).Value > ((DoubleValue)b).Value), Bool_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Equals, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) =>
				LsnBoolValue.GetBoolValue(((DoubleValue)a).Value == ((DoubleValue)b).Value), Bool_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.LTE, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) =>
				LsnBoolValue.GetBoolValue(((DoubleValue)a).Value <= ((DoubleValue)b).Value), Bool_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.GTE, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) =>
				LsnBoolValue.GetBoolValue(((DoubleValue)a).Value >= ((DoubleValue)b).Value), Bool_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.NotEquals, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) =>
				LsnBoolValue.GetBoolValue(((DoubleValue)a).Value != ((DoubleValue)b).Value), Bool_.Id));

			// double 'op' int -> double
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Add, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new DoubleValue(((DoubleValue)a).Value - ((IntValue)b).Value), double_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Subtract, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new DoubleValue(((DoubleValue)a).Value - ((IntValue)b).Value),double_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Divide, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new DoubleValue(((DoubleValue)a).Value / ((IntValue)b).Value),double_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Mod, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new DoubleValue(((DoubleValue)a).Value % ((IntValue)b).Value),double_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Multiply, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new DoubleValue(((DoubleValue)a).Value * ((IntValue)b).Value), double_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Power, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new DoubleValue(Math.Pow(((DoubleValue)a).Value, ((IntValue)b).Value)), double_.Id));

			// Comparisons: double 'op' int -> bool
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.LessThan, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(((DoubleValue)a).Value < ((IntValue)b).Value), Bool_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.GreaterThan, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(((DoubleValue)a).Value > ((IntValue)b).Value), Bool_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Equals, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(((DoubleValue)a).Value == ((IntValue)b).Value), Bool_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.LTE, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(((DoubleValue)a).Value <= ((IntValue)b).Value), Bool_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.GTE, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(((DoubleValue)a).Value >= ((IntValue)b).Value), Bool_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.NotEquals, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(((DoubleValue)a).Value != ((IntValue)b).Value), Bool_.Id));

			// string + string -> string
			string_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Add, string_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new StringValue(((StringValue)a).Value + ((StringValue)a).Value),string_.Id));

			// string * int -> string
			string_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Multiply, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => 
				new StringValue((new StringBuilder()).Append(((StringValue)a).Value,0,((IntValue)b).Value).ToString()), string_.Id) );

			// Comparison: string 'op' string -> bool
			string_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Equals, string_.Id),
				new Tuple<BinOp, TypeId>((a, b) => 
				LsnBoolValue.GetBoolValue(((StringValue)a).Value == ((StringValue)b).Value), Bool_.Id));
			string_._Operators.Add(new Tuple<Operator, TypeId>(Operator.NotEquals, string_.Id),
				new Tuple<BinOp, TypeId>((a, b) =>
				LsnBoolValue.GetBoolValue(((StringValue)a).Value != ((StringValue)b).Value), Bool_.Id));
			// ToDo: Add >, <, >=, and <=.
		}

		private static void SetUpMethods()
		{
			int_._Methods.Add("Abs", new BoundedMethod(int_,int_,(args)=>new IntValue(Math.Abs(((IntValue)args["self"]).Value))));


			double_._Methods.Add("Abs", new BoundedMethod(double_, double_, 
				(args) => new DoubleValue
				(
					Math.Abs
					(
						((DoubleValue)args["self"]).Value
					)
				)
			));

			double_._Methods.Add("Ceil", new BoundedMethod(double_, int_,
				(args) => new IntValue
				(
					(int)Math.Ceiling
					(
						((DoubleValue)args["self"]).Value
					)
				)
			));

			double_._Methods.Add("Floor", new BoundedMethod(double_, int_,
				(args) => new IntValue
				(
					(int)((DoubleValue)args["self"]).Value
				)
			));

			double_._Methods.Add("Round", new BoundedMethod(double_, int_,
				(args) => new IntValue
				(
					(int)Math.Round
					(
						((DoubleValue)args["self"]).Value
					)
				)
			));


			string_._Methods.Add("Length", new BoundedMethod(string_, int_,
				(args) => new IntValue
				(
					((StringValue)args["self"]).Value.Length
				)
			));

			string_._Methods.Add("SubString", new BoundedMethod(string_, string_,
				(args) => new StringValue
				(
					((StringValue)args["self"]).Value.Substring(((IntValue)args["start"]).Value,
						((IntValue)args["length"]).Value)
				)
			,new List<Parameter>() { new Parameter("start",int_,null,0), new Parameter("length", int_,null,1)}));

			string_._Methods.Add("ToLower", new BoundedMethod(string_, string_,
				(args) => new StringValue
				(
					((StringValue)args["self"]).Value.ToLower()
				)
			));
		}

		/// <summary>
		/// Get a list of base types.
		/// </summary>
		/// <returns></returns>
		public static List<LsnType> GetBaseTypes()
		{
			var types = new List<LsnType>();
			types.Add(int_);
			types.Add(double_);
			types.Add(string_);
			types.Add(Bool_);
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

		private TypeId _Id;

		public TypeId Id
		{
			get
			{
				if (_Id == null)
					_Id = new TypeId(this);
				return _Id;
			}
		}

		/// <summary>
		/// Operators...
		/// </summary>
		private readonly Dictionary<Tuple<Operator, TypeId>, Tuple<BinOp, TypeId>> _Operators
			= new Dictionary<Tuple<Operator, TypeId>, Tuple<BinOp, TypeId>>();

		/// <summary>
		/// Operators...
		/// </summary>
		public IReadOnlyDictionary<Tuple<Operator, TypeId>, Tuple<BinOp, TypeId>> Operators
			{ get { return _Operators; } }

		public bool IsName(string name) => Name == name || Aliases.Contains(name);

		public bool Subsumes(LsnType type)
		{
			return this.Equals(type) || SubsumesList.Contains(type);
		}

		public abstract ILsnValue CreateDefaultValue();

	}
}
