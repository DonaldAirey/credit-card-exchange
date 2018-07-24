if(!${PROJDIR})
{
	${PROJDIR} = "C:\build\projects\Fluid trade\main"
}


function Usage
{
  echo "Usage: ";
  echo "  from cmd.exe: ";
  echo "     powershell.exe SetVersion.ps1  2.8.3.0";
  echo " ";
  echo "  from powershell.exe prompt: ";
  echo "     .\SetVersion.ps1  2.8.3.0";
  echo " ";
}


function Update-SourceVersion
{
  Param ([string]$Version)

  $NewVersion = 'AssemblyVersion("' + $Version + '")';
  $NewFileVersion = 'AssemblyFileVersion("' + $Version + '")';

  foreach ($o in $input) 
  {

    #### !! do Version control checkout here if necessary 
    Write-output $o.FullName
    $TmpFile = $o.FullName + ".tmp"

     get-content $o.FullName | 
        %{$_ -replace 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $NewVersion } |
        %{$_ -replace 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $NewFileVersion } | out-file $TmpFile -encoding UTF8 

     move-item $TmpFile $o.FullName -force
  }
}


function Update-AssemblyInfoFiles ($directory, $version) 
{
	cd $directory
	Update-AllAssemblyInfoFiles $version
}

function Update-AllAssemblyInfoFiles ( $version )
{
  foreach ($file in "AssemblyInfo.cs", "AssemblyInfo.vb" ) 
  {
    get-childitem -recurse |? {$_.Name -eq $file} | Update-SourceVersion $version ;
  }
}


Update-AssemblyInfoFiles "$PROJDIR\Guardian\Guardian Library\" 1.3.0.0;
Update-AssemblyInfoFiles "$PROJDIR\Fluid Trade Library" 1.3.0.0;
Update-AssemblyInfoFiles "$PROJDIR\Message Library\" 1.3.0.0;
