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
			int_._Methods.Add("Abs", new BoundedMethod(int_,int_,(args)=>new LsnValue((int)Math.Abs(args[0].IntValue)), "Abs"));

			double_._Methods.Add("Abs", new BoundedMethod(double_, double_,
				(args) => new LsnValue
				(
					Math.Abs
					(
						args[0].DoubleValue
					)
				), "Abs"
			));

			double_._Methods.Add("Ceil", new BoundedMethod(double_, int_,
				(args) => new LsnValue
				(
					(int)Math.Ceiling
					(
						args[0].DoubleValue
					)
				), "Ceil"
			));

			double_._Methods.Add("Floor", new BoundedMethod(double_, int_,
				(args) => new LsnValue
				(
					(int)args[0].DoubleValue
				), "Floor"
			));

			double_._Methods.Add("Round", new BoundedMethod(double_, int_,
				(args) => new LsnValue
				(
					(int)Math.Round
					(
						args[0].DoubleValue
					)
				), "Round"
			));


			string_._Methods.Add("Length", new BoundedMethod(string_, int_,
				(args) => new LsnValue
				(
					((StringValue)args[0].Value).Value.Length
				), "Length"
			));

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
				VectorGeneric.Instance,
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
