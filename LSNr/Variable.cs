﻿using LsnCore;
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
		private IExpression _AccessExpression;

		public IExpression AccessExpression
			=> _AccessExpression;

		public IExpression InitialValue { get; private set; }

		private List<IExpression> _SubsequentValues = new List<IExpression>();

		public IReadOnlyList<IExpression> SubsequentValues => _SubsequentValues;

		private readonly List<AssignmentStatement> _Reassignments = new List<AssignmentStatement>();

		public IReadOnlyList<AssignmentStatement> Reassignments => _Reassignments;

		public bool Reassigned => _Reassignments.Count != 0;

		// The int item is the length of _SubsequentValues -1 at the time the user was added.
		private List<IExpressionContainer> _Users = new List<IExpressionContainer>();

		public IReadOnlyList<IExpressionContainer> Users => _Users;

		public AssignmentStatement Assignment { get; set; }

		private bool _IsUsed;
		public bool Used { get { return /*Users.Count > 0*/ _IsUsed; } }

		public Variable(string name, bool m, IExpression init)
		{
			throw new NotImplementedException();
		}

		public Variable(string name, bool m, IExpression init, int index)
		{
			Name = name;
			Type = init.Type.Type;
			Mutable = m;
			InitialValue = init;
			Index = index;
			var e = init.Fold();
			if (e.IsReifyTimeConst() && !m)
			{
				_AccessExpression = e;
				Index = -1; // This is a constant.
			}
			else
				_AccessExpression = new VariableExpression(Index, Type.Id);
		}

		/// <summary>
		/// Create a variable from a parameter.
		/// </summary>
		/// <param name="param"></param>
		public Variable(Parameter param)
		{
			Name = param.Name;
			Type = param.Type.Type;
			Mutable = false;
			Index = param.Index;
			_AccessExpression = new VariableExpression(Index, param.Type);
		}

		/// <summary>
		/// Is it constant?
		/// </summary>
		/// <returns></returns>
		public bool Const()
		{
			return (!Mutable && (InitialValue?.IsReifyTimeConst() ?? false));
		}

		public void AddUser(IExpressionContainer user) // Include an indication of its position...
		{
			_Users.Add(user);
		}

		public void AddReasignment(AssignmentStatement reassign)
		{
			_Reassignments.Add(reassign);
			_SubsequentValues.Add(reassign.Value);
		}


		/// <summary>
		/// Replace all usages of this variable with a new expression.
		/// </summary>
		/// <param name="newExpr"></param>
		public void Replace(IExpression newExpr)
		{
			foreach (var user in _Users)
				user.Replace(_AccessExpression, newExpr);
			_AccessExpression = newExpr;
		}

		/// <summary>
		/// Change the index of this variable.
		/// </summary>
		/// <param name="newIndex"></param>
		public void ChangeIndex(int newIndex)
		{
			if (newIndex == Index) return;
			var v = _AccessExpression as VariableExpression;
			if (v != null) v.Index = newIndex;
		}

		public void MarkAsUsed()
		{
			_IsUsed = true;
		}
	}
}
