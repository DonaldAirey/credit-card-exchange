namespace FluidTrade.Core
{

	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Reflection;
	using System.ServiceModel;
	using System.Transactions;
	using System.Xml.Linq;

	/// <summary>
	/// Runs a script against a middle tier.
	/// </summary>
	public class ScriptLoader
	{

		// Public Instance Members
		public Boolean ForceLogin;
		public Boolean HasErrors;
		public Int32 MethodCount;
		public String FileName;
		public String ScriptName;
		public string UserName { get; set; }
		public string Password { get; set; }

		public Boolean LocalMode { get; set; }

		public event MessageEventHander NotifyMessage;

		// Private Instance Fields
		private Dictionary<String, ClientChannel> clientTable;

		/// <summary>
		/// Creates an object that runs a script against a middle tier.
		/// </summary>
		public ScriptLoader()
		{

			// This is used to print the statistics at the end of the execution.
			this.MethodCount = 0;

			// Users can be forced to specify the connection information even when preferences have been set by a previous 
			// session.  The default is to not force the login.
			this.ForceLogin = false;

			// This table is used to map the client names to a channel object.
			this.clientTable = new Dictionary<String, ClientChannel>();

		}

		/// <summary>
		/// Load the XML script.
		/// </summary>
		public void Load()
		{

			try
			{

				this.LoadUnsafe();

			}
			catch (ScriptLoaderException)
			{

				throw;

			}
			catch (Exception exception)
			{

				throw new ScriptLoaderException("Initialization failed", exception, null, 0);

			}

		}

		private void LoadUnsafe()
		{

			// This flag is set when an error occurs anywhere in the processing of the XML file.
			this.HasErrors = false;

			// Read the XML data from the specified file.
			if(NotifyMessage != null)
			{
				this.NotifyMessage(this, new MessageEventArgs(string.Format("Loading {0}", FileName)));
			}

			XDocument xDocument = XDocument.Load(this.FileName);

			if(NotifyMessage != null)
			{
				this.NotifyMessage(this, new MessageEventArgs("Processing"));
			}

			// The script name is stored in the root node.  The name is used in status and debugging messages.
			XAttribute nameAttribute = xDocument.Root.Attribute("name");
			this.ScriptName = nameAttribute == null ? "Unnamed Transaction" : nameAttribute.Value;

			// This will force the user interface to appear the next time a channel is opened.  It also releases any threads that
			// may have been waiting for the user to provide a set of credentials.  Threads enter this waiting state when the user
			// hits the 'Cancel' button on any screen that prompts for credentials.
			if(this.ForceLogin && this.LocalMode == false)
			{
				ChannelStatus.IsPrompted = true;
			}

			if(this.LocalMode == false)
				ChannelStatus.LoginEvent.Set();

			List<XElement> xElementList = new List<XElement>();
			foreach(XElement xElement in xDocument.Root.Elements())
			{
				xElementList.Add(xElement);
			}

			//default to be 10%
			int tickPercent = xElementList.Count / 10;
			if(tickPercent < 50)
				tickPercent = 50;
			else if(tickPercent > 1000)
				tickPercent = 1000;

			// Cycle through all of the children of the root node.  Transactions are executed as a unit.  Methods outside of a 
			// transaction element are executed alone.
			for(int elementIndex = 0; elementIndex < xElementList.Count; elementIndex++)
			{
				XElement xElement = xElementList[elementIndex];
				try
				{
					if(NotifyMessage != null && elementIndex % tickPercent == 0)
					{
						MessageEventArgs mea = new MessageEventArgs("Next Tick");
						mea.IsProgressTick = true;
						this.NotifyMessage(this, mea);
					}

					switch(xElement.Name.LocalName)
					{
						case "client":

							// A channel is required for each endpont that will be contacted during his load.
							CreateClient(xElement);
							break;

						case "method":
							if(this.tranactionScopeStack.Count == 0)
							{
								using(TransactionScope transactionScope = new TransactionScope())
								{
									// Parse and execute a single method out of the script.
									if(ExecuteMethod(xElement))
										this.MethodCount++;

									transactionScope.Complete();
								}
							}
							else
							{
								// Parse and execute a single method out of the script.
								if(ExecuteMethod(xElement))
									this.MethodCount++;
							}
							break;

						case "transaction":

							// This creates a collection of methods that are executed as a unit.
							ExecuteTransaction(xElement);
							break;

					}
				}
				catch(Exception exception)
				{
					EventLog.Error(string.Format("{0} {1} {2}", exception.Message, exception.ToString(), exception.StackTrace));

					ScriptLoaderException slEx = new ScriptLoaderException("Error in " + this.FileName, exception, xElement, this.MethodCount);

					if(NotifyMessage != null)
					{
						this.NotifyMessage(this, new MessageEventArgs(slEx.Message));
					}

					throw slEx;
				}
			}

			if(NotifyMessage != null)
			{
				this.NotifyMessage(this, new MessageEventArgs(string.Format("Processed {0} methods in file: {1}", MethodCount, Path.GetFileName(FileName))));
			}

			// One or more channels may have been created dynamicall from the information in the scripts.  These dynamic channels 
			// should be shut down gracefully before exiting.
			if(this.LocalMode == false)
			{
				foreach(KeyValuePair<String, ClientChannel> clientPair in this.clientTable)
				{
					MethodInfo closeMethod = clientPair.Value.ChannelType.GetMethod("Close");
					closeMethod.Invoke(clientPair.Value.ChannelObject, null);
				}
			}

		}

		/// <summary>
		/// Creates a client channel dynamically.
		/// </summary>
		/// <param name="interfaceElement">A description of the client channel.</param>
		private void CreateClient(XElement clientElement)
		{
			if(this.LocalMode == true)
				return;

			// These attributes describe the client channel.
			XAttribute nameAttribute = clientElement.Attribute("name");
			XAttribute typeAttribute = clientElement.Attribute("type");
			XAttribute endpointAttribte = clientElement.Attribute("endpoint");

			// Use reflection to pull the 'Type' attribute apart to find the assembly and type where the client channel is
			// declared.  This shreds the attribute for the values needed by reflection to find the assembly and type information
			// dynamically.
			String[] parts = typeAttribute.Value.Split(',');
			String typeName = parts[0];
			String assemblyString = String.Empty;
			for(int index = 1; index < parts.Length; index++)
				assemblyString += (assemblyString == String.Empty ? String.Empty : ",") + parts[index];
			Assembly assembly = Assembly.Load(assemblyString);
			Type channelType = assembly.GetType(typeName);
			if(channelType == null)
				throw new Exception(String.Format("The specified client {0} in assembly {1} can't be located", typeName,
					assemblyString));

			// Once the assembly is loaded, a channel is created using the endpoint information provided.
			Object channelObject = assembly.CreateInstance(typeName, false, BindingFlags.CreateInstance, null,
				new Object[] { endpointAttribte.Value }, CultureInfo.InvariantCulture, null);
			if(channelObject == null)
				throw new Exception(String.Format("Can't create a channel of type '{0}'.", typeName));

			if(String.IsNullOrEmpty(UserName) == false ||
				String.IsNullOrEmpty(Password) == false)
				SetUserNamePassword(channelObject);

			// Once the client channel is successfully created it is added to a dictionary that is available to each of the script 
			// elements.  The name is used to symbolically link a method with a channel on which that method can be executed.
			this.clientTable.Add(nameAttribute.Value, new ClientChannel(channelObject, channelType));

		}

		/// <summary>
		/// Stuff credentials into channel.
		/// </summary>
		/// <param name="assembly"></param>
		/// <param name="channelObject"></param>
		private void SetUserNamePassword(object channelObject)
		{

			Type channelType = channelObject.GetType();

			//Navigate to Channel -> ClientCredentials -> UserName
			object userCredentialsInstance = channelType.GetProperty("ClientCredentials").GetValue(channelObject, null);
			object userNameInstance = userCredentialsInstance.GetType().GetProperty("UserName").GetValue(userCredentialsInstance, null);

			//Set the UserName and Password properties.  
			PropertyInfo userNameInfo = userNameInstance.GetType().GetProperty("UserName");
			PropertyInfo passwordInfo = userNameInstance.GetType().GetProperty("Password");

			userNameInfo.SetValue(userNameInstance, UserName, null);
			passwordInfo.SetValue(userNameInstance, Password, null);
		}

		private Stack<TransactionScope> tranactionScopeStack = new Stack<TransactionScope>();
		/// <summary>
		/// Process a transaction.
		/// </summary>
		/// <param name="xmlNode">The node containing the transaction.</param>
		private void ExecuteTransaction(XElement xElement)
		{
			// Create an explicit transaction for the methods found at this node.
			using(TransactionScope transactionScope = new TransactionScope())
			{
				this.tranactionScopeStack.Push(transactionScope);
				// These variables are used to count the number of successful methods.  If a transaction fails, then all the
				// methods in the transaction have failed.  The total count of methods executed by the class does not reflect the 
				// failed methods in the failed transaction.
				bool isSuccessful = true;
				int methodCount = 0;

				// This will execute each of the methods in the transaction.  If a single method fails, then all the methods, even
				// the successful ones, will be rolled back.
				foreach(XElement methodElement in xElement.Elements())
					try
					{

						if(ExecuteMethod(methodElement))
							methodCount++;
						else
							isSuccessful = false;

					}
					catch(Exception exception)
					{

						throw new ScriptLoaderException("Error in " + this.FileName, exception, methodElement, this.MethodCount);

					}

				// At this point, all the methods were successful and the transaction can be committed and the global counter of
				// good methods reflects the successes.
				if(isSuccessful)
				{
					this.MethodCount += methodCount;
					transactionScope.Complete();
				}

				this.tranactionScopeStack.Pop();
			}
		}

		private XElement lastMethodElement = null;
		/// <summary>
		/// Creates a method plan from the parameters listed.
		/// </summary>
		/// <param name="methodNode">An XML node where the method and parameters are found.</param>
		private bool ExecuteMethod(XElement methodElement)
		{
			this.lastMethodElement = methodElement;
			// Indicates the success or failure of an individual method execution.
			bool isSuccessful = false;

			try
			{

				// The client channel on which this method is to be executed is described by the 'client' attribute.
				ClientChannel client;
				XAttribute clientAttribte = null;
				if(this.LocalMode == false)
				{
					clientAttribte = methodElement.Attribute("client");
					if(!clientTable.TryGetValue(clientAttribte.Value, out client))
						throw new Exception(String.Format("The client {0} hasn't been defined", clientAttribte.Value));
				}
				else
				{
					client = null;
				}
				// Reflection is used here to find the method to be executed.$
				XAttribute methodNameAttribute = methodElement.Attribute("name");
				MethodInfo methodInfo;
				object invokeObject;
				if(LocalMode == true)
				{
					Type t = Type.GetType("FluidTrade.Guardian.DataModel, FluidTrade.ServerDataModel, Version=1.3.0.0, Culture=neutral, PublicKeyToken=b200e265aff2c6ac");
					methodInfo = t.GetMethod(methodNameAttribute.Value);
					invokeObject = Activator.CreateInstance(t);
				}
				else
				{
					invokeObject = client.ChannelObject;
					methodInfo = client.ChannelType.GetMethod(methodNameAttribute.Value);
				}

				if(methodInfo == null)
					throw new Exception(String.Format("The method {0} isn't part of the library", methodNameAttribute.Value));

				// Each of the parameters from the XML is ripped into a has table indexed by the parameter name.  These values will
				// be mapped to each of the parameters found in the Reflection description of the method that is to be executed.
				Dictionary<String, XElement> parameterTable = new Dictionary<String, XElement>();
				foreach(XElement parameterElement in methodElement.Elements("parameter"))
				{
					XAttribute parameterNameAttribute = parameterElement.Attribute("name");
					parameterTable.Add(parameterNameAttribute.Value, parameterElement);
				}

				// Once the XML had been ripped into internal data structures and indexed by the parameter name, the values of
				// those parameters are evaluated based on the data type reqired by the parameters of the method found in the
				// assembly.
				ParameterInfo[] parameterInfoArray = methodInfo.GetParameters();
				Object[] parameterArray = new Object[parameterInfoArray.Length];
				for(int parameterIndex = 0; parameterIndex < parameterInfoArray.Length; parameterIndex++)
				{
					ParameterInfo parameterInfo = parameterInfoArray[parameterIndex];
					XElement parameterElement;
					if(parameterTable.TryGetValue(parameterInfo.Name, out parameterElement))
						parameterArray[parameterIndex] = ConvertElement(parameterInfo.ParameterType, parameterElement);
				}

				try
				{

					// At this point, all XML data has been converted to CLR datatypes and correlated to the position of their
					// respective parameters in an array.  The middle tier method can be called through the dynamic proxy.
					methodInfo.Invoke(invokeObject, parameterArray);

				}
				catch(TargetInvocationException targetInvocationException)
				{

					// Rethrow the target invocation exception.  This will make the exception handling logic work as though the
					// method was called directly.
					throw targetInvocationException.InnerException;

				}

				// The method invocation was successful at this point.
				isSuccessful = true;
				this.lastMethodElement = null;

			}
			catch(FaultException<IndexNotFoundFault> indexNotFoundException)
			{

				// The record wasn't found.
				Console.WriteLine(Properties.Resources.IndexNotFoundError, indexNotFoundException.Detail.IndexName,
					indexNotFoundException.Detail.TableName);
				throw;

			}
			catch(FaultException<RecordNotFoundFault> recordNotFoundException)
			{

				// The record wasn't found.
				Console.WriteLine(Properties.Resources.RecordNotFoundError,
					CommonConversion.FromArray(recordNotFoundException.Detail.Key),
					recordNotFoundException.Detail.TableName);
				throw;

			}
			catch(FaultException<ArgumentFault> argumentFaultException)
			{

				// The arguments weren't in the proper range.
				Console.WriteLine(argumentFaultException.Detail.Message);
				throw;

			}
			catch(FaultException<FormatFault> formatFaultException)
			{

				// The arguments weren't in the proper range.
				Console.WriteLine(formatFaultException.Detail.Message);
				throw;

			}
			catch(FaultException<ExceptionDetail> exceptionDetail)
			{

				// This is a general purpose exception for debugging.
				Console.WriteLine(exceptionDetail.Message);
				throw;

			}
#if false
			catch (CommunicationException communicationException)
			{

				// This indicates there was a communication exception.
				Console.WriteLine(communicationException.Message);

			}
#endif
			// This is the final indication of whether the method was successful or not.
			return isSuccessful;

		}

		/// <summary>
		/// Extracts the data type from an XElement.
		/// </summary>
		/// <param name="xElement">An XElement containing a parameter.</param>
		/// <returns>The native CLR datatype described by the 'dataType' attribute.</returns>
		private Type GetElementType(Type originalType, XElement xElement)
		{

			// All the currently loaded assemblies are searched for a datatype described by the 'dataType' attribute.
			XAttribute dataTypeAttribute = xElement.Attribute("type");
			if(dataTypeAttribute != null)
				foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					Type type = assembly.GetType(dataTypeAttribute.Value);
					if(type != null)
						return type;
				}

			// If there is no override the use the original type.
			return originalType;

		}

		/// <summary>
		/// Converts an XElement describing a value to a native CLR value.
		/// </summary>
		/// <param name="type">The target datatype.</param>
		/// <param name="parameterElement">An XElement containing a String representation of a value.</param>
		/// <returns>The native CLR value of the described value.</returns>
		private Object ConvertElement(Type type, XElement parameterElement)
		{

			// This method takes the target data type for a parameter and converts the text representation of the value into the CLR equivalent.  Key values
			// are passed to methods as arrays of generic objects.  An element of this array can be specified as a single 'value' attribute of the 'parameter'
			// element, and/or it can be one or more 'value' elements that are children of the 'parameter' element.
			if(type == typeof(Object[]))
			{

				// The values can be found in a single attribute of the 'parameter' element or be listed as children.  This list collects both methods of
				// describing values and constructs a single array when all elements and attributes are parsed.
				List<Object> valueList = new List<Object>();

				// An attribute can be used to desribe a value.  An optional 'Type' attribute can specify what type of conversion is used to evaluate the CLR
				// value.
				XAttribute valueAttribute = parameterElement.Attribute("value");
				if(valueAttribute != null)
					valueList.Add(ConvertValue(GetElementType(typeof(String), parameterElement), valueAttribute.Value));

				// It is possible to specify the value using the content of an XML element or through an "import" statement.  This will cycle through any nodes
				// of the parameter looking for additional nodes containing the data for the parameter.
				foreach(XObject xObject in parameterElement.Nodes())
				{

					// This uses the element content as the value for the parameter.
					if(xObject is XText)
					{
						XText xText = xObject as XText;
						valueList.Add(ConvertValue(typeof(String), xText.Value));
					}

					// This will import an XML file from the given path.
					if(xObject is XElement)
					{

						// This element holds special instructions for the parameter.
						XElement xElement = xObject as XElement;

						// Values for a key can be specified as child elements of the parameter.
						if(xElement.Name == "value")
							valueList.Add(ConvertValue(GetElementType(typeof(String), xElement), xElement));

						// This special instruction allows the value of a parameter to come from an external file.  This is used primary to load XML content
						// into a record.
						if(xElement.Name == "import")
						{
							XAttribute xAttribute = xElement.Attribute("path");
							String path = Path.IsPathRooted(xAttribute.Value) ? xAttribute.Value :
								Path.Combine(Path.GetDirectoryName(this.FileName), xAttribute.Value);
							XDocument xDocument = XDocument.Load(path);
							valueList.Add(ConvertValue(type, xDocument.ToString()));
						}

						// A 'load' element will read a binary resource into a byte array.
						if(xElement.Name == "load")
						{
							XAttribute xAttribute = xElement.Attribute("path");
							String path = Path.IsPathRooted(xAttribute.Value) ? xAttribute.Value :
								Path.Combine(Path.GetDirectoryName(this.FileName), xAttribute.Value);
							using(FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
							{
								Byte[] binaryData = new Byte[fileStream.Length];
								fileStream.Read(binaryData, 0, Convert.ToInt32(fileStream.Length));
								return ConvertValue(type, Convert.ToBase64String(binaryData));
							}
						}

					}

				}

				// This array is most often used as a key to find a record in a table.
				return valueList.ToArray();

			}
			else
			{

				// If the XML specifies an override for the native data type of the parameter, make sure that the new value's type 
				// is compatible with the parameter.
				Type originalType = type;
				type = GetElementType(type, parameterElement);
				if(type != originalType && !type.IsSubclassOf(originalType))
					throw new Exception(String.Format("Can't cast a parameter of type {0} to {1}.", type, originalType));

				// It is possible to specify the value using the content of an XML element or through an "import" statement.  This
				// will cycle through any nodes of the parameter looking for additional nodes containing the data for the 
				// parameter.
				foreach(XObject xObject in parameterElement.DescendantNodes())
				{

					// This uses the element content as the value for the parameter.
					if(xObject is XText)
						return ConvertValue(type, (xObject as XText).Value);

					// This will import an XML file from the given path.
					if(xObject is XElement)
					{

						// This object describes an XML element.
						XElement xElement = xObject as XElement;

						// An 'import' element will read an XML file into a string.
						if(xElement.Name == "import")
						{
							XAttribute xAttribute = xElement.Attribute("path");
							String path = Path.IsPathRooted(xAttribute.Value) ? xAttribute.Value :
								Path.Combine(Path.GetDirectoryName(this.FileName), xAttribute.Value);
							XDocument xDocument = XDocument.Load(path);
							return ConvertValue(type, xDocument.ToString());
						}

						// A 'load' element will read a binary resource into a byte array.
						if(xElement.Name == "load")
						{
							XAttribute xAttribute = xElement.Attribute("path");
							String path = Path.IsPathRooted(xAttribute.Value) ? xAttribute.Value :
								Path.Combine(Path.GetDirectoryName(this.FileName), xAttribute.Value);
							using(FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
							{
								Byte[] binaryData = new Byte[fileStream.Length];
								fileStream.Read(binaryData, 0, Convert.ToInt32(fileStream.Length));
								return ConvertValue(type, Convert.ToBase64String(binaryData));
							}
						}

					}

				}

				// Simple parameters are converted according to the Reflection information found in the description of the
				// parameters to a method in an assembly.  If a value isn't given for a parameter, it is explicitly set to 
				// DBNull.Value.
				XAttribute valueAttribute = parameterElement.Attribute("value");
				return valueAttribute == null ? DBNull.Value : ConvertValue(type, valueAttribute.Value);

			}

		}

		/// <summary>
		/// Convert text to a CLR object based on the target datatype.
		/// </summary>
		/// <param name="type">The target datatype.</param>
		/// <param name="value">The string representation of the value.</param>
		/// <returns>The CLR equivalent of the given value.</returns>
		private Object ConvertValue(Type type, String value)
		{

			// Use the destination type to drive the creation of a statement that will convert text into a CLR value.
			switch (type.ToString())
			{

				case "System.Object":
					return value;

				case "System.Boolean":
					return Convert.ToBoolean(value);

				case "System.Int16":
					return Convert.ToInt16(value);

				case "System.Int32":
					return Convert.ToInt32(value);

				case "System.Int64":
					return Convert.ToInt64(value);

				case "System.Decimal":
					return Convert.ToDecimal(value);

				case "System.Double":
					return Convert.ToDouble(value);

				case "System.DateTime":
					return Convert.ToDateTime(value);

				case "System.String":
					return value;

				case "System.Guid":
					return new Guid(value);

				case "System.Byte[]":
					return Convert.FromBase64String(value);

				default:
					if (type.IsEnum)
						return  Enum.Parse(type, value);
					break;

			}

			// Throw the exception to catch any data types that aren't converted above.
			throw new Exception(String.Format("There is no conversion expression that can be created for a {0} type.", type));

		}

		/// <summary>
		/// Convert text to a CLR object based on the target datatype.
		/// </summary>
		/// <param name="type">The target datatype.</param>
		/// <param name="element">The xml element.</param>
		/// <returns>The CLR equivalent of the given value.</returns>
		private Object ConvertValue(Type type, XElement element)
		{

			if (element.IsEmpty)
				return DBNull.Value;
			else
				return this.ConvertValue(type, element.Value);

		}

		public delegate void MessageEventHander(ScriptLoader sender, MessageEventArgs mea);
		public class MessageEventArgs : System.EventArgs
		{
			public MessageEventArgs(string message)
			{
				this.Message = message;
			}

			public string Message { get; private set; }

			public bool IsProgressTick { get; set; }
		}
	}

}
