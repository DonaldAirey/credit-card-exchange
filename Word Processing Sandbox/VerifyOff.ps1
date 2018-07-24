if(!${PROJDIR})
{
	${PROJDIR} = "${Home}\Documents\Visual Studio 2008\Projects"
}

sn -q -Vr "${PROJDIR}\Fluid Trade\Expression Evaluation Library\bin\${OUTPUTTARGET}\Debug\FluidTrade.ExpressionEvaluationLibrary.dll"
sn -q -Vr "${PROJDIR}\Fluid Trade\Fluid Trade Library\bin\${OUTPUTTARGET}\Debug\FluidTrade.FluidTradeLibrary.dll"
sn -q -Vr "${PROJDIR}\Fluid Trade\Mail Merge\bin\${OUTPUTTARGET}\Debug\FluidTrade.MailMerge.dll"

