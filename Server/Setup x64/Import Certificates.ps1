# Grants Access to the given certificate in the given store.
function Grant-CertificatePermissions([string]$SubjectName, [string]$CertStoreLocation)
{
    $getCert = Get-ChildItem -Recurse $CertStoreLocation | Where-Object {$_.Subject -eq $SubjectName}

    $commonCertPathStub = "\Microsoft\Crypto\RSA\MachineKeys\"
    $programData = $Env:ProgramData
    if(!$programData)
    {
        $programData = $Env:ALLUSERSPROFILE + "\Application Data"
    }
    $keypath = $programData + $commonCertPathStub

    $certHash = $getCert.PrivateKey.CspKeyContainerInfo.UniqueKeyContainerName
    $certFullPath = $keypath + $certHash
    $certAcl = Get-Acl -Path $certFullPath

	$serviceSID = New-Object System.Security.Principal.SecurityIdentifier('S-1-5-20') 
	$serviceUser = $serviceSID.Translate([System.Security.Principal.NTAccount]) 

    try
    {
        $accessRule = new-object System.Security.AccessControl.FileSystemAccessRule $serviceUser, 'FullControl', 'Allow'
        $certAcl.AddAccessRule($accessRule)
    }
    catch [System.Exception]
    {
        throw "Invalid User Id Or Permission"
    }
    Set-Acl $certFullPath $certAcl
    
}

# Import a DER Certificate
function Import-Certificate([String]$FilePath , [String]$CertStoreLocation)
{
	$pfx = new-object System.Security.Cryptography.X509Certificates.X509Certificate2
	$pfx.import($FilePath)
	$store = Get-Item $CertStoreLocation
	$store.Open('ReadWrite')
	$store.add($pfx)
	$store.Close()
}

# Import a PFX Certificate
function Import-PfxCertificate([String]$FilePath , [String]$CertStoreLocation, $Password=$null)
{
	$pfx = new-object System.Security.Cryptography.X509Certificates.X509Certificate2
	$pfx.import($FilePath, $Password, “Exportable, PersistKeySet”)
	$store = Get-Item $CertStoreLocation
	$store.Open('ReadWrite')
	$store.add($pfx)
	$store.Close()
}

# Calculate the project root from the invocation.
$certificateRoot = "C:\Program Files\Teraque\Credit Card Exchange"

# This is the common password for unlocking certificates.
$mypwd = ConvertTo-SecureString -String "Dark Bond" -Force –AsPlainText

Import-PfxCertificate –FilePath "${certificateRoot}\Certificates\Dark Bond CA.pfx" -CertStoreLocation "Cert:\LocalMachine\Root" -Password $mypwd
Import-PfxCertificate –FilePath "${certificateRoot}\Certificates\cce.darkbond.com.pfx" -CertStoreLocation "Cert:\LocalMachine\My" -Password $mypwd
Grant-CertificatePermissions -SubjectName "CN=cce.darkbond.com" -CertStoreLocation "Cert:\LocalMachine\My"
