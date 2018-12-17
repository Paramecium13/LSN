using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Utilities;
using LSNr.ReaderRules;

namespace LSNr
{
	class ResourceReader : ReaderBase
	{
		private readonly IPreResource PreResource;

		private readonly ResourceReaderStatementRule[] StatementRules;

		private readonly ResourceReaderBodyRule[] BodyRules;

		ResourceReader(string src, string path, ISlice<Token> tokens) : base(tokens)
		{

		}

		public static ResourceReader OpenResource(string src, string path)
		{
			var tokens = new CharStreamTokenizer().Tokenize(src);
			return new ResourceReader(src, path, Slice<Token>.Create(tokens, 0, tokens.Count));
		}

		protected override void OnReadAdjSemiColon(){}

		protected override void OnReadBody(ISlice<Token> headTokens, ISlice<Token> bodyTokens)
		{
			throw new NotImplementedException();
		}

		protected override void OnReadStatement(ISlice<Token> tokens)
		{
			throw new NotImplementedException();
		}
	}
}
