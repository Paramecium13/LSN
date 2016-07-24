using LsnCore;
using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tokens;

namespace LSNr
{
	public interface ITypeContainer
	{
		LsnType GetType(string name);
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
		public static LsnType ParseType(this ITypeContainer self, List<IToken> tokens, int startIndex, out int endIndex)
		{
			int i = startIndex;
			var tName = tokens[startIndex].Value;
			if (self.TypeExists(tName))
			{
				endIndex = i + 1;
				return self.GetType(tokens[i].Value);
			}
			else if (self.GenericTypeExists(tName))
			{
				if(tokens[++i].Value != "<")
				{
					Console.WriteLine($"Error: expected token '<', recieved '{tokens[i].Value}'.");
					endIndex = -1;
					return null;
				}
				++i;
				GenericType gType = self.GetGenericType(tName);
				var generics = new List<LsnType>();
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