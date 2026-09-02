#nullable enable
namespace Microsoft.Maui
{
	public partial class Semantics
	{
		public string? Description { get; set; }

		public string? Hint { get; set; }

#if IOS || MACCATALYST
		internal bool? IsInAccessibleTree { get; set; }

		internal bool HasAccessibleDescendant { get; set; }
#endif

		public bool IsHeading => HeadingLevel != SemanticHeadingLevel.None;

		public Semantics()
		{
		}

		/// <summary>
		/// WinUI: Sets the heading level of the element
		/// Other: Supports only one heading level, so setting this to 
		/// any level will enable it as a heading.
		/// </summary>
		public SemanticHeadingLevel HeadingLevel { get; set; } = SemanticHeadingLevel.None;

		public override string ToString()
		{
			return string.Format("Description: {0}, Hint: {1}, HeadingLevel: {2}", Description, Hint, HeadingLevel);
		}
	}
}