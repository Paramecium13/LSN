using LsnCore;
using LsnCore.Expressions;
using LsnCore.Statements;
using LsnCore.Types;
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
		private int _index;
		public int Index
		{
			get { return _index; }
			set
			{
				if(_index != value)
				{
					_index = value;
					if (Assignment != null)
						Assignment.Index = value;
					if (AccessExpression is VariableExpression v)
						v.Index = value;
					foreach (var re in Reassignments)
					{
						re.Index = value;
					}
				}
			}
		}

		public IExpression AccessExpression { get; private set; }

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

		public Variable(string name, IExpression access, LsnType type)
		{
			Name = name;
			Type = type;
			Mutable = false;
			_index = -1;
			AccessExpression = access;
		}

		public Variable(string name, bool m, IExpression init, int index)
		{
			Name = name;
			Type = init.Type.Type;
			Mutable = m;
			InitialValue = init;
			_index = index;
			var e = init.Fold();
			if (e.IsReifyTimeConst() && !m)
			{
				AccessExpression = e;
				_index = -1; // This is a constant.
			}
			else
				AccessExpression = new VariableExpression(Index, Type.Id, this);
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
			_index = param.Index;
			AccessExpression = new VariableExpression(Index, param.Type, this);
		}

		public Variable(string name, LsnType type, int index, bool mutable = false)
		{
			Name = name;
			Type = type;
			Mutable = mutable;
			_index = index; AccessExpression = new VariableExpression(Index, type.Id, this);
		}

		public Variable(string name, Variable indexVariable, IExpression collection)
		{
			Name = name;
			Type = (collection.Type.Type as ICollectionType).ContentsType;
			Mutable = false;
			_index = -1;
			AccessExpression =
				new CollectionValueAccessExpression(collection, indexVariable.AccessExpression, Type.Id);
		}
		/// <summary>
		/// Is it constant?
		/// </summary>
		/// <returns></returns>
		public bool Const() => (!Mutable && (InitialValue?.IsReifyTimeConst() ?? false));

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
				user.Replace(AccessExpression, newExpr);
			AccessExpression = newExpr;
		}

		/// <summary>
		/// Change the index of this variable.
		/// </summary>
		/// <param name="newIndex"></param>
		public void ChangeIndex(int newIndex)
		{
			if (newIndex == Index) return;
			Index = newIndex;
		}

		public void MarkAsUsed()
		{
			_IsUsed = true;
		}
	}
}
