using LsnCore.Statements;
using LsnCore.Types;
using LSNr.Optimization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	public class CodeGen
	{
		private readonly IPreScript Script;
		private readonly TypeId ReturnType;
		private readonly string ProcedureTitle;

		private IScope Scope => Script.CurrentScope;

		public int StackSize { get; private set; }

		public Statement[] Code { get; private set; }

		public CodeGen(IPreScript script, TypeId retType, string procedureTitle)
		{
			Script = script;
			ReturnType = retType;
			ProcedureTitle = procedureTitle;
		}

		private void DoGenerate(IReadOnlyList<Token> tokens)
		{
			var parser = new Parser(tokens, Script);
			parser.Parse();
			Script.CurrentScope.Pop(parser.Components);
			if (!Script.Valid) return;
			var cmps = Parser.Consolidate(parser.Components).Where(c => c != null).ToList();
			StackSize = (Script.CurrentScope as VariableTable)?.MaxSize ?? -1;
			Code = new ComponentFlattener().Flatten(cmps);
		}

		private void CheckReturn()
		{
			if (!Script.Valid) return;
			if (ReturnType != null)
			{
				var hasRet = false;
				foreach (var st in Code.OfType<ReturnStatement>())
				{
					if (st.Value == null || !ReturnType.Subsumes(st.Value.Type.Type))
					{
						Script.Valid = false;
						throw new ApplicationException($"Not all paths of {ProcedureTitle} return a value of type '{ReturnType.Name}'.");
					}
					hasRet = true;
				}
				if (!hasRet)
					throw new ApplicationException($"{ProcedureTitle} does not return a value; it should return a value of type '{ReturnType.Name}'.");
				return;
			}
			foreach (var st in Code.OfType<ReturnStatement>())
			{
				if (st.Value == null) continue;
				Script.Valid = false;
				throw new ApplicationException($"{ProcedureTitle} should not return a value. It returns a value of type {st.Value.Type.Name}.");
			}
		}

		public void Generate(IReadOnlyList<Token> tokens)
		{
			DoGenerate(tokens);
			CheckReturn();
		}
	}
}
