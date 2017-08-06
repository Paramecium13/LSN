using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.ControlStructures;
using LsnCore.Statements;
using LsnCore;

namespace LSNr.Optimization
{
	sealed class ComponentFlattener : ComponentWalker
	{

		private readonly List<PreStatement> PreStatements = new List<PreStatement>();

		private readonly Stack<string> ExitingLabels = new Stack<string>();

		private readonly Stack<string> InnerMostLoopStartLabels = new Stack<string>();

		private readonly Stack<string> InnerMostLoopEndLabels = new Stack<string>();


		public Statement[] Flatten(List<Component> components)
		{
			Walk(components);

			foreach (var jmp in PreStatements.Where(s => s.Target != null))
				(jmp.Statement as IHasTargetStatement).Target = FindLabel(jmp.Target);
			

			return PreStatements.Select(p => p.Statement).ToArray();
		}

		private int FindLabel(string label)
		{
			for(int i = 0; i < PreStatements.Count; i++)
				if (PreStatements[i].Label == label)
					return i;
			throw new ApplicationException("");
		}

		private int IfCount;
		protected override void WalkIfElse(IfElseControl f)
		{
			string endifLabel = "EndIf" + (IfCount++).ToString();
			var preSt = new PreStatement(new ConditionalJumpStatement(f.Condition))
			{
				Target = endifLabel
			};

			if (ExitingLabels.Count > 0)
				preSt.Label = ExitingLabels.Pop();
			PreStatements.Add(preSt);

			Walk(f.Body);
			ExitingLabels.Push(endifLabel);
			for (int i = 0; i < f.Elsifs.Count; i++)
			{
				WalkElsif(f.Elsifs[i]);
			}
			Walk(f.ElseBlock);

		}

		protected override void WalkElsif(ElsifControl e)
		{
			string endifLabel = "EndIf" + (IfCount++).ToString();
			var preSt = new PreStatement(new ConditionalJumpStatement(e.Condition))
			{
				Target = endifLabel
			};

			if (ExitingLabels.Count > 0)
				preSt.Label = ExitingLabels.Pop();
			PreStatements.Add(preSt);

			Walk(e.Body);
			ExitingLabels.Push(endifLabel);

		}

















		protected override void View(Statement s)
		{
			PreStatement preSt;
			var br = s as BreakStatement;
			var nxt = s as NextStatement;
			if(br != null)
			{
				preSt = new PreStatement(new JumpStatement())
				{
					Target = InnerMostLoopEndLabels.Peek()
				};
			}
			else if(nxt != null)
			{
				preSt = new PreStatement(new JumpStatement())
				{
					Target = InnerMostLoopStartLabels.Peek()
				};
			}
			else
			{
				preSt = new PreStatement(s);
			}
			if (ExitingLabels.Count > 0)
				preSt.Label = ExitingLabels.Pop();
			PreStatements.Add(preSt);
		}

	}
}
