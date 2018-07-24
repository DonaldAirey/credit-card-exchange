namespace FluidTrade.PresentationGenerator.PresentationClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// A description of an event that indicates a property has changed.
	/// </summary>
	class PropertyChangedEvent : CodeMemberEvent
	{

		/// <summary>
		/// A description of an event that indicates a property has changed.
		/// </summary>
		public PropertyChangedEvent()
		{

			//		/// <summary>
			//		/// Indicates that one of the object's properties has changed.
			//		/// <summary>
			//		public event global::System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Indicates that one of the object's properties has changed.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public;
			this.Type = new CodeGlobalTypeReference(typeof(System.ComponentModel.PropertyChangedEventHandler));
			this.Name = "PropertyChanged";

		}

	}

}
