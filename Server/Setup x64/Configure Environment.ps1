# Calculate the project root from the invocation.
$directoryRoot = $(split-path -parent $SCRIPT:MyInvocation.MyCommand.Path)
&"$directoryRoot/Import Certificates.ps1"
&"$directoryRoot/Open Firewall.ps1"
