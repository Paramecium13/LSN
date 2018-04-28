using LsnCore;
using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSNr
{
	public static class TypeContainerExtensions
	{
		/// <summary>
		/// Parses a type name, returning the type object.
		/// </summary>
		/// <param name="self"></param>
		/// <param name="tokens"></param>
		/// <param name="startIndex"></param>
		/// <param name="endIndex"> The index of the token after the last token used in the type.</param>
		/// <returns></returns>
		public static LsnType ParseType(this ITypeContainer self, IReadOnlyList<Token> tokens, int startIndex, out int endIndex)
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
					Console.WriteLine($"Error: expected token '<', received '{tokens[i].Value}'.");
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
				return gType.GetType(generics.Select(t => t.Id).ToList());
			}
			endIndex = -1;
			return null;
		}

		/// <summary>
		/// Parses a type name, returning the TypeId object.
		/// </summary>
		/// <param name="self"></param>
		/// <param name="tokens"></param>
		/// <param name="startIndex"></param>
		/// <param name="endIndex"> The index of the token after the last token used in the type.</param>
		/// <returns></returns>
		public static TypeId ParseTypeId(this ITypeContainer self, IReadOnlyList<Token> tokens, int startIndex, out int endIndex)
		{
			int i = startIndex;
			var tName = tokens[startIndex].Value;
			if (self.TypeExists(tName))
			{
				endIndex = i + 1;
				return self.GetTypeId(tokens[i].Value);
			}
			else if (self.GenericTypeExists(tName))
			{
				if (tokens[++i].Value != "<")
				{
					Console.WriteLine($"Error: expected token '<', received '{tokens[i].Value}'.");
					endIndex = -1;
					return null;
				}
				++i;
				GenericType gType = self.GetGenericType(tName);
				var generics = new List<TypeId>();
				while (tokens[i].Value != ">")
				{
					generics.Add(self.ParseTypeId(tokens, i, out i));
					if (tokens[i].Value == ",") i++; // else error?
				}
				endIndex = i + 1;
				return gType.GetType(generics).Id;
			}
			endIndex = -1;
			return null;
		}

	}

}