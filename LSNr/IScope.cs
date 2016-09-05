using LsnCore;
using LsnCore.Expressions;
using LsnCore.Statements;
using System.Collections.Generic;

namespace LSNr
{
	public interface IScope
	{
		Variable CreateVariable(string name, bool mutable, IExpression init, AssignmentStatement assign);
		Variable CreateVariable(Parameter param);
		Variable GetVariable(string name);
		bool HasVariable(string name);
		bool VariableExists(string name);
		IScope Pop(List<Component> components);
		IScope CreateChild();
	}
}