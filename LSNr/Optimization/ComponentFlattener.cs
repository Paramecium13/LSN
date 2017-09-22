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
		
		private readonly Stack<string> InnerMostLoopStartLabels = new Stack<string>();

		private readonly Stack<string> InnerMostLoopEndLabels = new Stack<string>();

		private string NextLabel;


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

			if (NextLabel != null)
			{
				preSt.Label = NextLabel;
				NextLabel = null;
			}
			PreStatements.Add(preSt);

			Walk(f.Body);
			NextLabel = endifLabel;
			for (int i = 0; i < f.Elsifs.Count; i++)
			{
				WalkElsif(f.Elsifs[i]);
			}
			if(f.ElseBlock != null) Walk(f.ElseBlock);

		}

		protected override void WalkElsif(ElsifControl e)
		{
			string endifLabel = "EndIf" + (IfCount++).ToString();
			var preSt = new PreStatement(new ConditionalJumpStatement(new NotExpression(e.Condition)))
			{
				Target = endifLabel
			};

			if (NextLabel != null)
			{
				preSt.Label = NextLabel;
				NextLabel = null;
			}
			PreStatements.Add(preSt);

			Walk(e.Body);
			NextLabel = endifLabel;

		}


		private int WhileLoopCount;
		protected override void WalkWhileLoop(WhileLoop wl)
		{
			var index = WhileLoopCount++;
			var cndLabel = "While" + index.ToString();
			var endLabel = "EndWhile" + index.ToString();

			var preSt = new PreStatement(new ConditionalJumpStatement(new NotExpression(wl.Condition)))
			{ Target = endLabel, Label = cndLabel };
			if (NextLabel != null)
			{
				preSt.Label = NextLabel;
				NextLabel = null;
			}
			PreStatements.Add(preSt);

			InnerMostLoopStartLabels.Push(cndLabel);
			InnerMostLoopEndLabels.Push(endLabel);

			Walk(wl.Body);
			var loopPreSt = new PreStatement(new JumpStatement()){Target = cndLabel};
			if (NextLabel != null)
			{
				loopPreSt.Label = NextLabel;
				NextLabel = null;
			}
			PreStatements.Add(loopPreSt);

			NextLabel = endLabel;

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
			if (NextLabel != null)
			{
				assignPreSt.Label = NextLabel;
				NextLabel = null;
			}
			PreStatements.Add(assignPreSt);

			var cndPreSt = new PreStatement(new ConditionalJumpStatement(new NotExpression(f.Condition)))
			{ Label = cndLabel, Target = endLabel };    // Jump to EndLabel if cnd is false
			PreStatements.Add(cndPreSt);

			InnerMostLoopStartLabels.Push(cndLabel);
			InnerMostLoopEndLabels.Push(endLabel);

			Walk(f.Body);

			// Increment
			var postPreSt = new PreStatement(f.Post);
			if (NextLabel != null)
			{
				postPreSt.Label = NextLabel;
				NextLabel = null;
			}
			PreStatements.Add(postPreSt);

			PreStatements.Add(new PreStatement(new JumpStatement()) { Target = cndLabel });

			NextLabel = endLabel;

			InnerMostLoopStartLabels.Pop();
			InnerMostLoopEndLabels.Pop();
		}

		int ChoiceCount;
		protected override void WalkCbc(ChoicesBlockControl c)
		{
			var index = (ChoiceCount++).ToString();
			string endLabel = "ChoiceEnd" + index;

			var choices = new Tuple<IList<Component>, string>[c.Choices.Count];

			for (int i = 0; i < c.Choices.Count; i++)
			{
				var ch = c.Choices[i];
				string chTarget = "Choice" + index + "Target" + i.ToString();
				var regPreSt = new PreStatement(new RegisterChoiceStatement(ch.Condition ?? LsnBoolValue.GetBoolValue(true), ch.Title))
				{Target = chTarget};
				if (i == 0 && NextLabel != null)
				{
					regPreSt.Label = NextLabel; NextLabel = null;
				}

				PreStatements.Add(regPreSt);

				choices[i] = new Tuple<IList<Component>, string>(ch.Components, chTarget);
			}

			PreStatements.Add(new PreStatement(new DisplayChoicesStatement()));

			foreach (var ch in choices)
			{
				NextLabel = ch.Item2;
				Walk(ch.Item1);
				var jEndPreSt = new PreStatement(new JumpStatement()) { Target = endLabel };
				if (NextLabel != null)
				{
					jEndPreSt.Label = NextLabel;
					NextLabel = null;
				}
				PreStatements.Add(jEndPreSt);
			}

			NextLabel = endLabel;
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
			/*if (ExitLabelStack.Count > 0)
				preSt.Label = ExitLabelStack.Pop();*/
			if(NextLabel != null)
			{
				preSt.Label = NextLabel;
				NextLabel = null;
			}
			PreStatements.Add(preSt);
		}

	}
}
