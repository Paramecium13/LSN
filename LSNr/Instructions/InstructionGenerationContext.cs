namespace LSNr
{
	public enum ExpressionContext
	{
		/// <summary>
		/// A sub expression
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
		/// For example, variable access and index access instructions need to copy the struct but procedure calls and constructors don't
		/// </summary>
		Store,
		/// <summary>
		/// This expression's return value is being passed as a parameter--normally (i.e. structs will be copied).
		/// <para/>
		/// In this case, the procedure call expression will emit the copy struct instruction when needed.
		/// </summary>
		Parameter_Default,
	}

	/// <summary>
	/// The context in which instructions are generated.
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
			new InstructionGenerationContext(context, LabelFactory);
	}
}