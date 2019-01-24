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
		private string LabelPrefix = "";

		private readonly List<PreStatement> PreStatements = new List<PreStatement>();

		private readonly Stack<string> InnerMostLoopContinueLabels = new Stack<string>();

		private readonly Stack<string> InnerMostLoopEndLabels = new Stack<string>();

		private string NextLabel;

		public Statement[] Flatten(List<Component> components)
		{
			Walk(components);

			if (NextLabel != null)
				PreStatements.Add(new PreStatement(new ReturnStatement(null)) { Label = NextLabel });

			foreach (var jmp in PreStatements.Where(s => s.Target != null))
				(jmp.Statement as IHasTargetStatement).Target = FindLabel(jmp.Target);

			return PreStatements.Select(p => p.Statement).ToArray();
		}

		public void ConvPartialFlatten(List<Component> components, string prefix, string startLabel)
		{
			if (startLabel != null)
				NextLabel = startLabel;
			LabelPrefix = prefix;
			Walk(components);

			// add a Jump to Target statement, w/ next label, if any.
		}

		public void AddJumpToTargetStatement()
		{
			PreStatements.Add(new PreStatement(null) {Label = PopNextLabel() });
		}

		public void AddOptionalJumpToTargetStatement()
		{
			if (!(PreStatements[PreStatements.Count - 1].Statement is JumpToTargetStatement))
			{

			}
		}

		public void AddSetTargetStatement(string target)
		{
			if (LabelAliases.ContainsKey(target))
				target = LabelAliases[target];
			PreStatements.Add(new PreStatement(null) { Target = target, Label = PopNextLabel() });
		}

		private readonly Dictionary<string, string> LabelAliases = new Dictionary<string, string>();

		/// <summary>
		/// For when the node has no start block, [Node Start] should redirect to [Node]
		/// </summary>
		/// <param name="alias"></param>
		/// <param name="label"></param>
		public void AddLabelAlias(string alias, string label)
		{
			LabelAliases.Add(alias, label);
		}

		// Use at start of node
		public void AddLabel(string label)
		{
			NextLabel = label;
		}

		public Statement[] FinishFlatten()
		{
			LabelPrefix = "";
			foreach (var pre in PreStatements)
			{
				if (pre.Label != null && LabelAliases.ContainsKey(pre.Label))
					pre.Label = LabelAliases[pre.Label];
				if (pre.Target != null && LabelAliases.ContainsKey(pre.Target))
					pre.Target = LabelAliases[pre.Target];
			}
			foreach (var jmp in PreStatements.Where(s => s.Target != null))
				(jmp.Statement as IHasTargetStatement).Target = FindLabel(jmp.Target);

			return PreStatements.Select(p => p.Statement).ToArray();
		}

		private string PopNextLabel()
		{
			var tmp = NextLabel;
			NextLabel = null;
			return tmp;
		}

		private int FindLabel(string label)
		{
			for(int i = 0; i < PreStatements.Count; i++)
				if (PreStatements[i].Label == label)
					return i - 1; // Because the interpreter's main loop does 'NextStatement++'...
			return PreStatements.Count;
		}

		private int IfCount;
		protected override void WalkIfElse(IfElseControl f)
		{
			var endifLabel = LabelPrefix + "EndIf" + (IfCount++);
			var preSt = new PreStatement(new ConditionalJumpStatement(new NotExpression(f.Condition)))
			{
				Target = endifLabel,
				Label = PopNextLabel()
			};
			PreStatements.Add(preSt);

			Walk(f.Body);
			NextLabel = endifLabel;
			for (int i = 0; i < f.Elsifs.Count; i++)
				WalkElsif(f.Elsifs[i]);

			if (f.ElseBlock != null) Walk(f.ElseBlock);
		}

		protected override void WalkElsif(ElsifControl e)
		{
			var endifLabel = LabelPrefix + "EndIf" + (IfCount++);
			var preSt = new PreStatement(new ConditionalJumpStatement(new NotExpression(e.Condition)))
			{
				Target = endifLabel,
				Label = PopNextLabel()
			};
			PreStatements.Add(preSt);

			Walk(e.Body);
			NextLabel = endifLabel;
		}

		private int WhileLoopCount;
		protected override void WalkWhileLoop(WhileLoop wl)
		{
			var index = WhileLoopCount++;
			var cndLabel = LabelPrefix + "While" + index;
			var endLabel = LabelPrefix + "EndWhile" + index;

			var preSt = new PreStatement(new ConditionalJumpStatement(new NotExpression(wl.Condition)))
			{
				Target = endLabel,
				Label = PopNextLabel()
			};
			PreStatements.Add(preSt);

			InnerMostLoopContinueLabels.Push(cndLabel);
			InnerMostLoopEndLabels.Push(endLabel);

			Walk(wl.Body);
			var loopPreSt = new PreStatement(new JumpStatement())
			{
				Target = cndLabel,
				Label = PopNextLabel()
			};
			PreStatements.Add(loopPreSt);

			NextLabel = endLabel;

			InnerMostLoopContinueLabels.Pop();
			InnerMostLoopEndLabels.Pop();
		}

		private int ForLoopCount;
		protected override void WalkForLoop(ForLoop f)
		{
			var index = ForLoopCount++;
			var cndLabel = LabelPrefix + "For" + index;
			var endLabel = LabelPrefix + "EndFor" + index;

			var assignPreSt = new PreStatement(new AssignmentStatement(f.Index, f.VarValue))
			{ Label = PopNextLabel() };
			PreStatements.Add(assignPreSt);

			var cndPreSt = new PreStatement(new ConditionalJumpStatement(new NotExpression(f.Condition)))
			{ Label = cndLabel, Target = endLabel };    // Jump to EndLabel if cnd is false
			PreStatements.Add(cndPreSt);

			InnerMostLoopContinueLabels.Push(cndLabel);
			InnerMostLoopEndLabels.Push(endLabel);

			Walk(f.Body);

			// Increment
			var postPreSt = new PreStatement(f.Post) { Label = PopNextLabel() };
			PreStatements.Add(postPreSt);
			PreStatements.Add(new PreStatement(new JumpStatement()) { Target = cndLabel });

			NextLabel = endLabel;

			InnerMostLoopContinueLabels.Pop();
			InnerMostLoopEndLabels.Pop();
		}

		int ChoiceCount;
		protected override void WalkCbc(ChoicesBlockControl c)
		{
			var index = (ChoiceCount++).ToString();
			var endLabel = LabelPrefix + "ChoiceEnd" + index;

			var choices = new Tuple<IList<Component>, string>[c.Choices.Count];

			for (int i = 0; i < c.Choices.Count; i++)
			{
				var ch = c.Choices[i];
				var chTarget = LabelPrefix + "Choice" + index + "Target" + i;
				var regPreSt = new PreStatement(new RegisterChoiceStatement(ch.Condition ?? LsnBoolValue.GetBoolValue(true), ch.Title))
				{Target = chTarget};
				if (i == 0)
				{
					regPreSt.Label = PopNextLabel();
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
					Target = InnerMostLoopContinueLabels.Peek()
				};
			}
			else if(s is RegisterChoiceStatement reg && reg.Label != null)
			{
				preSt = new PreStatement(reg) { Target = reg.Label };
			}
			else
				preSt = new PreStatement(s);
			/*if (ExitLabelStack.Count > 0)
				preSt.Label = ExitLabelStack.Pop();*/
			if(NextLabel != null)
			{
				preSt.Label = NextLabel;
				NextLabel = null;
			}
			PreStatements.Add(preSt);
		}

		protected override void WalkForInRangeLoop(ForInRangeLoop fr)
		{
			if (fr.Start is LsnValue start && fr.End is LsnValue end && start.IntValue > end.IntValue)
				return;
			var index = ForLoopCount++;
			var startLabel = LabelPrefix + "For" + index;
			var endLabel = LabelPrefix + "EndFor" + index;
			var continueLabel = LabelPrefix + "ContinueFor" + index;
			if (fr.Statement != null)
			{
				var p = new PreStatement(fr.Statement) { Label = PopNextLabel() };
				PreStatements.Add(p);
				// [label?] assign limit ?
			}
			var initPreSt = new PreStatement(new AssignmentStatement(fr.Iterator.Index, fr.Start)) { Label = PopNextLabel() };
			PreStatements.Add(initPreSt);
					// [label?] init var
			if (!(fr.Start is LsnValue start1 && fr.End is LsnValue end1 && start1.IntValue <= end1.IntValue))
			{
				// Don't make a pre check if the range is constant.
				var startCondExpr = new BinaryExpression(fr.Iterator.AccessExpression, fr.End,
					BinaryOperation.GreaterThan, BinaryOperationArgsType.Int_Int);
				var startCond = new PreStatement(new ConditionalJumpStatement(startCondExpr)) { Target = endLabel };
				PreStatements.Add(startCond);
					// if(not in range) jmp [end]
			}

			InnerMostLoopContinueLabels.Push(continueLabel);
			InnerMostLoopEndLabels.Push(endLabel);

			NextLabel = startLabel;
			Walk(fr.Body);
					// [start] body
			//if(NextLabel == null) add NOP.
			var incrStatement = new AssignmentStatement(fr.Iterator.Index,
				new BinaryExpression(fr.Iterator.AccessExpression, new LsnValue(1), BinaryOperation.Sum,
				BinaryOperationArgsType.Int_Int));
			PreStatements.Add(new PreStatement(incrStatement) { Label = continueLabel});
					// [continue] increment
			var condExpr = new BinaryExpression(fr.Iterator.AccessExpression, fr.End,
				BinaryOperation.LessThanOrEqual, BinaryOperationArgsType.Int_Int);
			var jumpBack = new PreStatement(new ConditionalJumpStatement(condExpr)) { Target = startLabel };
			PreStatements.Add(jumpBack);
					// if(in range) jmp [start]
			NextLabel = endLabel;
					// [end] ...
		}

		// [label?] assign limit ?
		// [label?] init
		// if (not in range) jmp [end]
		// [start] body
		// [continue] increment var
		// if(in range) jmp [start]
		// [end]

		protected override void WalkForInCollectionLoop(ForInCollectionLoop fc)
		{
			var index = ForLoopCount++;
			var startLabel = LabelPrefix + "For" + index;
			var endLabel = LabelPrefix + "EndFor" + index;
			var continueLabel = LabelPrefix + "ContinueFor" + index;

			// [label?] assign collection to var ?
			/*if(fc.Statement != null)
			{
				// ...
			}*/

			// [label?] init index
			var initPreSt = new PreStatement(new AssignmentStatement(fc.Index.Index, new LsnValue(0))) { Label = PopNextLabel() };
			PreStatements.Add(initPreSt);
			// if (collection is empty) jmp end
			var length = new MethodCall(fc.Collection.Type.Type.Methods["Length"], new IExpression[] { fc.Collection });
			var stCond = new BinaryExpression(length, new LsnValue(0), BinaryOperation.Equal, BinaryOperationArgsType.Int_Int);
			var preCheck = new PreStatement(new ConditionalJumpStatement(stCond)) { Target = endLabel };

			InnerMostLoopContinueLabels.Push(continueLabel);
			InnerMostLoopEndLabels.Push(endLabel);

			NextLabel = startLabel;
			// [start] Body
			Walk(fc.Body);
			//if(NextLabel == null) add NOP.

			// index++
			var incrStatement = new AssignmentStatement(fc.Index.Index,
				new BinaryExpression(fc.Index.AccessExpression, new LsnValue(1), BinaryOperation.Sum,
				BinaryOperationArgsType.Int_Int));
			PreStatements.Add(new PreStatement(incrStatement) { Label = continueLabel });

			// if (index < collection.length) jmp [start]
			var condExpr = new BinaryExpression(fc.Index.AccessExpression, length,
				BinaryOperation.LessThan, BinaryOperationArgsType.Int_Int);
			var jumpBack = new PreStatement(new ConditionalJumpStatement(condExpr)) { Target = startLabel };
			PreStatements.Add(jumpBack);
			// [end] ...
			NextLabel = endLabel;
		}
	}
}
