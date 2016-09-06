using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	/// <summary>
	/// A type defined outside the current file, such as a(n) LSN core type.
	/// </summary>
	[Serializable]
	public class ExternalType : LsnType
	{
		[NonSerialized]
		private LsnType External;

		public override bool IsBounded => External.IsBounded;


		public override IReadOnlyDictionary<string, Method> Methods => External.Methods;


		public override IReadOnlyDictionary<Tuple<Operator, LsnType>, Tuple<BinOp, LsnType>> Operators
			=> External.Operators;

		// These will be found using an expression walker...
		private IList<IExpression> ExprUsers = new List<IExpression>();

		private IList<Function> FnRetUsers = new List<Function>();

		public override ILsnValue CreateDefaultValue()
			=> External.CreateDefaultValue();


		public ExternalType(LsnType external)
		{
			External = external;
			Name = external.Name;
		}


		public void Resolve(LsnType type)
		{
			foreach (var user in ExprUsers)
				user.Type = type;
			ExprUsers.Clear();
			ExprUsers = null;
		}

	}
}
