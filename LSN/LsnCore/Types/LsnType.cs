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
	public delegate LsnValue BinOp(LsnValue left, LsnValue right);

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
			int_ = new LsnBoundedType<int>("int",()=> new LsnValue(0),"Integer");
			double_ = new LsnBoundedType<double>("double", () => new LsnValue(0.0));
			string_ = new LsnBoundedType<string>("string", () => new LsnValue(new StringValue("")));
			dynamic_ = new LsnBoundedType<Object>("dynamic",()=> LsnValue.Nil);
			object_ = new LsnBoundedType<object>("object", () => LsnValue.Nil);

			BaseTypes.Add(int_);
			BaseTypes.Add(double_);
			BaseTypes.Add(string_);
			
			SetUpOperators(); SetUpMethods();
        }

		private static void SetUpOperators()
		{
			// int 'op' int -> int
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Add, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnValue.IntSum(a,b),int_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Divide, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnValue.IntQuotient(a,b), int_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Mod, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnValue.IntMod(a,b), int_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Multiply, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnValue.IntProduct(a,b), int_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Power, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnValue.IntPow(a,b), int_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Subtract, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnValue.IntDiff(a,b), int_.Id));

			// int * string -> string
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Multiply, string_.Id),
				new Tuple<BinOp, TypeId>((a, b) =>
					new LsnValue(
						new StringValue(
							(new StringBuilder())
							.Append(((StringValue)b.Value).Value, 0, a.IntValue)
							.ToString()
						)
					), string_.Id));

			// Comparisons: int 'op' int -> bool
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.GreaterThan, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(a.IntValue > b.IntValue), Bool_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.LessThan, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(a.IntValue < b.IntValue), Bool_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Equals, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(a.IntValue == b.IntValue), Bool_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.GTE, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(a.IntValue >= b.IntValue), Bool_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.LTE, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(a.IntValue <= b.IntValue), Bool_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.NotEquals, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(a.IntValue != b.IntValue), Bool_.Id));

			// int 'op' double -> double
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Add, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new LsnValue(a.IntValue + b.DoubleValue),double_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Divide, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new LsnValue(a.IntValue / b.DoubleValue), double_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Mod, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new LsnValue(a.IntValue % b.DoubleValue), double_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Multiply, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new LsnValue(a.IntValue * b.DoubleValue), double_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Power, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new LsnValue(Math.Pow(a.IntValue,b.DoubleValue)), double_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Subtract, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new LsnValue(a.IntValue - b.DoubleValue), double_.Id));

			// Comparisons: int 'op' double -> bool
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.GreaterThan, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(a.IntValue > b.DoubleValue), Bool_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.LessThan, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(a.IntValue < b.DoubleValue), Bool_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Equals, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(a.IntValue == b.DoubleValue), Bool_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.LTE, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(a.IntValue <= b.DoubleValue), Bool_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.GTE, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(a.IntValue >= b.DoubleValue), Bool_.Id));
			int_._Operators.Add(new Tuple<Operator, TypeId>(Operator.NotEquals, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue(a.IntValue != b.DoubleValue), Bool_.Id));

			// double 'op' double -> double
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Add, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnValue.DoubleSum(a,b),double_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Divide, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnValue.DoubleQuotient(a,b),double_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Mod, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnValue.DoubleMod(a,b),double_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Multiply, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnValue.DoubleProduct(a,b),double_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Power, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnValue.DoublePow(a,b),double_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Subtract, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnValue.DoubleDiff(a,b),double_.Id));

			// double 'op' double -> bool
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.LessThan, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) => 
				LsnBoolValue.GetBoolValue(a.DoubleValue < b.DoubleValue), Bool_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.GreaterThan, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) =>
				LsnBoolValue.GetBoolValue(a.DoubleValue > b.DoubleValue), Bool_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Equals, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) =>
				LsnBoolValue.GetBoolValue((a).DoubleValue == b.DoubleValue), Bool_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.LTE, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) =>
				LsnBoolValue.GetBoolValue((a).DoubleValue <= (b).DoubleValue), Bool_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.GTE, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) =>
				LsnBoolValue.GetBoolValue((a).DoubleValue >= (b).DoubleValue), Bool_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.NotEquals, double_.Id),
				new Tuple<BinOp, TypeId>((a, b) =>
				LsnBoolValue.GetBoolValue((a).DoubleValue != (b).DoubleValue), Bool_.Id));

			// double 'op' int -> double
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Add, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new LsnValue(a.DoubleValue + b.IntValue), double_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Subtract, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new LsnValue(a.DoubleValue - b.IntValue), double_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Divide, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new LsnValue(a.DoubleValue / b.IntValue), double_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Mod, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new LsnValue(a.DoubleValue % b.IntValue), double_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Multiply, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new LsnValue(a.DoubleValue * b.IntValue), double_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Power, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new LsnValue(Math.Pow(a.DoubleValue,b.IntValue)), double_.Id));

			// Comparisons: double 'op' int -> bool
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.LessThan, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue((a).DoubleValue < (b).IntValue), Bool_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.GreaterThan, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue((a).DoubleValue > (b).IntValue), Bool_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Equals, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue((a).DoubleValue == (b).IntValue), Bool_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.LTE, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue((a).DoubleValue <= (b).IntValue), Bool_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.GTE, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue((a).DoubleValue >= (b).IntValue), Bool_.Id));
			double_._Operators.Add(new Tuple<Operator, TypeId>(Operator.NotEquals, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => LsnBoolValue.GetBoolValue((a).DoubleValue != (b).IntValue), Bool_.Id));

			// string + string -> string
			string_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Add, string_.Id),
				new Tuple<BinOp, TypeId>((a, b) => new LsnValue(
					new StringValue(((StringValue)a.Value).Value + ((StringValue)a.Value).Value)
					)
				,string_.Id));

			// string * int -> string
			string_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Multiply, int_.Id),
				new Tuple<BinOp, TypeId>((a, b) => 
					new LsnValue(
						new StringValue((new StringBuilder()).Append(((StringValue)a.Value).Value,0,b.IntValue).ToString())
					)
					, string_.Id));

			// Comparison: string 'op' string -> bool
			string_._Operators.Add(new Tuple<Operator, TypeId>(Operator.Equals, string_.Id),
				new Tuple<BinOp, TypeId>((a, b) => 
				LsnBoolValue.GetBoolValue(((StringValue)a.Value).Value == ((StringValue)b.Value).Value), Bool_.Id));
			string_._Operators.Add(new Tuple<Operator, TypeId>(Operator.NotEquals, string_.Id),
				new Tuple<BinOp, TypeId>((a, b) =>
				LsnBoolValue.GetBoolValue(((StringValue)a.Value).Value != ((StringValue)b.Value).Value), Bool_.Id));
			// ToDo: Add >, <, >=, and <=.
		}

		private static void SetUpMethods()
		{
			int_._Methods.Add("Abs", new BoundedMethod(int_,int_,(args)=>new LsnValue((int)Math.Abs(args[0].IntValue))));


			double_._Methods.Add("Abs", new BoundedMethod(double_, double_, 
				(args) => new LsnValue
				(
					Math.Abs
					(
						args[0].DoubleValue
					)
				)
			));

			double_._Methods.Add("Ceil", new BoundedMethod(double_, int_,
				(args) => new LsnValue
				(
					(int)Math.Ceiling
					(
						args[0].DoubleValue
					)
				)
			));

			double_._Methods.Add("Floor", new BoundedMethod(double_, int_,
				(args) => new LsnValue
				(
					(int)args[0].DoubleValue
				)
			));

			double_._Methods.Add("Round", new BoundedMethod(double_, int_,
				(args) => new LsnValue
				(
					(int)Math.Round
					(
						args[0].DoubleValue
					)
				)
			));


			string_._Methods.Add("Length", new BoundedMethod(string_, int_,
				(args) => new LsnValue
				(
					((StringValue)args[0].Value).Value.Length
				)
			));

			string_._Methods.Add("SubString", new BoundedMethod(string_, string_,
				(args) => new LsnValue (
					new StringValue (
						((StringValue)args[0].Value).Value.Substring(args[1].IntValue,
							args[2].IntValue)
						)
				),
				new List<Parameter>() { new Parameter("start",int_,LsnValue.Nil,1), new Parameter("length", int_,LsnValue.Nil,2)})
			);

			string_._Methods.Add("ToLower", new BoundedMethod(string_, string_,
				(args) => new LsnValue(new StringValue
				(
					((StringValue)args[0].Value).Value.ToLower()
				))
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

		public abstract LsnValue CreateDefaultValue();

	}
}
