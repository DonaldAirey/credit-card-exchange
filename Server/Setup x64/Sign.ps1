# Calculate the project root from the invocation.
${projectRoot} = $(split-path -parent $(split-path -parent $(split-path -parent $SCRIPT:MyInvocation.MyCommand.Path)))
$keyRoot = "C:\Source\Product Keys"
$sn64 = "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\x64\sn.exe"

# Sign the partially signed assemblies for the web service.
&"${sn64}" -q -R "${projectRoot}\Guardian\Web Service Host\bin\Release\FluidTrade.FixLibrary.dll" "${keyRoot}\Key Pair.snk"
&"${sn64}" -q -R "${projectRoot}\Guardian\Web Service Host\bin\Release\FluidTrade.AdminSupport.dll" "${keyRoot}\Key Pair.snk"
&"${sn64}" -q -R "${projectRoot}\Guardian\Web Service Host\bin\Release\FluidTrade.GuardianLibrary.dll" "${keyRoot}\Key Pair.snk"
&"${sn64}" -q -R "${projectRoot}\Guardian\Web Service Host\bin\Release\FluidTrade.FluidTradeLibrary.dll" "${keyRoot}\Key Pair.snk"
&"${sn64}" -q -R "${projectRoot}\Guardian\Web Service Host\bin\Release\FluidTrade.MessageLibrary.dll" "${keyRoot}\Key Pair.snk"
&"${sn64}" -q -R "${projectRoot}\Guardian\Web Service Host\bin\Release\FluidTrade.ServerDataModel.dll" "${keyRoot}\Key Pair.snk"
&"${sn64}" -q -R "${projectRoot}\Guardian\Web Service Host\bin\Release\FluidTrade.TradingSupport.dll" "${keyRoot}\Key Pair.snk"
&"${sn64}" -q -R "${projectRoot}\Guardian\Web Service Host\bin\Release\FluidTrade.SettlementSupport.dll" "${keyRoot}\Key Pair.snk"
&"${sn64}" -q -R "${projectRoot}\Guardian\Web Service Host\bin\Release\FluidTrade.WebServiceHost.exe" "${keyRoot}\Key Pair.snk"
&"${sn64}" -q -R "${ProjectRoot}\Guardian\Web Service Host\bin\Release\FluidTrade.ExpressionEvaluationLibrary.dll" "${keyRoot}\Key Pair.snk"
&"${sn64}" -q -R "${ProjectRoot}\Guardian\Web Service Host\bin\Release\FluidTrade.MailMerge.dll" "${keyRoot}\Key Pair.snk"

# Sign the partially signed assemblies for the script loader.
&"${sn64}" -q -R "${projectRoot}\Guardian\Script Loader\bin\Release\FluidTrade.ClientDataModel.dll" "${keyRoot}\Key Pair.snk"
&"${sn64}" -q -R "${projectRoot}\Guardian\Script Loader\bin\Release\FluidTrade.FluidTradeLibrary.dll" "${keyRoot}\Key Pair.snk"
&"${sn64}" -q -R "${projectRoot}\Guardian\Script Loader\bin\Release\FluidTrade.GuardianLibrary.dll" "${keyRoot}\Key Pair.snk"
&"${sn64}" -q -R "${projectRoot}\Guardian\Script Loader\bin\Release\FluidTrade.MessageLibrary.dll" "${keyRoot}\Key Pair.snk"
&"${sn64}" -q -R "${projectRoot}\Guardian\Script Loader\bin\Release\FluidTrade.ServerDataModel.dll" "${keyRoot}\Key Pair.snk"
&"${sn64}" -q -R "${projectRoot}\Guardian\Script Loader\bin\Release\FluidTrade.TradingSupport.dll" "${keyRoot}\Key Pair.snk"

