using LsnCore.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	public class LabelInfo
	{
		private readonly Stack<string> InnerMostLoopStartLabels = new Stack<string>();

		private readonly Stack<string> InnerMostLoopEndLabels = new Stack<string>();

		int IfCount;
		int WhileCount;
		int ForCount;
		int ChoiceCount;

		public string GetIfLabel()
			=> "EndIf" + IfCount++;

		public void GetWhileLabels(out string start, out string end)
		{
			start = "While" + WhileCount;
			end = "EndWhile" + WhileCount++;
			InnerMostLoopStartLabels.Push(start);
			InnerMostLoopEndLabels.Push(end);
		}

		public void GetForLabels(out string start, out string end)
		{
			start = "For" + ForCount;
			end = "EndFor" + ForCount;
			InnerMostLoopStartLabels.Push(start);
			InnerMostLoopEndLabels.Push(end);
		}

		public void ExitLoop()
		{
			InnerMostLoopEndLabels.Pop();
			InnerMostLoopStartLabels.Pop();
		}

		public void EnterCustomLoop(string start, string end)
		{
			InnerMostLoopStartLabels.Push(start);
			InnerMostLoopEndLabels.Push(end);
		}

		public string GetContinueTarget()
			=> InnerMostLoopStartLabels.Peek();

		public string GetBreakTarget()
			=> InnerMostLoopEndLabels.Peek();

		public string GetChoiceLabel()
			=> "Choice" + ChoiceCount;

		int ChoiceTargetCount;
		public string GetChoiceTargetLable()
			=> "Choice" + ChoiceCount + "Target" + ChoiceTargetCount++;

		public void ExitChoiceControl()
		{
			ChoiceTargetCount = 0;
			ChoiceCount++;
		}

		string NextLabel;
	}

	public sealed class PreStatement
	{
		public readonly Statement Statement;
		public string Label;
		public string Target;

		public PreStatement(Statement statement)
		{
			Statement = statement;
		}
	}
}
