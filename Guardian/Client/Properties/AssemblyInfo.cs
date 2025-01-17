﻿using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("DebtTrak Client")]
[assembly: AssemblyDescription("An general purpose tool to explore financial information and handle trading tasks.")]
[assembly: AssemblyCompany("DebtTrak")]
[assembly: AssemblyProduct("DebtTrak Client")]
[assembly: AssemblyCopyright("Copyright @  2009")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]

//In order to begin building localizable applications, set 
//<UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
//inside a <PropertyGroup>.  For example, if you are using US english
//in your source files, set the <UICulture> to en-US.  Then uncomment
//the NeutralResourceLanguage attribute below.  Update the "en-US" in
//the line below to match the UICulture setting in the project file.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
	ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
	//(used if a resource is not found in the page, 
	// or application resource dictionaries)
	ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
	//(used if a resource is not found in the page, 
	// app, or any theme specific resource dictionaries)
)]

// This provides the mapping of the XML namespace to the CLR namespace used in this assembly.
[assembly: XmlnsDefinition("http://schemas.fluidtrade.com/winfx/2009/xaml/fludtradeclient/Core", "FluidTrade.Guardian")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: AssemblyVersion("1.3.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
