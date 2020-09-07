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
	/// <summary>
	/// A class that represents a variable in LSN. Keeps track of its Name, Type, Mutability, and the expression used to access it.
	/// </summary>
	public class Variable
	{
		/// <summary>
		/// Is this variable mutable?
		/// </summary>
		public readonly bool Mutable;

		/// <summary>
		/// The name of the variable.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// The Type of the variable.
		/// </summary>
		/// <see cref="LsnType"/>
		public readonly LsnType Type;

		/// <summary>
		/// The index of the variable within the procedure's stack
		/// </summary>
		private int _index;

		/// <summary>
		/// The index of the variable within the procedure's stack
		/// </summary>
		public int Index
		{
			get => _index;
			set
			{
				if (_index == value) return;
				_index = value;
				if (AccessExpression is VariableExpression v)
					v.Index = value;
			}
		}

		/// <summary>
		/// The <see cref="IExpression"/> used to access this variable's value.
		/// </summary>
		public IExpression AccessExpression { get; private set; }

		/// <summary>
		/// The <see cref="IExpression"/> whose result is initially stored in this variable.
		/// Does not always have a value (e.g. variables from <see cref="Parameter"/>s).
		/// </summary>
		public IExpression InitialValue { get; private set; }

		/// <summary>
		/// <see cref="AssignmentStatement"/>s that store a different value in this variable.
		/// </summary>
		private readonly List<AssignmentStatement> _Reassignments = new List<AssignmentStatement>();

		/// <summary>
		/// <see cref="AssignmentStatement"/>s that store a different value in this variable.
		/// </summary>
		public IReadOnlyList<AssignmentStatement> Reassignments => _Reassignments;

		/// <summary>
		/// Gets a value indicating whether this <see cref="Variable"/> is reassigned.
		/// </summary>
		public bool Reassigned => _Reassignments.Count != 0;

		/// <summary>
		/// The places that use this variable.
		/// </summary>
		private readonly List<IExpressionContainer> _Users = new List<IExpressionContainer>();

		/// <summary>
		/// The places that use this variable.
		/// </summary>
		public IReadOnlyList<IExpressionContainer> Users => _Users;

		/// <summary>
		/// The <see cref="AssignmentStatement"/> where this variable is initially assigned.
		/// </summary>
		private AssignmentStatement _assignment;

		/// <summary>
		/// The <see cref="AssignmentStatement"/> where this variable is initially assigned.
		/// </summary>
		public AssignmentStatement Assignment
		{
			get => _assignment;
			set { _assignment = value; Used = true; }
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="Variable"/> is used.
		/// </summary>
		public bool Used { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Variable"/> class.
		/// This constructor is currently used for variables assigned in 'if-let' structures.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="access">The access.</param>
		/// <param name="type">The type.</param>
		public Variable(string name, IExpression access, LsnType type)
		{
			Name = name;
			Type = type;
			Mutable = false;
			_index = -1;
			AccessExpression = access;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Variable"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="m">if set to <c>true</c> [m].</param>
		/// <param name="init">The initialize.</param>
		/// <param name="index">The index.</param>
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

		/// <summary>
		/// Initializes a new instance of the <see cref="Variable"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="type">The type.</param>
		/// <param name="index">The index.</param>
		/// <param name="mutable">if set to <c>true</c> [mutable].</param>
		public Variable(string name, LsnType type, int index, bool mutable = false)
		{
			Name = name;
			Type = type;
			Mutable = mutable;
			_index = index; AccessExpression = new VariableExpression(Index, type.Id, this);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Variable"/> class for the iterated variable in a 'for-in' loop.
		/// </summary>
		/// <remarks>
		/// I guess this is a pseudo-variable in that it is just a shortcut for 'collection[indexVariable]'.
		/// </remarks>
		/// <param name="name">The name.</param>
		/// <param name="indexVariable">The index variable.</param>
		/// <param name="collection">The collection expression.</param>
		public Variable(string name, Variable indexVariable, IExpression collection)
		{
			Name = name;
			Type = ((ICollectionType) collection.Type.Type).ContentsType;
			Mutable = false;
			_index = -1;
			AccessExpression =
				new CollectionValueAccessExpression(collection, indexVariable.AccessExpression, Type.Id);
		}
		/// <summary>
		/// Is this variable a constant?
		/// </summary>
		public bool Const() => (!Mutable && (InitialValue?.IsReifyTimeConst() ?? false));

		/// <summary>
		/// Adds a user of this variable.
		/// </summary>
		/// <param name="user">The user.</param>
		public void AddUser(IExpressionContainer user) // Include an indication of its position...
		{
			_Users.Add(user);
			Used = true;
		}

		/// <summary>
		/// Adds a statement that reassigns this variable.
		/// </summary>
		/// <param name="reassign">The reassign.</param>
		public void AddReasignment(AssignmentStatement reassign)
		{
			_Reassignments.Add(reassign);
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
		/// Marks this variable as being used.
		/// </summary>
		public void MarkAsUsed()
		{
			Used = true;
		}
	}
}
