if(!${KEYDIR})
{
	${KEYDIR} = "${Home}\Documents\My Keys\Fluid Trade\"
}

$BUILDDIR = $args[0]
if(!$BUILDDIR)
{
	$BUILDDIR = "."
}

sn -q -R "$BUILDDIR\..\..\Fluid Trade Library\obj\Release\FluidTrade.FluidTradeLibrary.dll" "${KEYDIR}Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Fluid Trade Windows\obj\Release\FluidTrade.FluidTradeWindows.dll" "${KEYDIR}Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Guardian\Blotter Detail Report\obj\Release\FluidTrade.BlotterDetailReport.dll" "${KEYDIR}Guardian\Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Guardian\Blotter Viewer\obj\Release\FluidTrade.BlotterViewer.dll" "${KEYDIR}Guardian\Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Guardian\Debt Negotiator Negotiation Console\obj\Release\FluidTrade.DebtNegotiatorNegotiationConsole.dll" "${KEYDIR}Guardian\Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Guardian\Debt Blotter Viewer\obj\Release\FluidTrade.DebtBlotterViewer.dll" "${KEYDIR}Guardian\Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Guardian\Debt Holder Blotter Viewer\obj\Release\FluidTrade.DebtHolderBlotterViewer.dll" "${KEYDIR}Guardian\Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Guardian\Debt Holder Working Order Report\obj\Release\FluidTrade.DebtHolderWorkingOrderReport.dll" "${KEYDIR}Guardian\Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Guardian\Debt Negotiator Working Order Report\obj\Release\FluidTrade.DebtNegotiatorWorkingOrderReport.dll" "${KEYDIR}Guardian\Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Guardian\Client Data Model\obj\Release\FluidTrade.ClientDataModel.dll" "${KEYDIR}Guardian\Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Guardian\Client Trading Support\obj\Release\FluidTrade.ClientTradingSupport.dll" "${KEYDIR}Guardian\Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Guardian\Destination Order Report\obj\Release\FluidTrade.DestinationOrderReport.dll" "${KEYDIR}Guardian\Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Guardian\Equity Blotter Viewer\obj\Release\FluidTrade.EquityBlotterViewer.dll" "${KEYDIR}Guardian\Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Guardian\Equity Working Order Report\obj\Release\FluidTrade.EquityWorkingOrderReport.dll" "${KEYDIR}Guardian\Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Guardian\Execution Report\obj\Release\FluidTrade.ExecutionReport.dll" "${KEYDIR}Guardian\Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Guardian\Folder Viewer\obj\Release\FluidTrade.FolderViewer.dll" "${KEYDIR}Guardian\Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Guardian\Guardian Windows\obj\Release\FluidTrade.GuardianWindows.dll" "${KEYDIR}Guardian\Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Guardian\Guardian Library\obj\Release\FluidTrade.GuardianLibrary.dll" "${KEYDIR}Guardian\Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Guardian\Match Report\obj\Release\FluidTrade.MatchReport.dll" "${KEYDIR}Guardian\Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Message Library\obj\Release\FluidTrade.MessageLibrary.dll" "${KEYDIR}Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Guardian\Debt Holder Negotiation Console\obj\Release\FluidTrade.DebtHolderNegotiationConsole.dll" "${KEYDIR}Guardian\Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Guardian\Negotiation Service\obj\Release\FluidTrade.NegotiationService.dll" "${KEYDIR}Guardian\Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Guardian\Source Order Report\obj\Release\FluidTrade.SourceOrderReport.dll" "${KEYDIR}Guardian\Key Pair.snk"
sn -q -R "$BUILDDIR\..\..\Guardian\Working Order Report\obj\Release\FluidTrade.WorkingOrderReport.dll" "${KEYDIR}Guardian\Key Pair.snk"



#
#	in client but not in utils	
#
#sn -q -R "$BUILDDIR\..\..\Guardian\Debt Holder Match Report\obj\Release\FluidTrade.DebtHolderMatchReport.dll" "${KEYDIR}Guardian\Key Pair.snk"
#sn -q -R "$BUILDDIR\..\..\Guardian\Debt Holder Settlement Report\obj\Release\FluidTrade.DebtHolderSettlementReport.dll" "${KEYDIR}Guardian\Key Pair.snk"
#sn -q -R "$BUILDDIR\..\..\Guardian\Debt Negotiator Blotter Viewer\obj\Release\FluidTrade.DebtNegotiatorBlotterViewer.dll" "${KEYDIR}Guardian\Key Pair.snk"
#sn -q -R "$BUILDDIR\..\..\Guardian\Debt Negotiator Match Report\obj\Release\FluidTrade.DebtNegotiatorMatchReport.dll" "${KEYDIR}Guardian\Key Pair.snk"
#sn -q -R "$BUILDDIR\..\..\Guardian\Debt Negotiator Settlement Report\obj\Release\FluidTrade.DebtNegotiatorSettlementReport.dll" "${KEYDIR}Guardian\Key Pair.snk"
#sn -q -R "$BUILDDIR\..\..\Guardian\Debt Working Order Report\obj\Release\FluidTrade.DebtWorkingOrderReport.dll" "${KEYDIR}Guardian\Key Pair.snk"
#sn -q -R "$BUILDDIR\..\..\Guardian\Detail Reports\obj\Release\FluidTrade.DetailReports.dll" "${KEYDIR}Guardian\Key Pair.snk"
#sn -q -R "$BUILDDIR\..\..\Guardian\Negotiation Console\obj\Release\FluidTrade.NegotiationConsole.dll" "${KEYDIR}Guardian\Key Pair.snk"
#sn -q -R "$BUILDDIR\..\..\Guardian\Reporting\obj\Release\FluidTrade.Reporting.dll" "${KEYDIR}Guardian\Key Pair.snk"
#sn -q -R "$BUILDDIR\..\..\Guardian\Settlement Report\obj\Release\FluidTrade.SettlementReport.dll" "${KEYDIR}Guardian\Key Pair.snk"
#sn -q -R "$BUILDDIR\..\..\PDF Control\obj\Release\FluidTrade.PdfControl.dll" "${KEYDIR}Key Pair.snk"
