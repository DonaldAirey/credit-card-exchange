$BUILDDIR = $args[0]
if(!$BUILDDIR)
{
	$BUILDDIR = "."
}

sn -q -Vr "$BUILDDIR\..\..\Guardian\Client Data Model\obj\${OUTPUTTARGET}\Debug\FluidTrade.ClientDataModel.dll"
sn -q -Vr "$BUILDDIR\..\..\Guardian\Client Trading Support\obj\${OUTPUTTARGET}\Debug\FluidTrade.ClientTradingSupport.dll"
sn -q -Vr "$BUILDDIR\..\..\Guardian\Guardian Windows\obj\${OUTPUTTARGET}\Debug\FluidTrade.GuardianWindows.dll"
sn -q -Vr "$BUILDDIR\..\..\Guardian\Guardian Library\obj\${OUTPUTTARGET}\Debug\FluidTrade.GuardianLibrary.dll"
sn -q -Vr "$BUILDDIR\..\..\Fluid Trade Library\obj\${OUTPUTTARGET}\Debug\FluidTrade.FluidTradeLibrary.dll"
sn -q -Vr "$BUILDDIR\..\..\Fluid Trade Windows\obj\${OUTPUTTARGET}\Debug\FluidTrade.FluidTradeWindows.dll"
