using LsnCore;
using LsnCore.Expressions;
using LsnCore.Statements;
using System.Collections.Generic;

namespace LSNr
{
	public interface IScope
	{
		Variable CreateVariable(string name, bool mutable, IExpression init);
		Variable CreateVariable(string name, LsnType type);
		Variable CreateVariable(Parameter param);
		Variable CreateIteratorVariable(string name, IExpression collection, Variable index);
		Variable CreateMaskVariable(string name, IExpression accessExpression, LsnType type);
		Variable GetVariable(string name);
		bool HasVariable(string name);
		bool VariableExists(string name);
		IScope Pop(List<Component> components);
		IScope CreateChild();
	}
}