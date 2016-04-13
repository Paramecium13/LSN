using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core
{
	/// <summary>
	/// 
	/// </summary>
	public class ExternalFunction : Function
	{

		public override bool HandlesScope
		{
			get
			{
				if (External == null) throw new ApplicationException($"Error: unresolved external function: \"{Name}\".");
				return External.HandlesScope;
			}
		}

		private bool _IsResolved;
		/// <summary>
		/// 
		/// </summary>
		public bool IsResolved => _IsResolved;

		/// <summary>
		/// 
		/// </summary>
		private Function External;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parameters"></param>
		/// <param name="returnType"></param>
		public ExternalFunction(string name, List<Parameter> parameters, LSN_Type returnType = null)
			:base(parameters)
		{
			Name = name;
			ReturnType = returnType;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="function"></param>
		/// <param name="suppressNameMatching"> Allows this to be resolved to a function with a different name.</param>
		public void Resolve(Function function, bool suppressNameMatching = false)
		{
			if (IsResolved)
				throw new ApplicationException($"Error: function \"{Name}\" has already been resolved.");

			if (function == null)
				throw new ArgumentNullException(nameof(function), "Error: cannot resolve an external function to null.");

			if (!suppressNameMatching && function.Name != Name)
				throw new ApplicationException($"Error: attempted to resolve external function\"{Name}\" to a function with a different name, \"{function.Name}\".");

			if (function.Parameters.Count != Parameters.Count || Parameters.All(p => function.Parameters.Any(q => q.Name == p.Name && q.Type == p.Type))
				|| function.Parameters.All(p => Parameters.Any(q => q.Name == p.Name && q.Type == p.Type)))
				throw new ApplicationException("Error: attempted to resolve the external function \"{Name}\" to a function whose parameters do not match its parameters.");

			if (function is ExternalFunction)
			{
				// Throw exception?
				if ((function as ExternalFunction).IsResolved) Resolve((function as ExternalFunction).External, suppressNameMatching);
				else throw new ApplicationException($"Error: attempted to resolve the external function \"{Name}\" with an unresolved external function.");
			}
			else
			{
				Environment = function.Environment;
				External = function;
			}
		}


		public override ILSN_Value Eval(Dictionary<string, ILSN_Value> args, IInterpreter i)
		{
			if (External == null)
				throw new ApplicationException("Unresolved external function:" + Name);
			return External.Eval(args, i);
		}

	}
}
