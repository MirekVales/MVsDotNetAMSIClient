# MV's DotNet AMSI Client
A convenient .NET client for [Microsoft's Antimalware Scan Interface (AMSI)](https://docs.microsoft.com/en-us/windows/win32/amsi/antimalware-scan-interface-portal). It allows to invoke installed antivirus engine to scan content for malware. 

## Available as NuGet Package

```
PM> Install-Package MVsDotNetAMSIClient
```

## Essential Methods

* ScanBuffer  
Invokes AMSI to scan a fileless byte array for malware.  

* ScanString  
Invokes AMSI to scan a fileless string for malware.  

* ScanFile  
Invokes AMSI to scan a file-based content. Internally, a file is split into chunks and several byte arrays are scanned 

## Example of Use

```csharp
var fileToCheck = @"Q:\EmailAttachments\Quarantine\2020\April\15\photos.zip";

using (var client = AMSIClient.Create())
    return client.ScanFile(fileToCheck);
```
