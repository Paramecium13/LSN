using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.ControlStructures;
using LsnCore.Statements;
using LsnCore;
using LsnCore.Expressions;

namespace LSNr.Optimization
{
	sealed class ComponentFlattener : ComponentWalker
	{

		private readonly List<PreStatement> PreStatements = new List<PreStatement>();

		private readonly Stack<string> NextLabelStack = new Stack<string>();

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
			return PreStatements.Count;
		}

		private int IfCount;
		protected override void WalkIfElse(IfElseControl f)
		{
			string endifLabel = "EndIf" + (IfCount++).ToString();
			var preSt = new PreStatement(new ConditionalJumpStatement(new NotExpression(f.Condition)))
			{
				Target = endifLabel
			};

			if (NextLabelStack.Count > 0)
				preSt.Label = NextLabelStack.Pop();
			PreStatements.Add(preSt);

			Walk(f.Body);
			NextLabelStack.Push(endifLabel);
			for (int i = 0; i < f.Elsifs.Count; i++)
			{
				WalkElsif(f.Elsifs[i]);
			}
			Walk(f.ElseBlock);

		}

		protected override void WalkElsif(ElsifControl e)
		{
			string endifLabel = "EndIf" + (IfCount++).ToString();
			var preSt = new PreStatement(new ConditionalJumpStatement(new NotExpression(e.Condition)))
			{
				Target = endifLabel
			};

			if (NextLabelStack.Count > 0)
				preSt.Label = NextLabelStack.Pop();
			PreStatements.Add(preSt);

			Walk(e.Body);
			NextLabelStack.Push(endifLabel);

		}


		private int WhileLoopCount;
		protected override void WalkWhileLoop(WhileLoop wl)
		{
			var index = WhileLoopCount++;
			var cndLabel = "While" + index.ToString();
			var endLabel = "EndWhile" + index.ToString();

			var preSt = new PreStatement(new ConditionalJumpStatement(new NotExpression(wl.Condition)))
			{ Target = endLabel, Label = cndLabel };
			if (NextLabelStack.Count > 0) preSt.Label = NextLabelStack.Pop();
			PreStatements.Add(preSt);

			InnerMostLoopStartLabels.Push(cndLabel);
			InnerMostLoopEndLabels.Push(endLabel);

			Walk(wl.Body);
			var loopPreSt = new PreStatement(new JumpStatement()){Target = cndLabel};
			if (NextLabelStack.Count > 0) loopPreSt.Label = NextLabelStack.Pop();

			PreStatements.Add(loopPreSt);

			NextLabelStack.Push(endLabel);

			InnerMostLoopStartLabels.Pop();
			InnerMostLoopEndLabels.Pop();
		}


		private int ForLoopCount;
		protected override void WalkForLoop(ForLoop f)
		{
			var index = ForLoopCount++;
			var cndLabel = "For" + index.ToString();
			var endLabel = "EndFor" + index.ToString();

			var assignPreSt = new PreStatement(new AssignmentStatement(f.Index, f.VarValue));
			if (NextLabelStack.Count > 0)
				assignPreSt.Label = NextLabelStack.Pop();
			PreStatements.Add(assignPreSt);

			var cndPreSt = new PreStatement(new ConditionalJumpStatement(new NotExpression(f.Condition)))
			{ Label = cndLabel, Target = endLabel };    // Jump to EndLabel if cnd is false
			PreStatements.Add(cndPreSt);

			InnerMostLoopStartLabels.Push(cndLabel);
			InnerMostLoopEndLabels.Push(endLabel);

			Walk(f.Body);

			// Increment
			var postPreSt = new PreStatement(f.Post);
			if (NextLabelStack.Count > 0)
				postPreSt.Label = NextLabelStack.Pop();
			PreStatements.Add(postPreSt);

			PreStatements.Add(new PreStatement(new JumpStatement()) { Target = cndLabel });

			NextLabelStack.Push(endLabel);

			InnerMostLoopStartLabels.Pop();
			InnerMostLoopEndLabels.Pop();
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
			if (NextLabelStack.Count > 0)
				preSt.Label = NextLabelStack.Pop();
			PreStatements.Add(preSt);
		}

	}
}
