namespace LSNr
{
	/// <summary>
	/// A generalization of where/how the expression is being used.
	/// </summary>
	public enum ExpressionContext
	{
		/// <summary>
		/// The expression's result will be used in some internal method/function (e.g. the text for 'Register Choice')
		/// </summary>
		Internal,

		/// <summary>
		/// A sub expression. The value of this sub expression won't be stored.
		/// </summary>
		SubExpression,

		/// <summary>
		/// A field of this expression's return value is being written to.
		/// </summary>
		FieldWrite,
		
		/// <summary>
		/// For an array or list: this expression's result will have an item stored in it.
		/// </summary>
		ItemWrite,
		
		/// <summary>
		/// This expression's return value is being stored somewhere.
		/// <para/>
		/// It is up to the expression given this context to emit the copy struct instruction when needed.
		/// <para/>
		/// For example, variable access and index access instructions need to copy the struct but procedure calls and constructors, both of which create instances, don't.
		/// </summary>
		Store,
		
		/// <summary>
		/// This expression's return value is being passed as a parameter--normally (i.e. structs will be copied).
		/// <para/>
		/// In this case, the procedure call expression will emit the copy struct instruction when needed.
		/// </summary>
		Parameter_Default,

		/// <summary>
		/// This expression's return value will have one of its methods called.
		/// </summary>
		MethodCall,

		/// <summary>
		/// Jump to <see cref="InstructionGenerationContext.ShortCircuitLabelA"/> (and pop) if false.
		/// </summary>
		ShortCircuitOnFalse,

		/// <summary>
		/// Jump to <see cref="InstructionGenerationContext.ShortCircuitLabelA"/> (and pop) if true.
		/// </summary>
		ShortCirtuitOnTrue,

		/// <summary>
		/// Jump to <see cref="InstructionGenerationContext.ShortCircuitLabelA"/> when true. <see cref="InstructionGenerationContext.ShortCircuitLabelB"/>
		/// is the instruction after jump.
		/// </summary>
		JumpTrueStatement,

		/// <summary>
		/// Jump to <see cref="InstructionGenerationContext.ShortCircuitLabelA"/> when false. <see cref="InstructionGenerationContext.ShortCircuitLabelB"/>
		/// is the instruction after jump.
		/// </summary>
		JumpFalseStatement,

		/// <summary>
		/// The expression's value is being returned.
		/// </summary>
		ReturnValue
	}

	/// <summary>
	/// The context in which instructions are generated from <see cref="LsnCore.Expressions.IExpression"/>s.
	/// </summary>
	public class InstructionGenerationContext
	{
		/// <summary>
		/// Gets the label factory.
		/// </summary>
		public InstructionLabel.Factory LabelFactory { get; }

		/// <summary>
		/// Gets the context.
		/// </summary>
		public ExpressionContext Context { get; }

		/// <summary>
		/// Gets or sets the short circuit label A. When in a conditional branch, this is the branch's target.
		/// </summary>
		public InstructionLabel ShortCircuitLabelA { get; set; }

		/// <summary>
		/// Used in conditional branches. This is the instruction after the conditional jump instruction.
		/// </summary>
		public InstructionLabel ShortCircuitLabelB { get; set; }

		public bool WantsBoolReturnValue { get; set; }

		public InstructionGenerationContext(ExpressionContext context, InstructionLabel.Factory labelFactory)
		{
			Context = context;
			LabelFactory = labelFactory;
		}

		/// <summary>
		/// Creates a copy of this with a different context.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		public InstructionGenerationContext WithContext(ExpressionContext context) =>
			new InstructionGenerationContext(context, LabelFactory)
			{
				ShortCircuitLabelA = ShortCircuitLabelA,
				ShortCircuitLabelB = ShortCircuitLabelB,
				WantsBoolReturnValue = WantsBoolReturnValue,
			};
	}
}