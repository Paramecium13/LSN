using LsnCore.Types;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LsnCore
{
	/// <summary>
	/// The type of a generic LSN_List.
	/// </summary>
	[Serializable]
	public class LsnListType : LsnReferenceType, ICollectionType
	{

		
		private LsnType _Generic;
		public LsnType ContentsType { get { return _Generic; } set { _Generic = value; } }

		private LsnType _int;
		public LsnType IndexType => _int;

		internal LsnListType(LsnType type, _int)
		{
			ContentsType = type;
			// Set up methods
			_Methods.Add("Add", new BoundedMethod(this, null,
				(args) =>
				{
					((LSN_List)args["self"]).Add(args["value"]);
					return null;
				},
				new List<Parameter>() { new Parameter("self",this,null,0), new Parameter("value",type,null,1)}
			));
			_Methods.Add("Length", new BoundedMethod(this, _int, (args) => ((LSN_List)args["self"]).Length()));
			List<int> x = new List<int>();
			
		}

		public override ILsnValue CreateDefaultValue()
			=> new LSN_List(this);

	}

	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class LsnListGeneric : GenericType
	{
		public override string Name => "List";

		internal static readonly LsnListGeneric Instance = new LsnListGeneric();

		private LsnListGeneric() { }

		protected override LsnType CreateType(List<LsnType> types)
		{
			if (types.Count != 1) throw new ArgumentException("List types must have exactly one generic parameter.");
			return new LsnListType(types[0]);
		}
	}
}
