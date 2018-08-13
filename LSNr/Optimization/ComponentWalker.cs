using LsnCore;
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
			var cs = c as ControlStructure;
			if (cs != null)
				Walk(cs);
			else View((Statement)c);
		}

		protected virtual void Walk(IList<Component> components)
		{
			var length = components.Count;
			for (int i = 0; i < length; i++)
				Walk(components[i]);
		}

		private void Walk(ControlStructure c)
		{
			if (c is CaseStructure cs)
			{
				WalkCaseStructure(cs);
				return;
			}
			if (c is Choice ch)
			{
				WalkChioce(ch);
				return;
			}
			if (c is ChoicesBlockControl cbc)
			{
				WalkCbc(cbc);
				return;
			}
			if (c is ForLoop fl)
			{
				WalkForLoop(fl);
				return;
			}
			if (c is IfElseControl ife)
			{
				WalkIfElse(ife);
				return;
			}
			if (c is MatchStructure ms)
			{
				WalkMatchStructure(ms);
				return;
			}
			if (c is WhileLoop wl)
				WalkWhileLoop(wl);
			throw new NotImplementedException();
		}

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
			for (int i = 0; i < length; i++)
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
			for(int i = 0; i < f.Elsifs.Count; i++)
			{
				WalkElsif(f.Elsifs[i]);
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
			for (int i = 0; i < ms.Cases.Count; i++)
				WalkCaseStructure(ms.Cases[i]);
		}

		protected virtual void View(MatchStructure c) { }

		protected virtual void WalkWhileLoop(WhileLoop wl)
		{
			View(wl);
			Walk(wl.Body);
		}

		protected virtual void View(WhileLoop wl) { }

		protected virtual void View(Statement s)
		{

		}
	}
}
