using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		ResourceReader(string path, ISlice<Token> tokens) : base(tokens)
		{
			PreResource = new ResourceBuilder(path);
			_StatementRules = new ResourceReaderStatementRule[] {
				new ResourceUsingStatementRule(PreResource)
			};
			_BodyRules = new ResourceReaderBodyRule[]
			{
				new ResourceReaderFunctionRule(PreResource),

			};
		}

		public static ResourceReader OpenResource(string src, string path)
		{
			var tokens = new CharStreamTokenizer().Tokenize(src);
			return new ResourceReader(path, Slice<Token>.Create(tokens, 0, tokens.Count));
		}

		protected override void OnReadAdjSemiColon(){}
	}
}
