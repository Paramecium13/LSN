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

		/// <summary>
		/// 
		/// </summary>
		private int _MaxSizeFromChildren;


		public int MaxSize
		{
			get
			{
				return NextOffset > _MaxSizeFromChildren ? NextOffset : _MaxSizeFromChildren;
			}
		}

		/// <summary>
		/// The index of the first variable in this table.
		/// </summary>
		private readonly int Offset;

		private readonly List<Variable> Variables = new List<Variable>();

		private readonly IList<Variable> MasterVariableList;

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


		public bool HasVariable(string name)
			=> Variables.Any(v => v.Name == name);


		public bool VariableExists(string name)
			=> HasVariable(name) || (Parent?.VariableExists(name) ?? false);


		public Variable GetVariable(string name)
			=> Variables.FirstOrDefault(v => v.Name == name) ?? Parent?.GetVariable(name);


		public IExpression GetAccessExpression(string name, IExpressionContainer container)
		{
			var v = GetVariable(name);
			v.AddUser(container);
			return v.GetAccessExpression();
		}


		public Variable CreateVariable(string name, bool mutable, IExpression init, AssignmentStatement assign)
		{
			var v = new Variable(name, mutable, init, assign, NextOffset);
			Variables.Add(v);
			return v;
		}


		public Variable CreateVariable(Parameter param)
		{
			var v = new Variable(param);
			Variables.Add(v);
			if (v.Const())
				ConstCount++;
			return v;
		}


		public IScope Pop(List<Component> components)
		{
			// Optimize contained variables...
			foreach (var variable in Variables)
			{
				if (!variable.Used && variable.Assignment != null) components.Remove(variable.Assignment);
			}
			Parent.RecieveChildMaxSize(MaxSize);
			return Parent;
		}


		private void RecieveChildMaxSize(int max)
		{
			if (max > _MaxSizeFromChildren) _MaxSizeFromChildren = max;
		}


		public IScope CreateChild()
			=> new VariableTable(this, MasterVariableList);

	}
}
