using LsnCore.Expressions;
using LsnCore.Types;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	public abstract class LsnType
	{
		public static LsnType Int_ => I32Type.Instance;
		public static LsnType Double_ => F64Type.Instance;
		public static LsnType String_ => StringType.Instance;
		public static LsnType Bool_ => BoolType.Instance;

		static LsnType()
		{
			SetUpMethods();
		}

		private static void SetUpMethods()
		{
			Int_._Methods.Add("Abs", new BoundedMethod(Int_,Int_,(args)=>new LsnValue((int)Math.Abs(args[0].IntValue)), "Abs"));

			Double_._Methods.Add("Abs", new BoundedMethod(Double_, Double_,
				(args) => new LsnValue
				(
					Math.Abs
					(
						args[0].DoubleValue
					)
				), "Abs"
			));

			Double_._Methods.Add("Ceil", new BoundedMethod(Double_, Int_,
				(args) => new LsnValue
				(
					(int)Math.Ceiling
					(
						args[0].DoubleValue
					)
				), "Ceil"
			));

			Double_._Methods.Add("Floor", new BoundedMethod(Double_, Int_,
				(args) => new LsnValue
				(
					(int)args[0].DoubleValue
				), "Floor"
			));

			Double_._Methods.Add("Round", new BoundedMethod(Double_, Int_,
				(args) => new LsnValue
				(
					(int)Math.Round
					(
						args[0].DoubleValue
					)
				), "Round"
			));


			String_._Methods.Add("Length", new BoundedMethod(String_, Int_,
				(args) => new LsnValue
				(
					((StringValue)args[0].Value).Value.Length
				), "Length"
			));

			String_._Methods.Add("SubString", new BoundedMethod(String_, String_,
				(args) => new LsnValue (
					new StringValue (
						((StringValue)args[0].Value).Value.Substring(args[1].IntValue,
							args[2].IntValue)
						)
				), "SubString",
				new List<Parameter> { new Parameter("start",Int_,LsnValue.Nil,1), new Parameter("length", Int_,LsnValue.Nil,2)})
			);

			String_._Methods.Add("ToLower", new BoundedMethod(String_, String_,
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
				Int_,
				Double_,
				String_,
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
			if (this == Double_ && type == Int_) return true;
			return Equals(type) || SubsumesList.Contains(type);
		}

		public abstract LsnValue CreateDefaultValue();

		internal abstract bool LoadAsMember(ILsnDeserializer deserializer, BinaryDataReader reader, Action<LsnValue> setter);

		internal abstract void WriteAsMember(LsnValue value, ILsnSerializer serializer, BinaryDataWriter writer);
	}
}
