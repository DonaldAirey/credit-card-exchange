# ####
# Pushes the Data created from the Consumer Debt Data Generation app out to 
# the Unit Test/Production directories for all exchanges/branches
# ####

if(!${PROJDIR})
{
      ${PROJDIR} = "${Home}\Documents\Visual Studio 2008\Projects"
}

#
# DebtTrak
#
set-itemproperty -path "${PROJDIR}\DebtTrak\Guardian\Database\Data\*.*" -name attributes -value ([System.IO.FileAttributes]::Normal)
set-itemproperty -path "${PROJDIR}\DebtTrak\Guardian\Database\Unit Test\*.*" -name attributes -value ([System.IO.FileAttributes]::Normal)
set-itemproperty -path "${PROJDIR}\DebtTrak\Guardian\Database\Unit Test\Production\*.*" -name attributes -value ([System.IO.FileAttributes]::Normal)

copy-item "${PROJDIR}\Consumer Debt Data\Generated Data\Credit Card Issuer.xml" "${PROJDIR}\DebtTrak\Guardian\Database\Data\Credit Card Issuer.xml" -force -verbose

copy-item "${PROJDIR}\Consumer Debt Data\Generated Data\*.*" "${PROJDIR}\DebtTrak\Guardian\Database\Unit Test" -force -verbose
copy-item "${PROJDIR}\Consumer Debt Data\Generated Data\*.*" "${PROJDIR}\DebtTrak\Guardian\Database\Unit Test\Production" -force -verbose

#
# Fluid Trade
#
set-itemproperty -path "${PROJDIR}\Fluid Trade\Guardian\Database\Data\*.*" -name attributes -value ([System.IO.FileAttributes]::Normal)
set-itemproperty -path "${PROJDIR}\Fluid Trade\Guardian\Database\Unit Test\*.*" -name attributes -value ([System.IO.FileAttributes]::Normal)
set-itemproperty -path "${PROJDIR}\Fluid Trade\Guardian\Database\Unit Test\Production\*.*" -name attributes -value ([System.IO.FileAttributes]::Normal)

copy-item "${PROJDIR}\Consumer Debt Data\Generated Data\Credit Card Issuer.xml" "${PROJDIR}\Fluid Trade\Guardian\Database\Data\Credit Card Issuer.xml" -force -verbose

copy-item "${PROJDIR}\Consumer Debt Data\Generated Data\*.*" "${PROJDIR}\Fluid Trade\Guardian\Database\Unit Test" -force -verbose
copy-item "${PROJDIR}\Consumer Debt Data\Generated Data\*.*" "${PROJDIR}\Fluid Trade\Guardian\Database\Unit Test\Production" -force -verbose 


