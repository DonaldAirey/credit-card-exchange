namespace FluidTrade.Core
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Xml.Linq;

	/// <summary>
	/// Exception information for script loader errors.
	/// </summary>
	public class ScriptLoaderException : Exception
	{

		private XElement currentElement;
		private Int32 methodIndex;

		/// <summary>
		/// Create a new script loader exception.
		/// </summary>
		/// <param name="message">The message to accompany the exception.</param>
		/// <param name="innerException">The original exception.</param>
		/// <param name="currentElement">The xml element the script loader was processing.</param>
		/// <param name="methodIndex">The index of the element in the document.</param>
		public ScriptLoaderException(String message, Exception innerException, XElement currentElement, Int32 methodIndex)
			: base(message, innerException)
		{

			this.currentElement = currentElement;
			this.methodIndex = methodIndex;

		}

		/// <summary>
		/// The XML Element that the script loader was processing when the exception happened.
		/// </summary>
		public XElement CurrentElement
		{

			get { return this.currentElement; }

		}

		/// <summary>
		/// The index of CurrentElement in the document, not counting transactions.
		/// </summary>
		public Int32 MethodIndex
		{

			get { return this.methodIndex; }

		}

		private XElement InnerElement
		{
			get
			{
				if(this.InnerException is ScriptLoaderException)
					return ((ScriptLoaderException)this.InnerException).InnerElement;

				return this.currentElement;
			}
		}

		private System.ServiceModel.FaultException FindInnerFaultException()
		{
			Exception innerException = this.InnerException;
			while(innerException != null)
			{
				if(innerException is System.ServiceModel.FaultException)
					return (System.ServiceModel.FaultException)innerException;

				innerException = innerException.InnerException;
			}

			if (this.InnerException != null)
				return new System.ServiceModel.FaultException(String.Format("Inner exception: {0}: {1}", this.InnerException.GetType(), this.InnerException.Message));
			else
				return null;
		}

		public override string Message
		{
			get
			{
				string elementString = null;
				XElement element = InnerElement;
				if(InnerElement != null)
					elementString = InnerElement.ToString();

				System.ServiceModel.FaultException innerFaultEx = this.FindInnerFaultException();

				object faultDetail = null;
				if(innerFaultEx != null)
				{
					System.Reflection.PropertyInfo pi = innerFaultEx.GetType().GetProperty("Detail");

					if (pi != null)
						faultDetail = pi.GetValue(innerFaultEx, null);
					else
						faultDetail = String.Format("{0}: {1}", innerFaultEx.GetType(), innerFaultEx.Message);
				}

				return string.Format(
					"ScriptLoaderException: {0} \r\n {1}\r\nElement:{2}\r\nMethodIndex:{3}\n\n{4}",
					base.Message,
					(faultDetail == null) ? this.InnerException.ToString() : faultDetail.ToString(),
					elementString,
					methodIndex,
					this.StackTrace);
			}
		}
		public override string ToString()
		{
			return this.Message;
		}
	}

}
