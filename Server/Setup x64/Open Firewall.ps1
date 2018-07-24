# This describes the certificate
$domain = "cce.darkbond.com"
$thumbprint = "e81309ebd155029882c0290787af6d532496fb98"

# This script must be run with administrator privileges before the service can be run in the development environment.
# Project root.
$projectRoot = $(split-path -parent $(split-path -parent $SCRIPT:MyInvocation.MyCommand.Path))

# Grant access to user the web service certificate to any authenticated user.
$certCN = "CN=$domain"
Try
{
    $WorkingCert = Get-ChildItem CERT:\LocalMachine\My | where {$_.Subject -match $certCN} | sort $_.NotAfter -Descending | select -first 1 -erroraction STOP
    $TPrint = $WorkingCert.Thumbprint
    $rsaFile = $WorkingCert.PrivateKey.CspKeyContainerInfo.UniqueKeyContainerName
}
Catch
{
    "Error: unable to locate certificate for $($CertCN)"
    Exit
}

$keyPath = "C:\ProgramData\Microsoft\Crypto\RSA\MachineKeys\"
$fullPath=$keyPath+$rsaFile
$acl=Get-Acl -Path $fullPath
$permission="Authenticated Users","Read","Allow"
$accessRule=new-object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.AddAccessRule($accessRule)
Try 
{
    Set-Acl $fullPath $acl
    "Success: ACL set on certificate"
}
Catch
{
    "Error: unable to set ACL on certificate"
    Exit
}

# Remove the previous configuration of the ports.
netsh http delete urlacl url="http://+:80/Guardian"

# To run this project in Visual Studio without Administrator privileges, grant access to the private key of the 'oms.darkbond.com' certificate to the
# current user.  In addition, this gives the current user permission to use the ports (80 and 443).  This script must be run with elevated
# (Administrator) privileges.
$user = "NT Authority\NetworkService"
netsh http add urlacl url="http://+:80/Guardian" user="$user"

# Open up the ports used by this application
netsh advfirewall firewall add rule name="Guardian (TCP-In)" protocol=TCP dir=in localport=809 action=allow
