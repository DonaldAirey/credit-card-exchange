namespace FluidTrade.Office
{

	using Aspose.Words.Fields;

	/// <summary>
	/// Any field not yet recognized by the parser.
	/// </summary>
	/// <remarks></remarks>
	public class UnhandledField : WordField
	{

		/// <summary>
		/// Creates a field not handled by the field evaluation logic.
		/// </summary>
		/// <param name="fieldstart">The start of the field.</param>
		public UnhandledField(FieldStart fieldstart) : base(fieldstart)
		{
		}

	}

}