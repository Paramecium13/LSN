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

		public static Dictionary<LSN_Type, LSN_BinDel> Adders { get; private set; }
			= new Dictionary<LSN_Type, LSN_BinDel>();
		public static Dictionary<LSN_Type, LSN_BinDel> Multipliers { get; private set; }
			= new Dictionary<LSN_Type, LSN_BinDel>();
		public static Dictionary<LSN_Type, LSN_BinDel> Exps { get; private set; }
			= new Dictionary<LSN_Type, LSN_BinDel>();
		
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

			Adders[int_]	= (x, y) => new LSN_BoundedInstance(int_, (int)		x.Value + (int)		y.Value);
			Adders[double_] = (x, y) => new LSN_BoundedInstance(int_, (double)	x.Value + (double)	y.Value);
			Adders[string_] = (x, y) => new LSN_BoundedInstance(int_, (string)	x.Value + (string)	y.Value);

			Multipliers[int_]	= (x, y) => new LSN_BoundedInstance(int_, (int)		x.Value * (int)		y.Value);
			Multipliers[double_]= (x, y) => new LSN_BoundedInstance(int_, (double)	x.Value * (double)	y.Value);

			Translators[int_]		= i => ((int)	i.Value).ToString();
			Translators[double_]	= i => ((double)i.Value).ToString();
			Translators[string_]	= i => (string)	i.Value;
			Translators[Bool_]		= i => (bool) i.Value ? "true" : "false"; 

		}

		public virtual bool IsBounded { get { return false; } }

		public abstract bool BoolVal { get; }

		public List<string> Aliases = new List<string>();

		public string Name { get; protected set; }

		protected List<LSN_Type> SubsumesList = new List<LSN_Type>();


		public readonly Dictionary<string, Method> Methods = new Dictionary<string, Method>();

		public bool IsName(string name) => Name == name || Aliases.Contains(name);

		public bool Subsumes(LSN_Type type)
		{
			return this.Equals(type) || SubsumesList.Contains(type);
		}

		public abstract ILSN_Value CreateDefaultValue();

		public abstract int GetSize();

	}
}
