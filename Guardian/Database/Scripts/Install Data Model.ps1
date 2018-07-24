# Calculate the project root from the invocation.
$server = "."
$databaseRoot = $(split-path -parent $(split-path -parent $SCRIPT:MyInvocation.MyCommand.Path))
$sqlcmd = "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\130\Tools\Binn\sqlcmd.exe"

# Drop the previous schema and install the new one.
write-host "Installing schemas for" $args[0]
&"${sqlcmd}" -S ${server} -E -d "Guardian" -i "${databaseRoot}\Scripts\Drop All.sql"
&"${sqlcmd}" -S ${server} -E -d "Guardian" -i "${databaseRoot}\Scripts\DataModel.sql"