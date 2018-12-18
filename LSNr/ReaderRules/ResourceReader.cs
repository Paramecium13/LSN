using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Utilities;
using LSNr.ReaderRules;

namespace LSNr
{
	class ResourceReader : RuledReader<ResourceReaderStatementRule, ResourceReaderBodyRule>
	{
		readonly IPreResource PreResource;

		readonly ResourceReaderStatementRule[] _StatementRules;
		protected override IEnumerable<ResourceReaderStatementRule> StatementRules => _StatementRules;

		readonly ResourceReaderBodyRule[] _BodyRules;
		protected override IEnumerable<ResourceReaderBodyRule> BodyRules => _BodyRules;

		internal bool Valid => PreResource.Valid;

		ResourceReader(string path, ISlice<Token> tokens, DependencyWaiter waiter) : base(tokens)
		{
			PreResource = new ResourceBuilder(path);
			_StatementRules = new ResourceReaderStatementRule[] {
				new ResourceUsingStatementRule(PreResource, waiter)
			};
			_BodyRules = new ResourceReaderBodyRule[]
			{
				new ResourceReaderFunctionRule(PreResource),
				new ResourceReaderStructRule(PreResource),
				new ResourceReaderRecordRule(PreResource),
				new ResourceReaderHostInterfaceRule(PreResource),
				new ResourceReaderScriptClassRule(PreResource)
			};
		}

		internal LsnResourceThing Read()
		{
			ReadTokens();
			return PreResource.Parse();
		}

		public static ResourceReader OpenResource(string src, string path, DependencyWaiter waiter)
			=> new ResourceReader(path, new CharStreamTokenizer().Tokenize(src), waiter);

		protected override void OnReadAdjSemiColon(){}
	}
}
