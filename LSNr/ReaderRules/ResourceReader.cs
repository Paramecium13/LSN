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
	internal class ResourceReader : RuledReader<ResourceReaderStatementRule, ResourceReaderBodyRule>
	{
		private readonly IPreResource PreResource;

		private readonly ResourceReaderStatementRule[] _StatementRules;
		protected override IEnumerable<ResourceReaderStatementRule> StatementRules => _StatementRules;

		private readonly ResourceReaderBodyRule[] _BodyRules;
		protected override IEnumerable<ResourceReaderBodyRule> BodyRules => _BodyRules;

		internal bool Valid => PreResource.Valid;

		private ResourceReader(string path, ISlice<Token> tokens) : base(tokens)
		{
			PreResource = new ResourceBuilder(path);
			_StatementRules = new ResourceReaderStatementRule[] {
				new ResourceUsingStatementRule(PreResource),
				new ResourceHandleTypeStatementRule(PreResource)
			};
			_BodyRules = new ResourceReaderBodyRule[]
			{
				new ResourceReaderFunctionRule(PreResource),
				new ResourceReaderStructRule(PreResource),
				new ResourceReaderRecordRule(PreResource),
				new ResourceReaderHostInterfaceRule(PreResource),
				new ResourceReaderScriptClassRule(PreResource),
				new ResourceReaderConversationRule(PreResource)
			};
		}

		internal LsnResourceThing Read()
		{
			ReadTokens();
			return PreResource.Parse();
		}

		public static ResourceReader OpenResource(string src, string path)
			=> new ResourceReader(path, new CharStreamTokenizer().Tokenize(src));

		protected override void OnReadAdjSemiColon(ISlice<Token>[] attributes) {}
	}
}
