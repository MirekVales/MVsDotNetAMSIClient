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
Invokes AMSI to scan a file-based content. Internally, a file is split into chunks and several byte arrays are scanned.  

* TestEICARString  
Invokes AMSI to scan a string (containing [EICAR signuature](https://www.eicar.org)) for malware. This method can be used to verify integration of AV engine since a call should result into a positive detection due to the EICAR signature.  

* TestEICARString  
Invokes AMSI to scan a byte array (containing [EICAR signuature](https://www.eicar.org)) for malware. This method can be used to verify integration of AV engine since a call should result into a positive detection due to the EICAR signature.  

* ListDetectionEngines  
Enumerates AV engines registered in system.


## Example of Use

* Sessionless file scan

```csharp
var fileToCheck = @"Q:\EmailAttachments\Quarantine\2020\April\15\photos.zip";

using (var client = AMSIClient.Create())
    return client.ScanFile(fileToCheck);
```
* Buffer scan with session

```csharp
byte[] buffer = content;

using (var client = AMSIClient.Create())
using (var session = client.CreateSession())
   return client.ScanBuffer(buffer);
```

## Tested AV Engines



* Windows Defender (including detection detail retrieval)

* ESET NOD32 Antivirus

* BitDefender
