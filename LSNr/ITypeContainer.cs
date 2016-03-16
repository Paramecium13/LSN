using LSN_Core;
using LSN_Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tokens;

namespace LSNr
{
	public interface ITypeContainer
	{
		LSN_Type GetType(string name);
		bool TypeExists(string name);
		bool GenericTypeExists(string name);
		GenericType GetGenericType(string name);
    }

	public static class TypeContainerExtensions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="tokens"></param>
		/// <param name="startIndex"></param>
		/// <param name="endIndex"> The index of the token after the last token used in the type.</param>
		/// <returns></returns>
		public static LSN_Type ParseType(this ITypeContainer self, List<IToken> tokens, int startIndex, out int endIndex)
		{
			int i = startIndex;
			if(self.TypeExists(tokens[i].Value))
			{
				endIndex = i + 1;
				return self.GetType(tokens[i].Value);
			}
			else if (self.GenericTypeExists(tokens[i].Value))
			{
				if(tokens[++i].Value != "<")
				{
					Console.WriteLine($"Error: expected token '<', recieved '{tokens[i].Value}'.");
					endIndex = -1;
					return null;
				}
				++i;
				GenericType gType = self.GetGenericType(tokens[i].Value);
				var generics = new List<LSN_Type>();
				while(tokens[i].Value != ">")
				{
					generics.Add(self.ParseType(tokens, i, out i));
					if(tokens[i].Value == ",") i++; // else error?
				}
				endIndex = i + 1;
				return gType.GetType(generics);
			}
			endIndex = -1;
			return null;
		}

	}

}