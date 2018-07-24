# Calculate the project root from the invocation.
$projectRoot = $(split-path -parent $(split-path -parent $SCRIPT:MyInvocation.MyCommand.Path))
$sn64 = "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\x64\sn.exe"
$sn32 = "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\sn.exe"

# Verify off for 64 bit assemblies.
&"${sn64}" -q -Vr "${projectRoot}\Fix Library\bin\Debug\FluidTrade.FixLibrary.dll"
&"${sn64}" -q -Vr "${projectRoot}\Guardian\Admin Support\bin\Debug\FluidTrade.AdminSupport.dll"
&"${sn64}" -q -Vr "${projectRoot}\Guardian\Client Data Model\bin\Debug\FluidTrade.ClientDataModel.dll"
&"${sn64}" -q -Vr "${projectRoot}\Guardian\Guardian Library\bin\Debug\FluidTrade.GuardianLibrary.dll"
&"${sn64}" -q -Vr "${projectRoot}\Fluid Trade Library\bin\Debug\FluidTrade.FluidTradeLibrary.dll"
&"${sn64}" -q -Vr "${projectRoot}\Message Library\bin\Debug\FluidTrade.MessageLibrary.dll"
&"${sn64}" -q -Vr "${projectRoot}\Guardian\Server Data Model\bin\Debug\FluidTrade.ServerDataModel.dll"
&"${sn64}" -q -Vr "${projectRoot}\Guardian\Trading Support\bin\Debug\FluidTrade.TradingSupport.dll"
&"${sn64}" -q -Vr "${projectRoot}\Guardian\Settlement Support\bin\Debug\FluidTrade.SettlementSupport.dll"
&"${sn64}" -q -Vr "${projectRoot}\Guardian\Web Service Host\bin\Debug\FluidTrade.WebServiceHost.exe"
&"${sn64}" -q -Vr "${ProjectRoot}\Expression Evaluation Library\obj\Debug\FluidTrade.ExpressionEvaluationLibrary.dll"
&"${sn64}" -q -Vr "${ProjectRoot}\Fluid Trade MailMerge\obj\Debug\FluidTrade.MailMerge.dll"

# Verify off for 32 bit assemblies.
&"${sn32}" -q -Vr "${projectRoot}\Fix Library\bin\Debug\FluidTrade.FixLibrary.dll"
&"${sn32}" -q -Vr "${projectRoot}\Guardian\Admin Support\bin\Debug\FluidTrade.AdminSupport.dll"
&"${sn32}" -q -Vr "${projectRoot}\Guardian\Client Data Model\bin\Debug\FluidTrade.ClientDataModel.dll"
&"${sn32}" -q -Vr "${projectRoot}\Guardian\Guardian Library\bin\Debug\FluidTrade.GuardianLibrary.dll"
&"${sn32}" -q -Vr "${projectRoot}\Fluid Trade Library\bin\Debug\FluidTrade.FluidTradeLibrary.dll"
&"${sn32}" -q -Vr "${projectRoot}\Message Library\bin\Debug\FluidTrade.MessageLibrary.dll"
&"${sn32}" -q -Vr "${projectRoot}\Guardian\Server Data Model\bin\Debug\FluidTrade.ServerDataModel.dll"
&"${sn32}" -q -Vr "${projectRoot}\Guardian\Trading Support\bin\Debug\FluidTrade.TradingSupport.dll"
&"${sn32}" -q -Vr "${projectRoot}\Guardian\Settlement Support\bin\Debug\FluidTrade.SettlementSupport.dll"
&"${sn32}" -q -Vr "${projectRoot}\Guardian\Web Service Host\bin\Debug\FluidTrade.WebServiceHost.exe"
&"${sn32}" -q -Vr "${projectRoot}\Additional Server Libraries\FluidTrade.MailMerge.dll"
&"${sn32}" -q -Vr "${ProjectRoot}\Expression Evaluation Library\obj\Debug\FluidTrade.ExpressionEvaluationLibrary.dll"
&"${sn32}" -q -Vr "${ProjectRoot}\Fluid Trade MailMerge\obj\Debug\FluidTrade.MailMerge.dll"