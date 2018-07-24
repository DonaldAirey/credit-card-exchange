namespace FluidTrade.ClientGenerator
{

    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Runtime.InteropServices;
    using FluidTrade.Core;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
	/// Create a generator for the middle tier of a data model.
	/// </summary>
	[Guid("47424E81-BE5A-4a56-83CC-497407428E05")]
	public class ClientGenerator : Microsoft.VisualStudio.Shell.Interop.IVsSingleFileGenerator
	{

		// Private Members
		private CodeDomProvider codeProvider;
		private CodeGeneratorOptions codeGeneratorOptions;

		/// <summary>
		/// Create a generator for the middle tier of a data model.
		/// </summary>
		public ClientGenerator()
		{

			// Initialize the object to generate C# code that is compatible with the default settings in Visual Studio.
			this.codeProvider = CodeDomProvider.CreateProvider("C#");
			this.codeGeneratorOptions = new CodeGeneratorOptions();
			this.codeGeneratorOptions.BracingStyle = "C";
			this.codeGeneratorOptions.IndentString = "\t";

		}

		#region IVsSingleFileGenerator Members

		/// <summary>
		/// Gets the default extension for the selected code generator.
		/// </summary>
		/// <param name="pbstrDefaultExtension">The suffix to be appended to the generated file.</param>
		/// <returns>S_OK</returns>
		public int DefaultExtension(out string pbstrDefaultExtension)
		{

			// This insures that the string will not be empty.
			pbstrDefaultExtension = string.Empty;

			// This insures that the period is part of the default extension.
			string defaultExtension = this.codeProvider.FileExtension;
			if (defaultExtension != null && defaultExtension.Length > 0 && defaultExtension[0] != '.')
				defaultExtension = "." + defaultExtension;

			// If the extension isn't empty, add the '.Designer' to the suffix so the visual studio will handle the file correctly
			// once it has been generated.
			if (!string.IsNullOrEmpty(defaultExtension))
				pbstrDefaultExtension = ".Designer" + defaultExtension;

			// This indicates that the extension was handled.
			return VSConstants.S_OK;

		}

		/// <summary>
		/// Generate the code from the custom tool.
		/// </summary>
		/// <param name="wszInputFilePath">The name of the input file.</param>
		/// <param name="bstrInputFileContents">The contents of the input file.</param>
		/// <param name="wszDefaultNamespace">The namespace FluidTrade.ClientGenerator the generated code.</param>
		/// <param name="pbstrOutputFileContents">The generated code.</param>
		/// <param name="pbstrOutputFileContentSize">The buffer size of the generated code.</param>
		/// <param name="pGenerateProgress">An indication of the tools progress.</param>
		/// <returns>0 indicates the tool handled the command.</returns>
		public int Generate(string wszInputFilePath, string bstrInputFileContents, string wszDefaultNamespace,
			IntPtr[] rgbOutputFileContents, out uint pcbOutput, IVsGeneratorProgress pGenerateProgress)
		{

			// Throw an execption if there is nothing to process.
			if (bstrInputFileContents == null)
				throw new ArgumentNullException(bstrInputFileContents);

			// This schema describes the data model that is to be generated.
			DataModelSchema dataModelSchema = new DataModelSchema(bstrInputFileContents);
			dataModelSchema.GeneratorType = typeof(ClientGenerator);
			dataModelSchema.TargetNamespace = wszDefaultNamespace;

			// This is where all the work is done to translate the input schema into the CodeDOM for the data model and the CodeDOM
			// for the interface to that data model.
			CodeCompileUnit codeCompileUnit = new CodeCompileUnit();
			codeCompileUnit.Namespaces.Add(new Namespace(dataModelSchema));
			CodeNamespace emptyNamespace = new CodeNamespace();
			emptyNamespace.Types.Add(new FluidTrade.Core.TargetInterface.TargetInterface(dataModelSchema));
			emptyNamespace.Types.Add(new FluidTrade.ClientGenerator.DataSetClientClass.DataSetClientClass(dataModelSchema));
			codeCompileUnit.Namespaces.Add(emptyNamespace);

			// If a handler was provided for the generation of the code, then call it with an update.
			if (pGenerateProgress != null)
				pGenerateProgress.Progress(50, 100);

			// This will generate the target source code in the language described by the CodeDOM provider.
			StringWriter stringWriter = new StringWriter();
			//There is no elegant way to generate preprocessor directives through CodeDom.  All the solutions
			//require string replacement after the fact.  So not elegant but effective.
			//This pragma suppresses build errors from uncommented files - which generated files are.
			this.codeProvider.GenerateCodeFromCompileUnit(codeCompileUnit, stringWriter, this.codeGeneratorOptions);

			// If a handler was provided for the progress, then let it know that the task is complete.
			if (pGenerateProgress != null)
				pGenerateProgress.Progress(100, 100);

			// This will pack the generated buffer into an unmanaged block of memory that can be passed back to Visual Studio.
			byte[] generatedBuffer = System.Text.Encoding.UTF8.GetBytes(stringWriter.ToString());
			rgbOutputFileContents[0] = Marshal.AllocCoTaskMem(generatedBuffer.Length);
			Marshal.Copy(generatedBuffer, 0, rgbOutputFileContents[0], generatedBuffer.Length);
			pcbOutput = (uint)generatedBuffer.Length;

			// At this point the code generation was a success.
			return VSConstants.S_OK;

		}

		#endregion

	}

}
