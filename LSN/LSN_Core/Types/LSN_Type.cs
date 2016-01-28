using LSN_Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core
{
	public delegate LSN_BoundedInstance LSN_BinDel(LSN_BoundedInstance a, LSN_BoundedInstance b);
	public delegate string TypeTranlator(LSN_BoundedInstance i);


	public enum Operator { Add, Subtract, Multiply, Divide, Mod, Power}

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
		
		public static Dictionary<LSN_Type, TypeTranlator> Translators { get; private set; }
			= new Dictionary<LSN_Type, TypeTranlator>();

		static LSN_Type()
		{
			BaseTypes = new List<LSN_Type>();
			int_ = new LSN_BoundedType<int>("int",sizeof(int),"Integer");
			double_ = new LSN_BoundedType<double>("double",sizeof(double));
			string_ = new LSN_BoundedType<string>("string", IntPtr.Size);
			dynamic_ = new LSN_BoundedType<Object>("dynamic", IntPtr.Size);
			object_ = new LSN_BoundedType<object>("object", IntPtr.Size);

			BaseTypes.Add(int_);
			BaseTypes.Add(double_);
			BaseTypes.Add(string_);

			Translators[int_]		= i => ((int)	i.Value).ToString();
			Translators[double_]	= i => ((double)i.Value).ToString();
			Translators[string_]	= i => (string)	i.Value;
			Translators[Bool_]		= i => (bool) i.Value ? "true" : "false";
			SetUpOperators();
		}

		private static void SetUpOperators()
		{
			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Add, int_),
				(a, b) => new IntValue(((IntValue)a).Value + ((IntValue)b).Value));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Divide, int_),
				(a, b) => new IntValue(((IntValue)a).Value / ((IntValue)b).Value));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Mod, int_),
				(a, b) => new IntValue(((IntValue)a).Value % ((IntValue)b).Value));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Multiply, int_),
				(a, b) => new IntValue(((IntValue)a).Value * ((IntValue)b).Value));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Power, int_),
				(a, b) => new IntValue((int)Math.Pow(((IntValue)a).Value,((IntValue)b).Value)));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Subtract, int_),
				(a, b) => new IntValue(((IntValue)a).Value - ((IntValue)b).Value));


			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Add, double_),
				(a, b) => new DoubleValue(((IntValue)a).Value + ((DoubleValue)b).Value));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Divide, double_),
				(a, b) => new DoubleValue(((IntValue)a).Value / ((DoubleValue)b).Value));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Mod, double_),
				(a, b) => new DoubleValue(((IntValue)a).Value % ((DoubleValue)b).Value));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Multiply, double_),
				(a, b) => new DoubleValue(((IntValue)a).Value * ((DoubleValue)b).Value));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Power, double_),
				(a, b) => new DoubleValue(Math.Pow(((IntValue)a).Value,((DoubleValue)b).Value)));

			int_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Subtract, double_),
				(a, b) => new DoubleValue(((IntValue)a).Value - ((DoubleValue)b).Value));


			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Add, double_),
				(a, b) => new DoubleValue(((DoubleValue)a).Value - ((DoubleValue)b).Value));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Divide, double_),
				(a, b) => new DoubleValue(((DoubleValue)a).Value / ((DoubleValue)b).Value));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Mod, double_),
				(a, b) => new DoubleValue(((DoubleValue)a).Value % ((DoubleValue)b).Value));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Multiply, double_),
				(a, b) => new DoubleValue(((DoubleValue)a).Value * ((DoubleValue)b).Value));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Power, double_),
				(a, b) => new DoubleValue(Math.Pow(((DoubleValue)a).Value, ((DoubleValue)b).Value)));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Subtract, double_),
				(a, b) => new DoubleValue(((DoubleValue)a).Value - ((DoubleValue)b).Value));


			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Subtract, int_),
				(a, b) => new DoubleValue(((DoubleValue)a).Value - ((IntValue)b).Value));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Divide, int_),
				(a, b) => new DoubleValue(((DoubleValue)a).Value / ((IntValue)b).Value));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Mod, int_),
				(a, b) => new DoubleValue(((DoubleValue)a).Value % ((IntValue)b).Value));

			double_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Power, int_),
				(a, b) => new DoubleValue(Math.Pow(((DoubleValue)a).Value, ((IntValue)b).Value)));


			string_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Add, string_),
				(a, b) => new StringValue(((StringValue)a).Value + ((StringValue)a).Value));

			string_._Operators.Add(new Tuple<Operator, LSN_Type>(Operator.Multiply, int_),
				(a, b) => new StringValue((new StringBuilder()).Append(((StringValue)a).Value,0,((IntValue)b).Value).ToString()));

		}

		public virtual bool IsBounded { get { return false; } }

		public List<string> Aliases = new List<string>();

		public string Name { get; protected set; }

		protected List<LSN_Type> SubsumesList = new List<LSN_Type>();


		public readonly Dictionary<string, Method> Methods = new Dictionary<string, Method>();

		/// <summary>
		/// Operators...
		/// </summary>
		private readonly Dictionary<Tuple<Operator, LSN_Type>, Func<ILSN_Value, ILSN_Value, ILSN_Value>> _Operators
			= new Dictionary<Tuple<Operator, LSN_Type>, Func<ILSN_Value, ILSN_Value, ILSN_Value>>();

		/// <summary>
		/// Operators...
		/// </summary>
		public IReadOnlyDictionary<Tuple<Operator, LSN_Type>, Func<ILSN_Value, ILSN_Value, ILSN_Value>> Operators
			{ get { return _Operators; } }

		public bool IsName(string name) => Name == name || Aliases.Contains(name);

		public bool Subsumes(LSN_Type type)
		{
			return this.Equals(type) || SubsumesList.Contains(type);
		}

		public abstract ILSN_Value CreateDefaultValue();

		public abstract int GetSize();

	}
}
