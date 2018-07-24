#
# Fluid-Trade tailored scripts for loading the DB on a dev system.  This script is made to be run from the 
# (staging area) framework database directory (projects\fluid trade\guardian\database\scripts)
#

if(!${PROJDIR})
{
      ${PROJDIR} = "${Home}\Documents\Visual Studio 2008\Projects"
}

# Initialize the Data Model
& '.\Load Small Data Model.ps1'  

# Bootstrap the DB with generated data 
& '.\Load Small Unit Test.ps1'  
 
 
