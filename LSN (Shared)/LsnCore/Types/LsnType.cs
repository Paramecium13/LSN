using LsnCore.Expressions;
using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	public abstract class LsnType
	{
		public static LsnType int_ { get; } = new LsnBoundedType<int>("int", () => new LsnValue(0));
		public static LsnType double_ { get; } = new LsnBoundedType<double>("double", () => new LsnValue(0.0));
		public static LsnType string_ { get; } = new LsnBoundedType<string>("string", () => new LsnValue(new StringValue("")));
		public static LsnType Bool_ { get; } = new BoolType("bool");
		public static LsnType object_ { get; } = new LsnBoundedType<object>("object", () => LsnValue.Nil);

		static LsnType()
		{
			SetUpMethods();
		}

		private static void SetUpMethods()
		{
			#if LSNR
			int_._Methods.Add("Abs", new InstructionMappedMethod(int_, int_, "Abs", OpCode.Abs));
			double_._Methods.Add("Abs", new InstructionMappedMethod(double_, double_, "Abs", OpCode.Abs));
			double_._Methods.Add("Ceil", new InstructionMappedMethod(double_, int_, "Ceil", OpCode.Ceil));
			double_._Methods.Add("Floor", new InstructionMappedMethod(double_, int_, "Floor", OpCode.Floor));
			double_._Methods.Add("Round", new InstructionMappedMethod(double_, int_, "Round", OpCode.Round));


			string_._Methods.Add("Length", new InstructionMappedMethod(string_, int_, "Length", OpCode.StringLength));
			#endif

			string_._Methods.Add("SubString", new BoundedMethod(string_, string_,
				(args) => new LsnValue (
					new StringValue (
						((StringValue)args[0].Value).Value.Substring(args[1].IntValue,
							args[2].IntValue)
						)
				), "SubString",
				new List<Parameter> { new Parameter("start",int_,LsnValue.Nil,1), new Parameter("length", int_,LsnValue.Nil,2)})
			);

			string_._Methods.Add("ToLower", new BoundedMethod(string_, string_,
				(args) => new LsnValue(new StringValue
				(
					((StringValue)args[0].Value).Value.ToLower()
				)), "ToLower"
			));
		}

		/// <summary>
		/// Get a list of base types.
		/// </summary>
		/// <returns></returns>
		public static List<LsnType> GetBaseTypes()
		{
			return new List<LsnType>
			{
				int_,
				double_,
				string_,
				Bool_,
				RangeType.Instance,
				NullType.Instance
			};
		}

		public static List<GenericType> GetBaseGenerics() => new List<GenericType>
			{
				ArrayGeneric.Instance,
				LsnListGeneric.Instance,
				OptionGeneric.Instance
			};

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
			protected set
			{
				_Id = value;
				_Id.Load(this);
			}
		}

		public bool IsName(string name) => Name == name || Aliases.Contains(name);

		public virtual bool Subsumes(LsnType type)
		{
			if (this == double_ && type == int_) return true;
			return Equals(type) || SubsumesList.Contains(type);
		}

		public abstract LsnValue CreateDefaultValue();
	}
}
