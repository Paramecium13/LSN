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


		private readonly List<ReassignmentStatement> _Reassignments = new List<ReassignmentStatement>();


		public IReadOnlyList<ReassignmentStatement> Reassignments => _Reassignments;


		public bool Reassigned => _Reassignments.Count != 0;

		
		private List<IExpressionContainer> _Users = new List<IExpressionContainer>();


		public IReadOnlyList<IExpressionContainer> Users => _Users;


		public AssignmentStatement Assignment { get; set; }


		public bool Used { get { return Users.Count > 0; } }


		public Variable(string name, bool m, IExpression init)
		{
			Name = name;
			Type = init.Type.Type;
			Mutable = m;
			InitialValue = init;
			var e = init.Fold();
			if (e.IsReifyTimeConst())
				AccessExpression = e;
			else
				AccessExpression = new VariableExpression(Index, Type.Id);
		}


		public Variable(string name, bool m, IExpression init, int index)
		{
			Name = name;
			Type = init.Type.Type;
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
				AccessExpression = new VariableExpression(Index, Type.Id);
		}


		public Variable(Parameter param)
		{
			Name = param.Name;
			Type = param.Type.Type;
			Mutable = false;
			AccessExpression = new VariableExpression(Index, param.Type);
			Index = param.Index;
		}


		public bool Const()
		{
			return (!Mutable && (InitialValue?.IsReifyTimeConst() ?? false));
		}


		public void AddUser(IExpressionContainer user) // Include an indication of its position...
		{
			_Users.Add(user);
		}


		public void AddReasignment(IExpression val)
		{
			_SubsequentValues.Add(val);
		}


		public IExpression GetAccessExpression()
			=> AccessExpression;


		public void Replace(IExpression newExpr)
		{
			foreach (var user in _Users)
				user.Replace(AccessExpression, newExpr);
			AccessExpression = newExpr;
		}


		public void ChangeIndex(int newIndex)
		{
			if (newIndex == Index) return;
			var v = AccessExpression as VariableExpression;
			if (v != null) v.Index = newIndex;
		}

	}
}
