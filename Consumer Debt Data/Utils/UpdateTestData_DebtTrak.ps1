#
# DebtTrak tailored scripts for loading the DB on a dev system.  This script is made to be run from the 
# framework database directory (projects\fluid trade\guardian\database\scripts)
#
# Note: Most of the work in here is dedicated to getting the DebtTrack version of the files over to this 
#       directory/Staging area. Once we have a dedicated DebtTrak branch, this file will probably end up 
#		being just like the _FluidTrade version 

if(!${PROJDIR})
{
      ${PROJDIR} = "${Home}\Documents\Visual Studio 2008\Projects"
}

# Make sure the files in the destination('staging area') directories are writable
set-itemproperty -path "..\Data\*.*" -name attributes -value ([System.IO.FileAttributes]::Normal)
set-itemproperty -path "..\Unit Test\*.*" -name attributes -value ([System.IO.FileAttributes]::Normal)
set-itemproperty -path "..\Scripts\*.*" -name attributes -value ([System.IO.FileAttributes]::Normal)
set-itemproperty -path "..\Templates\*.*" -name attributes -value ([System.IO.FileAttributes]::Normal)


# Overlay the DebtTrak specific database directory files over to the 'staging area'
copy-item "${PROJDIR}\DebtTrak\Guardian\Database\Data\*.*" "..\Data" -force -verbose
copy-item "${PROJDIR}\DebtTrak\Guardian\Database\Unit Test\*.*" "..\Unit Test" -force -verbose
copy-item "${PROJDIR}\DebtTrak\Guardian\Database\Scripts\*.*" "..\Scripts" -force –verbose
copy-item "${PROJDIR}\DebtTrak\Guardian\Database\Templates\*.*" "..\Templates" -force –verbose


# Initialize the Data Model
& '.\Load DebtTrak Data Model.ps1'  

# Bootstrap the DB with generated data 
& '.\Load DebtTrak Data.ps1'


 
