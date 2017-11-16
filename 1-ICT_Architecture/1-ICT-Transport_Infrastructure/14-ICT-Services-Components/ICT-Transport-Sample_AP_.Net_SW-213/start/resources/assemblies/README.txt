The start project requires the assemblies "System.IdentityModel.dll" and "System.ServiceModel.dll" 
in version 3.0.4506.4446 or higher. These assemblies are contained in the Microsoft hotfixes 
for the various OS listed at http://support.microsoft.com/kb/974842/

if you choose to install dlls, do the following:

- Go to run and type "cmd"
- Type "cd C:\Program Files\Microsoft SDKs\Windows\v6.0A\Bin" 
- Now type "gacutil /i D:/System.IdentityModel.dll"  (/i <assembly path>)
- Do the same for the "System.ServiceModel.dll"

* When using one of these hotfixes, you do not have to 
install the assemblies from this directory.

* It is recommended to use the hotfix.

-------------------------------------------------------
Regarding "Microsoft.IdentityModel"

This assembly is used for SAML and is needed in the project,
so it is necessary to make a reference to the "STARTAccessPoint"
project is you look an exclamation symbol in the project references.

* This occurs due to the server or pc does not have "Windows Identity Foundation" 
but for this project you just need "Microsoft.IdentityModel" assembly.

* If you want to download the "Windows Identity Foundation" go to
http://www.microsoft.com/download/en/details.aspx?id=17331 but you
can find the dll in this folder.

---------------------------------------------------------
Regarding "Mono.Security"

This assembly is used for recovering detailed information of any X509 certificate.