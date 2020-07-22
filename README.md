# MV's DotNet AMSI Client
A convenient .NET client for [Microsoft's Antimalware Scan Interface (AMSI)](https://docs.microsoft.com/en-us/windows/win32/amsi/antimalware-scan-interface-portal). 
 
AMSI is an interface standard that defines methods for file-based or stream-based content scanning for malware. AMSI is a part of Microsoft Windows OS, beginning from Windows 10 or Windows Server 2016. Operating system internally calls AMSI to dynamically evaluate a content that is about to be executed, e.g. JavaScript, PowerShell scripts.  

On other side, AMSI is implemented by major AV engines (e.g. Kaspersky, ESET, BitDefender), hence, once a third-party AV engine is installed on the computer, it can replace default Windows Defender, and AMSI effectively ends up invoking the third-party engine instead of Defender.  

This client library is a compact solution that enables calling AMSI methods conveniently, adds an option to scan a file-based content, including zip file inspection, and also, it can correlate a detection result detail (for now, only if Windows Defender is used).    

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
Invokes AMSI to scan a string (containing [EICAR dummy virus signature](https://www.eicar.org/?page_id=3950)) for malware. This method can be used to verify integration of AV engine since a call should result into a positive detection due to the EICAR signature.  

* TestEICARByteArray  
Invokes AMSI to scan a byte array (containing [EICAR dummy virus signature](https://www.eicar.org/?page_id=3950)) for malware. This method can be used to verify integration of AV engine since a call should result into a positive detection due to the EICAR signature.  

* ListDetectionEngines  
Enumerates AV engines registered in system.


## Example of Use

* File scan

```csharp
var fileToCheck = @"Q:\EmailAttachments\Quarantine\2020\April\15\photos.zip";

return new Scan.File(fileToCheck);
```

* Buffer scan with max two retries and 1 second delay

```csharp
byte[] buffer;
AMSIClientConfiguration configuration;

return new Scan(configuration, 2, TimeSpan.FromSeconds(1)).Buffer(fileToCheck);
```

* Buffer scan with session, using AMSIClient class directly 

```csharp
byte[] buffer = content;

using (var client = AMSIClient.Create())
using (var session = client.CreateSession())
   return session.ScanBuffer(buffer);
```

## Tested AV Engines



* Windows Defender (including detection detail retrieval)

* ESET NOD32 Antivirus

* BitDefender


## FAQ

### Is this an official project?
No, this is not an official client. At this moment, this the most advanced implementation of client library that makes possible to smoothly invoke AMSI methods. It is worth of stating explicitely, the project is not affined with Microsoft or any of AV engine vendors. Trademarks are owned by their respective owners, and the project is not meant to interfere with their rights.  

### What is the use of AMSI session?
The original purpose is to correlate together scanning of several data fragments. If scans are done within one session, the session information is passed to AV engine, and the engine hypothetically can use the indication to provide more efficient evaluation or better consolidation of detection results. If you want to invoke a scan of separate files or data chunks, it can make sense to skip the session. In that case, a session will be created for each call internally. 

### Why there is an additional method for scanning a file?  
AMSI as such does not define any method that allows to scan a file-based content. It makes sense since it is meant to be mostly used to scan a content that was already loaded into memory. However, file scanning is a useful feature, so this client implements it additionaly. 

### How is file scanning implemented?
AMSI as such allows to scan string or buffer (byte array) only. This library provides a method that splits a file stream into chunks and invokes scan of each data chunk, all together in one session. Also, [SharpZipLib](https://github.com/icsharpcode/SharpZipLib) is involved to provide an inspection of contents in case the file is zip or tar archive. Therefore, binary file parts are passed to AMSI just chunk by chunk. If the file is an archive, the base stream and also the content stream is passed to AMSI. This increases the scan efficiency, but obviously it is more time consuming. Also, only the first level content stream is evaluated, so in case an archive is contained in another archive, its data contents are not inspected. 

### How efficient is file scan?
Base efficiency depends on AV engine capabilities. Besides that the main degradation is caused by splitting file stream into chunks, by sending these chunks into AMSI separately. This client uses a buffer of configurable size - if a file is smaller, then no chunking is needed. But if the size is exceeded, it is split. So, it can make sense to refine this parameter according to resources available. This implementation also gives an option to limit a maximum size of file, so all files larger than threshold, are rejected. To mitigate the degradation caused by splitting, the client also generates overlap chunks. An overlap is created by taking the last half of previous chunk and the first half of the current one. This costs additional time but makes a situation when e.g. a threat signature is split and undetected less probable. Finally, if the file is an archive, also an archive content stream is passed to AMSI, so first level contents are inspected. By default the client rejects password-protected archives but, as most of features, it can be reconfigured easily.  

### How detection result retrieval works?
AMSI itself does not provide much result info about executed scan. It only gives back an interval that represents a threat level beginning from no detection to identified threat. But there is nothing like a name of threat or additional data about scanning process. However, this data are usually logged by AV engine and can be obtained specifically for each AV engine.  
Currently, the detection result retrieval is implemented for Windows Defender only. This engine records an information about scan into event log. The client therefore takes a look into the log, and based on process name and timestamp, it correlates the scan action with an existing event record. This is a simple but working approach, in case multiple scans were executed in multi-threaded manner, the data may be correlated wrongly. Detection result retrieval is enabled by default.  
