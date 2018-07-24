# Certificate Infrastructure for the Teraque Asset Network Demo.
$makecert = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.15063.0\x64\makecert.exe"

# This will create the certificate infrastructure for the Teraque Asset Network demo.
# Uncomment this line if there's not already a root certificate.
&$makecert -pe -n "CN=Dark Bond CA,O=Dark Bond,OU=Dark Bond Client Operations,L=Cambridge,S=MA,C=US" -ss root -sr LocalMachine -a sha256 -sky signature -r "${HOME}\My Documents\My Certificates\Dark Bond\DER\Dark Bond CA.cer"
&$makecert -pe -n "CN=cce.darkbond.com" -ss my -sr LocalMachine -a sha256 -sky exchange -eku 1.3.6.1.5.5.7.3.1 -in "Dark Bond CA" -is root -ir LocalMachine -sp "Microsoft RSA SChannel Cryptographic Provider" -sy 12 "${HOME}\My Documents\My Certificates\Dark Bond\DER\cce.darkbond.com.cer"
