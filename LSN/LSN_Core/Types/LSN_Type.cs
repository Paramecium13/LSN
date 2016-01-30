﻿using LSN_Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core
{

	/// <summary>
	/// A binary operator delegate.
	/// </summary>
	/// <param name="left"></param>
	/// <param name="right"></param>
	/// <returns></returns>
	public delegate ILSN_Value BinOp(ILSN_Value left, ILSN_Value right);

	public enum Operator { Add, Subtract, Multiply, Divide, Mod, Power, LessThan, GreaterThan, Equals, NotEquals, LTE, GTE}

	[Serializable]
	public abstract class LSN_Type
	{

        public static List<LSN_Type> BaseTypes;
		public static LSN_Type int_ { get; private set; }
		public static LSN_Type double_ { get; private set; }
		public static LSN_Type string_ { get; private set; }
		public static LSN_Type Bool_ { get; private set; } = new BoolType("bool","Boolean");
		public static LSN_Type dynamic_ { get; private set; }
		public static LSN_Type object_ { get; private set; }
		
		static LSN_Type()
		{
			BaseTypes = new List<LSN_Type>();
			int_ = new LSN_BoundedType<int>("int",()=> new IntValue(0),"Integer");
			double_ = new LSN_BoundedType<double>("double", () => new DoubleValue(0.0));
			string_ = new LSN_BoundedType<string>("string", () => new StringValue(""));
			dynamic_ = new LSN_BoundedType<Object>("dynamic",()=> null);
			object_ = new LSN_BoundedType<object>("object", () => null);

			BaseTypes.Add(int_);
			BaseTypes.Add(double_);
			BaseTypes.Add(string_);
			
			SetUpOperators(); SetUpMethods();
        }

		private static void SetUpOperators()
		{
			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Add, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => new IntValue(((IntValue)a).Value + ((IntValue)b).Value),int_));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Divide, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => new IntValue(((IntValue)a).Value / ((IntValue)b).Value), int_));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Mod, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => new IntValue(((IntValue)a).Value % ((IntValue)b).Value), int_));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Multiply, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => new IntValue(((IntValue)a).Value * ((IntValue)b).Value), int_));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Power, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => new IntValue((int)Math.Pow(((IntValue)a).Value,((IntValue)b).Value)), int_));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Subtract, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => new IntValue(((IntValue)a).Value - ((IntValue)b).Value), int_));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.GreaterThan, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value > ((IntValue)b).Value), Bool_));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.LessThan, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value < ((IntValue)b).Value), Bool_));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Equals, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value == ((IntValue)b).Value), Bool_));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.GTE, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value >= ((IntValue)b).Value), Bool_));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.LTE, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value <= ((IntValue)b).Value), Bool_));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.NotEquals, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value != ((IntValue)b).Value), Bool_));


			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Add, double_),
				new Tuple<BinOp, LSN_Type>((a, b) => new DoubleValue(((IntValue)a).Value + ((DoubleValue)b).Value),double_));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Divide, double_),
				new Tuple<BinOp, LSN_Type>((a, b) => new DoubleValue(((IntValue)a).Value / ((DoubleValue)b).Value),double_));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Mod, double_),
				new Tuple<BinOp, LSN_Type>((a, b) => new DoubleValue(((IntValue)a).Value % ((DoubleValue)b).Value),double_));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Multiply, double_),
				new Tuple<BinOp, LSN_Type>((a, b) => new DoubleValue(((IntValue)a).Value * ((DoubleValue)b).Value),double_));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Power, double_),
				new Tuple<BinOp, LSN_Type>((a, b) => new DoubleValue(Math.Pow(((IntValue)a).Value,((DoubleValue)b).Value)),double_));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Subtract, double_),
				new Tuple<BinOp, LSN_Type>((a, b) => new DoubleValue(((IntValue)a).Value - ((DoubleValue)b).Value),double_));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.GreaterThan, double_),
				new Tuple<BinOp, LSN_Type>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value > ((DoubleValue)b).Value), Bool_));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.LessThan, double_),
				new Tuple<BinOp, LSN_Type>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value < ((DoubleValue)b).Value), Bool_));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Equals, double_),
				new Tuple<BinOp, LSN_Type>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value == ((DoubleValue)b).Value), Bool_));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.LTE, double_),
				new Tuple<BinOp, LSN_Type>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value <= ((DoubleValue)b).Value), Bool_));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.GTE, double_),
				new Tuple<BinOp, LSN_Type>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value >= ((DoubleValue)b).Value), Bool_));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.NotEquals, double_),
				new Tuple<BinOp, LSN_Type>((a, b) => LSN_BoolValue.GetBoolValue(((IntValue)a).Value != ((DoubleValue)b).Value), Bool_));


			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Add, double_),
				new Tuple<BinOp, LSN_Type>((a, b) => new DoubleValue(((DoubleValue)a).Value - ((DoubleValue)b).Value),double_));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Divide, double_),
				new Tuple<BinOp, LSN_Type>((a, b) => new DoubleValue(((DoubleValue)a).Value / ((DoubleValue)b).Value),double_));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Mod, double_),
				new Tuple<BinOp, LSN_Type>((a, b) => new DoubleValue(((DoubleValue)a).Value % ((DoubleValue)b).Value),double_));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Multiply, double_),
				new Tuple<BinOp, LSN_Type>((a, b) => new DoubleValue(((DoubleValue)a).Value * ((DoubleValue)b).Value),double_));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Power, double_),
				new Tuple<BinOp, LSN_Type>((a, b) => new DoubleValue(Math.Pow(((DoubleValue)a).Value, ((DoubleValue)b).Value)),double_));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Subtract, double_),
				new Tuple<BinOp, LSN_Type>((a, b) => new DoubleValue(((DoubleValue)a).Value - ((DoubleValue)b).Value),double_));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.LessThan, double_),
				new Tuple<BinOp, LSN_Type>((a, b) => 
				LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value < ((DoubleValue)b).Value), Bool_));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.GreaterThan, double_),
				new Tuple<BinOp, LSN_Type>((a, b) =>
				LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value > ((DoubleValue)b).Value), Bool_));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Equals, double_),
				new Tuple<BinOp, LSN_Type>((a, b) =>
				LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value == ((DoubleValue)b).Value), Bool_));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.LTE, double_),
				new Tuple<BinOp, LSN_Type>((a, b) =>
				LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value <= ((DoubleValue)b).Value), Bool_));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.GTE, double_),
				new Tuple<BinOp, LSN_Type>((a, b) =>
				LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value >= ((DoubleValue)b).Value), Bool_));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.NotEquals, double_),
				new Tuple<BinOp, LSN_Type>((a, b) =>
				LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value != ((DoubleValue)b).Value), Bool_));


			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Add, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => new DoubleValue(((DoubleValue)a).Value - ((IntValue)b).Value), double_));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Subtract, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => new DoubleValue(((DoubleValue)a).Value - ((IntValue)b).Value),double_));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Divide, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => new DoubleValue(((DoubleValue)a).Value / ((IntValue)b).Value),double_));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Mod, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => new DoubleValue(((DoubleValue)a).Value % ((IntValue)b).Value),double_));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Multiply, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => new DoubleValue(((DoubleValue)a).Value * ((IntValue)b).Value), double_));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Power, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => new DoubleValue(Math.Pow(((DoubleValue)a).Value, ((IntValue)b).Value)), double_));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.LessThan, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value < ((IntValue)b).Value), Bool_));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.GreaterThan, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value > ((IntValue)b).Value), Bool_));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Equals, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value == ((IntValue)b).Value), Bool_));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.LTE, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value <= ((IntValue)b).Value), Bool_));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.GTE, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value >= ((IntValue)b).Value), Bool_));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.NotEquals, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => LSN_BoolValue.GetBoolValue(((DoubleValue)a).Value != ((IntValue)b).Value), Bool_));


			string_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Add, string_),
				new Tuple<BinOp, LSN_Type>((a, b) => new StringValue(((StringValue)a).Value + ((StringValue)a).Value),string_));

			string_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Multiply, int_),
				new Tuple<BinOp, LSN_Type>((a, b) => 
				new StringValue((new StringBuilder()).Append(((StringValue)a).Value,0,((IntValue)b).Value).ToString()), string_) );

			string_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Equals, string_),
				new Tuple<BinOp, LSN_Type>((a, b) => 
				LSN_BoolValue.GetBoolValue(((StringValue)a).Value == ((StringValue)b).Value), Bool_));

			string_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.NotEquals, string_),
				new Tuple<BinOp, LSN_Type>((a, b) =>
				LSN_BoolValue.GetBoolValue(((StringValue)a).Value != ((StringValue)b).Value), Bool_));
		}

		private static void SetUpMethods()
		{
			int_.Methods.Add("Abs", new BoundedMethod(int_,int_,(args)=>new IntValue(Math.Abs(((IntValue)args["self"]).Value))));
			double_.Methods.Add("Abs", new BoundedMethod(double_, double_, 
				(args) => new DoubleValue
				(
					Math.Abs
					(
						((DoubleValue)args["self"]).Value
					)
				)
			));

			double_.Methods.Add("Floor", new BoundedMethod(double_, int_,
				(args) => new IntValue
				(
					(int)((DoubleValue)args["self"]).Value
				)
			));

			double_.Methods.Add("Ceil", new BoundedMethod(double_, int_,
				(args) => new IntValue
				(
					(int)Math.Ceiling
					(
						((DoubleValue)args["self"]).Value
					)
				)
			));

			string_.Methods.Add("ToLower", new BoundedMethod(string_, string_,
				(args) => new StringValue
				(
					((StringValue)args["self"]).Value.ToLower()
				)
			));

			string_.Methods.Add("Length", new BoundedMethod(string_, int_,
				(args) => new IntValue
				(
					((StringValue)args["self"]).Value.Length
				)
			));
		}

		public virtual bool IsBounded { get { return false; } }

		public List<string> Aliases = new List<string>();

		public string Name { get; protected set; }

		protected List<LSN_Type> SubsumesList = new List<LSN_Type>();


		public readonly Dictionary<string, Method> Methods = new Dictionary<string, Method>();

		/// <summary>
		/// Operators...
		/// </summary>
		private readonly Dictionary<Tuple<Operator, LSN_Type>, Tuple<BinOp, LSN_Type>> _Operators
			= new Dictionary<Tuple<Operator, LSN_Type>, Tuple<BinOp,LSN_Type>>();

		/// <summary>
		/// Operators...
		/// </summary>
		public IReadOnlyDictionary<Tuple<Operator, LSN_Type>, Tuple<BinOp, LSN_Type>> Operators
			{ get { return _Operators; } }

		public bool IsName(string name) => Name == name || Aliases.Contains(name);

		public bool Subsumes(LSN_Type type)
		{
			return this.Equals(type) || SubsumesList.Contains(type);
		}

		public abstract ILSN_Value CreateDefaultValue();

	}
}
