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
	public class VariableTable : IScope
	{
		private readonly VariableTable Parent;

		public int MaxSize
		{
			get
			{
				var childrenMax = Children.Count != 0 ? Children.Select(c => c.MaxSize).Max() : 0;
				return NextOffset > childrenMax ? NextOffset : childrenMax;
			}
		}

		/// <summary>
		/// The index of the first variable in this table.
		/// </summary>
		private int Offset;

		private readonly List<Variable> Variables = new List<Variable>();

		private readonly IList<Variable> MasterVariableList; // ToDo: Remove?

		private readonly IList<VariableTable> Children = new List<VariableTable>();

		/// <summary>
		/// The number of variables in this table, not counting the parent(s);
		/// </summary>
		public int Count => Variables.Count;

		/// <summary>
		/// The number of constants...
		/// </summary>
		private int ConstCount;

		/// <summary>
		/// The offset for the next variable list or the index for the next variable.
		/// </summary>
		public int NextOffset => Offset + Count - ConstCount;

		public VariableTable(IList<Variable> masterVarList)
		{
			Offset = 0; MasterVariableList = masterVarList;
		}

		public VariableTable(VariableTable parent, IList<Variable> masterVarList)
		{
			Parent = parent;
			Offset = Parent.NextOffset;
			MasterVariableList = masterVarList;
			ConstCount = parent.ConstCount;
		}

		/// <inheritdoc/>
		public bool HasVariable(string name)
			=> Variables.Any(v => v.Name == name);

		/// <inheritdoc/>
		public bool VariableExists(string name)
			=> HasVariable(name) || (Parent?.VariableExists(name) ?? false);

		/// <inheritdoc/>
		public Variable GetVariable(string name)
			=> Variables.FirstOrDefault(v => v.Name == name) ?? Parent?.GetVariable(name) ?? throw new KeyNotFoundException($"No variable named '{name}' exists.");

		/// <inheritdoc/>
		public Variable CreateVariable(string name, bool mutable, IExpression init)
		{
			var v = new Variable(name, mutable, init, NextOffset);
			Variables.Add(v);
			return v;
		}

		/// <inheritdoc/>
		public Variable CreateVariable(Parameter param)
		{
			var v = new Variable(param);
			Variables.Add(v);
			if (v.Const())
				ConstCount++;
			return v;
		}

		/// <inheritdoc/>
		public Variable CreateVariable(string name, LsnType type, bool mutable = false)
		{
			var v = new Variable(name, type, NextOffset, mutable);
			Variables.Add(v);
			if (v.Const())
				ConstCount++;
			return v;
		}

		/// <inheritdoc/>
		public Variable CreateMaskVariable(string name, IExpression accessExpression, LsnType type)
		{
			var v = new Variable(name, accessExpression, type);
			Variables.Add(v);
			return v;
		}

		/// <inheritdoc/>
		public Variable CreateIteratorVariable(string name, IExpression collection, Variable index)
		{
			var v = new Variable(name, index, collection);
			Variables.Add(v);
			return v;
		}

		private const int MaxUsersForReplacement = 2;

		/// <inheritdoc/>
		public IScope Pop(List<Component> components)
		{
			// Optimize contained variables...
			var deadVars = new List<Variable>();
			var downShift = 0;
			foreach (var variable in Variables)
			{
				if (!variable.Used && variable.Assignment != null)
				{
					components.Remove(variable.Assignment);
					deadVars.Add(variable);
					downShift++; // Shift index to account for dead variable.
					var i = variable.Index;
					foreach (var child in Children)
						child.ParentVariableRemoved(i);
				}
				else if ((!variable.Mutable || !variable.Reassigned) && variable.Assignment != null && 
					(variable.InitialValue?.IsPure ?? false) && variable.Users.Count <= MaxUsersForReplacement)
				{ // Only do this if the initial assignment is a pure expression.
					components.Remove(variable.Assignment);
					deadVars.Add(variable);
					downShift++;
					var i = variable.Index;
					foreach (var child in Children)
						child.ParentVariableRemoved(i);
					variable.Replace(variable.InitialValue); // Replace uses of this variable with it's initial value.
				}
				else if(downShift != 0)
					variable.Index -= downShift;
			}
			foreach (var v in deadVars)
				Variables.Remove(v);

			deadVars.Clear();
			downShift = 0;

			// Check again for removable variables
			foreach (var variable in Variables)
			{
				if ((!variable.Mutable || !variable.Reassigned) && variable.Assignment != null &&
					(variable.InitialValue?.IsPure ?? false) && variable.Users.Count <= MaxUsersForReplacement)
				{ // Only do this if the initial assignment is a pure expression.
					components.Remove(variable.Assignment);
					deadVars.Add(variable);
					downShift++;
					var i = variable.Index;
					foreach (var child in Children)
						child.ParentVariableRemoved(i);
					variable.Replace(variable.InitialValue); // Replace uses of this variable with it's initial value.
				}
				else if (downShift != 0)
					variable.Index -= downShift;
			}
			foreach (var v in deadVars)
				Variables.Remove(v);

			return Parent;
		}

		/// <summary>
		/// A variable in the parent was removed.
		/// </summary>
		/// <param name="index">The index of the variable that was removed.</param>
		private void ParentVariableRemoved(int index)
		{
			if (index >= Offset) return;
			Offset--;
			foreach (var v in Variables)
				v.Index -= 1;
			foreach (var child in Children)
				child.ParentVariableRemoved(index);
		}

		public IScope CreateChild()
		{
			var child = new VariableTable(this, MasterVariableList);
			Children.Add(child);
			return child;
		}

	}
}
