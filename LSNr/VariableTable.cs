using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	public class VariableTable
	{
		private readonly VariableTable Parent;
		private readonly int Offset;

		private readonly List<Variable> Variables = new List<Variable>();

		private readonly IList<Variable> MasterVariableList;

		/// <summary>
		/// The number of variables;
		/// </summary>
		public int Count => Variables.Count;

		private int ConstCount;

		/// <summary>
		/// The offset for the next variable list
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
		}


		public bool HasVariable(string name)
			=> Variables.Any(v => v.Name == name) || (Parent?.HasVariable(name) ?? false);


		public Variable GetVariable(string name)
			=> Variables.FirstOrDefault(v => v.Name == name) ?? Parent?.GetVariable(name);


		public IExpression GetAccessExpression(string name, IExpressionContainer container)
		{
			var v = GetVariable(name);
			v.AddUser(container);
			return v.GetAccessExpression();
		}


	}
}
