using LsnCore;
using LsnCore.Expressions;
using LsnCore.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	public class Variable
	{
		public readonly bool Mutable;
		public readonly string Name;
		public readonly LsnType Type;
		public readonly int Index;
		private IExpression AccessExpression;

		public IExpression InitialValue { get; private set; }


		private List<IExpression> _SubsequentValues = new List<IExpression>();

		public IReadOnlyList<IExpression> SubsequentValues => _SubsequentValues;


		private List<IExpression> _Users = new List<IExpression>();

		public IReadOnlyList<IExpression> Users => _Users;

		private List<IExpressionContainer> _UsersB = new List<IExpressionContainer>();

		public IReadOnlyList<IExpressionContainer> UsersB => _UsersB;

		public AssignmentStatement Assignment { get; set; }

		public bool Used { get { return Users.Count > 0; } }

		public bool UsedB { get { return UsersB.Count > 0; } }

		public Variable(string name, bool m, IExpression init)
		{
			Name = name;
			Type = init.Type;
			Mutable = m;
			InitialValue = init;
			var e = init.Fold();
			if (e.IsReifyTimeConst())
				AccessExpression = e;
			else
				AccessExpression = new VariableExpressionB(Index, Type);
		}

		public Variable(string name, bool m, IExpression init, int index)
		{
			Name = name;
			Type = init.Type;
			Mutable = m;
			InitialValue = init;
			Index = index;
			var e = init.Fold();
			if (e.IsReifyTimeConst())
			{
				AccessExpression = e;
				Index = -1; // This is a constant.
			}
			else
				AccessExpression = new VariableExpressionB(Index, Type);
		}

		public Variable(Parameter param)
		{
			Name = param.Name;
			Type = param.Type;
			Mutable = false;
			AccessExpression = new VariableExpressionB(Index, Type);
			Index = param.Index;
		}

		public bool Const()
		{
			return (!Mutable && (InitialValue?.IsReifyTimeConst() ?? false));
		}


		public void AddUser(IExpressionContainer user) // Include an indication of its position...
		{
			_UsersB.Add(user);
		}

		public void AddUser(IExpression expr)
		{
			_Users.Add(expr);
		}


		public IExpression GetAccessExpression()
			=> AccessExpression;


		public void Replace(IExpression newExpr)
		{
			foreach (var user in _UsersB)
				user.Replace(AccessExpression, newExpr);
			AccessExpression = newExpr;
		}

		public void ChangeIndex(int newIndex)
		{
			if (newIndex == Index) return;
			var v = AccessExpression as VariableExpressionB;
			if (v != null) v.Index = newIndex;
		}

	}
}
