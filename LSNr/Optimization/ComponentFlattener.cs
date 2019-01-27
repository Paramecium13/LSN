using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.ControlStructures;
using LsnCore.Statements;
using LsnCore;
using LsnCore.Expressions;
using LSNr.Converations;

namespace LSNr.Optimization
{
	public sealed class ComponentFlattener : ComponentWalker
	{
		private string LabelPrefix = "";

		private readonly List<PreStatement> PreStatements = new List<PreStatement>();

		private readonly Stack<string> InnerMostLoopContinueLabels = new Stack<string>();

		private readonly Stack<string> InnerMostLoopEndLabels = new Stack<string>();

		private IList<string> NextLabel { get; } = new List<string>();

		public Statement[] Flatten(IEnumerable<Component> components)
		{
			Walk(components);

			if (NextLabel.Count > 0)
				PreStatements.Add(new PreStatement(new ReturnStatement(null)) { Label = NextLabel });

			foreach (var jmp in PreStatements.Where(s => s.Target != null))
				(jmp.Statement as IHasTargetStatement).Target = FindLabel(jmp.Target);

			return PreStatements.Select(p => p.Statement).ToArray();
		}

		public void ConvPartialFlatten(IEnumerable<Component> components, string prefix, string startLabel)
		{
			if (startLabel != null)
				NextLabel.Add(startLabel);
			LabelPrefix = prefix;
			Walk(components);

			// add a Jump to Target statement, w/ next label, if any.
		}

		public void AddJumpToTargetStatement(Variable convJumpTargetVariable)
		{
			var jump = new JumpToTargetStatement(convJumpTargetVariable.Index);
			PreStatements.Add(new PreStatement(jump) { Label = PopNextLabel() });
		}

		public void AddOptionalJumpToTargetStatement(Variable convJumpTargetVariable)
		{
			if (!(PreStatements[PreStatements.Count - 1].Statement is JumpToTargetStatement
				|| PreStatements[PreStatements.Count - 1].Statement is ReturnStatement))
				AddJumpToTargetStatement(convJumpTargetVariable);
		}

		public void AddSetTargetStatement(string target, Variable jumpTargetVar)
		{
			if (LabelAliases.ContainsKey(target))
				target = LabelAliases[target];
			var set = new SetTargetStatement(jumpTargetVar);
			jumpTargetVar.AddUser(set);
			PreStatements.Add(new PreStatement(set) { Target = target, Label = PopNextLabel() });
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
			NextLabel.Add(label);
		}

		public Statement[] FinishFlatten()
		{
			LabelPrefix = "";
			/*if (NextLabel.Count > 0)
				PreStatements.Add(new PreStatement(new ReturnStatement(null)) { Label = NextLabel });*/
			foreach (var pre in PreStatements)
			{
				if(pre.Label != null)
				{
					var foo = pre.Label.FirstOrDefault(l => LabelAliases.ContainsKey(l));
					if (foo != null)
						pre.Label[pre.Label.IndexOf(foo)] = LabelAliases[foo];
				}
				if (pre.Target != null && LabelAliases.ContainsKey(pre.Target))
					pre.Target = LabelAliases[pre.Target];
			}
			foreach (var jmp in PreStatements.Where(s => s.Target != null))
				(jmp.Statement as IHasTargetStatement).Target = FindLabel(jmp.Target);

			return PreStatements.Select(p => p.Statement).ToArray();
		}

		private IList<string> PopNextLabel()
		{
			var tmp = NextLabel.ToList();
			NextLabel.Clear();
			return tmp;
		}

		private int FindLabel(string label)
		{
			for(int i = 0; i < PreStatements.Count; i++)
				if (PreStatements[i]?.Label?.Contains(label) ?? false)
					return i;
			throw new ApplicationException("Label not found");
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
			AddLabel(endifLabel);
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
			AddLabel(endifLabel);
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

			AddLabel(endLabel);

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
			{ Label = new List<string> { cndLabel }, Target = endLabel };    // Jump to EndLabel if cnd is false
			PreStatements.Add(cndPreSt);

			InnerMostLoopContinueLabels.Push(cndLabel);
			InnerMostLoopEndLabels.Push(endLabel);

			Walk(f.Body);

			// Increment
			var postPreSt = new PreStatement(f.Post) { Label = PopNextLabel() };
			PreStatements.Add(postPreSt);
			PreStatements.Add(new PreStatement(new JumpStatement()) { Target = cndLabel });

			AddLabel(endLabel);

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
				AddLabel(ch.Item2);
				Walk(ch.Item1);
				var jEndPreSt = new PreStatement(new JumpStatement()) { Target = endLabel };
				if (NextLabel != null)
				{
					jEndPreSt.Label = PopNextLabel();
					//NextLabel = null;
				}
				PreStatements.Add(jEndPreSt);
			}
			AddLabel(endLabel);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "nxt")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "br")]
		protected override void View(Statement s)
		{
			PreStatement preSt;
			switch (s)
			{
				case BreakStatement br:
					preSt = new PreStatement(new JumpStatement())
					{ Target = InnerMostLoopEndLabels.Peek() };
					break;
				case NextStatement nxt:
					preSt = new PreStatement(new JumpStatement())
					{ Target = InnerMostLoopContinueLabels.Peek() };
					break;
				case RegisterChoiceStatement reg when reg.Label != null:
					preSt = new PreStatement(reg) { Target = reg.Label };
					break;
				case SetNodeStatement sn:
					var st = new SetTargetStatement(sn.Variable);
					preSt = new PreStatement(st) { Target = sn.Node };
					break;
				default:
					preSt = new PreStatement(s);
					break;
			}

			/*if (ExitLabelStack.Count > 0)
				preSt.Label = ExitLabelStack.Pop();*/
			preSt.Label = PopNextLabel();
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

			AddLabel(startLabel);
			Walk(fr.Body);
					// [start] body
			//if(NextLabel == null) add NOP.
			var incrStatement = new AssignmentStatement(fr.Iterator.Index,
				new BinaryExpression(fr.Iterator.AccessExpression, new LsnValue(1), BinaryOperation.Sum,
				BinaryOperationArgsType.Int_Int));
			PreStatements.Add(new PreStatement(incrStatement) { Label = new List<string> { continueLabel } });
					// [continue] increment
			var condExpr = new BinaryExpression(fr.Iterator.AccessExpression, fr.End,
				BinaryOperation.LessThanOrEqual, BinaryOperationArgsType.Int_Int);
			var jumpBack = new PreStatement(new ConditionalJumpStatement(condExpr)) { Target = startLabel };
			PreStatements.Add(jumpBack);
			// if(in range) jmp [start]
			AddLabel(endLabel);
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

			InnerMostLoopContinueLabels.Push(continueLabel);
			InnerMostLoopEndLabels.Push(endLabel);

			AddLabel(startLabel);
			// [start] Body
			Walk(fc.Body);
			//if(NextLabel == null) add NOP.

			// index++
			var incrStatement = new AssignmentStatement(fc.Index.Index,
				new BinaryExpression(fc.Index.AccessExpression, new LsnValue(1), BinaryOperation.Sum,
				BinaryOperationArgsType.Int_Int));
			PreStatements.Add(new PreStatement(incrStatement) { Label = new List<string> { continueLabel } });

			// if (index < collection.length) jmp [start]
			var condExpr = new BinaryExpression(fc.Index.AccessExpression, length,
				BinaryOperation.LessThan, BinaryOperationArgsType.Int_Int);
			var jumpBack = new PreStatement(new ConditionalJumpStatement(condExpr)) { Target = startLabel };
			PreStatements.Add(jumpBack);
			// [end] ...
			AddLabel(endLabel);
		}
	}
}
