using LsnCore;
using LsnCore.Types;
using LsnCore.Utilities;
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
			var i = startIndex;
			var tName = tokens[startIndex].Value;
			if (self.TypeExists(tName))
			{
				endIndex = i + 1;
				return self.GetType(tokens[i].Value);
			}
			if (self.GenericTypeExists(tName))
			{
				if(tokens[++i].Value != "<")
				{
					Console.WriteLine($"Error: expected token '<', received '{tokens[i].Value}'.");
					endIndex = -1;
					return null;
				}
				++i;
				var gType = self.GetGenericType(tName);
				var generics = new List<LsnType>();
				while(tokens[i].Value != ">")
				{
					generics.Add(self.ParseType(tokens, i, out i));
					if(tokens[i].Value == ",") i++; // else error?
				}
				endIndex = i + 1;
				var ty = gType.GetType(generics.Select(t => t.Id).ToArray());
				self.GenericTypeUsed(ty.Id);
				return ty;
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
			var i = startIndex;
			var tName = tokens[startIndex].Value;
			if (self.TypeExists(tName))
			{
				endIndex = i + 1;
				var type = self.GetTypeId(tokens[i].Value);
				if(tokens.TestAt(i+1, t => t.Value == "["))
				{
					// ToDo: Arrays...
					throw new NotImplementedException("Arrays");
				}
			}
			if (self.GenericTypeExists(tName))
			{
				if (tokens[++i].Value != "<")
				{
					Console.WriteLine($"Error: expected token '<', received '{tokens[i].Value}'.");
					endIndex = -1;
					return null;
				}
				++i;
				var gType = self.GetGenericType(tName);
				var generics = new List<TypeId>();
				try
				{
					while (tokens[i].Value != ">")
					{
						generics.Add(self.ParseTypeId(tokens, i, out i));
						if (tokens[i].Value == ",") i++; // else error?
					}
				}
				catch (Exception)
				{
					throw;
				}
				endIndex = i + 1;
				var tId = gType.GetType(generics.ToArray()).Id;
				self.GenericTypeUsed(tId);
				return tId;
			}
			endIndex = -1;
			return null;
		}

	}

}