﻿using LsnCore;
using LsnCore.ControlStructures;
using LsnCore.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr.Optimization
{
	public abstract class ComponentWalker
	{
		public void Walk(Component c)
		{
			if (c is ControlStructure cs)
				Walk(cs);
			else View((Statement)c);
		}

		protected virtual void Walk(IEnumerable<Component> components)
		{
			foreach (var component in components)
				Walk(component);
		}

		private void Walk(ControlStructure c)
		{
			switch (c)
			{
				case CaseStructure cs:
					WalkCaseStructure(cs);
					return;
				case Choice ch:
					WalkChioce(ch);
					return;
				case ChoicesBlockControl cbc:
					WalkCbc(cbc);
					return;
				case ForLoop fl:
					WalkForLoop(fl);
					return;
				case IfElseControl ife:
					WalkIfElse(ife);
					return;
				case MatchStructure ms:
					WalkMatchStructure(ms);
					return;
				case WhileLoop wl:
					WalkWhileLoop(wl);
					return;
				case ForInRangeLoop fr:
					WalkForInRangeLoop(fr);
					return;
				case ForInCollectionLoop fc:
					WalkForInCollectionLoop(fc);
					return;
				default:
					throw new NotImplementedException();
			}
		}

		protected virtual void WalkForInCollectionLoop(ForInCollectionLoop fc)
		{
			View(fc);
			Walk(fc.Body);
		}

		protected virtual void View(ForInCollectionLoop fc) {}

		protected virtual void WalkForInRangeLoop(ForInRangeLoop fr)
		{
			View(fr);
			Walk(fr.Body);
		}

		protected virtual void View(ForInRangeLoop fr) { }

		protected virtual void WalkCaseStructure(CaseStructure c)
		{
			View(c);
			foreach (var component in c.Components)
				Walk(component);
		}

		protected virtual void View(CaseStructure c) { }

		protected virtual void WalkChioce(Choice c)
		{
			View(c); Walk(c.Components);
		}

		protected virtual void View(Choice c) { }

		protected virtual void WalkCbc(ChoicesBlockControl c)
		{
			View(c);
			var length = c.Choices.Count;
			for (var i = 0; i < length; i++)
				WalkChioce(c.Choices[i]);
		}

		protected virtual void View(ChoicesBlockControl c) { }

		protected virtual void WalkForLoop(ForLoop f)
		{
			View(f);
			Walk(f.Body);
		}

		protected virtual void View(ForLoop f) { }

		protected virtual void WalkIfElse(IfElseControl f)
		{
			View(f);
			Walk(f.Body);
			foreach (var elsif in f.Elsifs)
			{
				WalkElsif(elsif);
			}
			Walk(f.ElseBlock);
		}

		protected virtual void View(IfElseControl f) { }

		protected virtual void WalkElsif(ElsifControl e)
		{
			View(e);
			Walk(e.Body);
		}

		protected virtual void View(ElsifControl e) { }

		protected virtual void WalkMatchStructure(MatchStructure ms)
		{
			View(ms);
			foreach (var @case in ms.Cases)
				WalkCaseStructure(@case);
		}

		protected virtual void View(MatchStructure c) { }

		protected virtual void WalkWhileLoop(WhileLoop wl)
		{
			View(wl);
			Walk(wl.Body);
		}

		protected virtual void View(WhileLoop wl) { }

		protected virtual void View(Statement s){}
	}
}
