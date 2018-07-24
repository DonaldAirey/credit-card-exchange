namespace FluidTrade.PresentationGenerator
{

    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using FluidTrade.Core;

	/// <summary>
	/// These are the parsing states used to read the arguments on the command line.
	/// </summary>
	enum ArgumentState { None, TargetNamespace, InputFileName, OutputFileName };

	/// <summary>
	/// Creates an executable wrapper around the IDE tool used to generate a middle tier.
	/// </summary>
	class PresentationCompiler
	{

		// Private Members
		private static ArgumentState argumentState;
		private static String inputFilePath;
		private static String outputFileName;
		private static String targetNamespace;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] args)
		{

			try
			{

				// Defaults
				targetNamespace = "DefaultNamespace";

				// The command line parser is driven by different states that are triggered by the flags read.  Unless a flag has
				// been read, the command line parser assumes that it's reading the file name from the command line.
				argumentState = ArgumentState.InputFileName;

				// Parse the command line for arguments.
				foreach (string argument in args)
				{

					// Decode the current argument into a state change (or some other action).
					if (argument == "-i") { argumentState = ArgumentState.InputFileName; continue; }
					if (argument == "-ns") { argumentState = ArgumentState.TargetNamespace; continue; }
					if (argument == "-out") { argumentState = ArgumentState.OutputFileName; continue; }

					// The parsing state will determine which variable is read next.
					switch (argumentState)
					{
					case ArgumentState.InputFileName:
						inputFilePath = argument;
						break;
					case ArgumentState.OutputFileName:
						outputFileName = argument;
						break;
					case ArgumentState.TargetNamespace:
						targetNamespace = argument;
						break;
					}

					// The default state is to look for the input file name on the command line.
					argumentState = ArgumentState.InputFileName;

				}

				// Expand the environment variables for the input file path.
				if (inputFilePath == null)
					throw new Exception("Usage: DataSetGenerator -i <InputFileName>");
				inputFilePath = Environment.ExpandEnvironmentVariables(inputFilePath);

				// Expand the environment variables for the outpt file path.
				if (outputFileName == null)
					outputFileName = string.Format("{0}.cs", Path.GetFileNameWithoutExtension(inputFilePath));
				outputFileName = Environment.ExpandEnvironmentVariables(outputFileName);

				// The main idea here is to emulate the interface that Visual Studio uses to generate a file.  The first step is to
				// read the schema from the input file into a string.  This emulates the way that the IDE would normally call a
				// code generator.  Then create a 'PresentationGenerator' to compile the schema.
				StreamReader streamReader = new StreamReader(inputFilePath);
				string fileContents = streamReader.ReadToEnd();
				streamReader.Close();
				IntPtr[] buffer = new IntPtr[1];
				uint bufferSize;
				PresentationGenerator presentationGenerator = new PresentationGenerator();
				presentationGenerator.Generate(inputFilePath, fileContents, targetNamespace, buffer, out bufferSize, new ProgressIndicator());

				// Once the buffer of source code is generated, it is copied back out of the unmanaged buffers and written to the
				// output file.
				byte[] outputBuffer = new byte[bufferSize];
				Marshal.Copy(buffer[0], outputBuffer, 0, Convert.ToInt32(bufferSize));
				StreamWriter streamWriter = new StreamWriter(outputFileName);
				streamWriter.Write(Encoding.UTF8.GetString(outputBuffer));
				streamWriter.Close();

			}
			catch (Exception exception)
			{

				// Dump any exceptions to the console.
				Console.WriteLine(exception.Message);

			}

			// At this point the code generated created the code-behind for the source schema successfully.
			return 0;

		}

	}

}
