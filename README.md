# PEunion

## Crypter, binder & downloader

PEunion encrypts executables, which are decrypted at runtime and executed in-memory.

![](https://bytecode77.com/images/pages/pe-union/runpe.webp)

## Stub

Two stubs are available to choose from, both of which work in a similar way.

* **Native:** Written in assembly (FASM)
* **.NET:** Written in C#

![](https://bytecode77.com/images/pages/pe-union/stub.webp)

## Key feature overview

* Emulator detection
* Low-entropy packing scheme
* Two-layer execution architecture
* Code obfuscation
* File compression
* Binder (combine multiple files)
* Downloader
* RunPE (process hollowing)
* In-memory invocation of .NET executables
* Drop files to disk
* Melt (self-deleting stub)
* EOF support
* Specify icon, version information & manifest
* Well-designed UI
* Commandline compiler

Multiple files can be compiled into the stub. A file can either be embedded within the compiled executable, or the stub downloads the file at runtime.

Typically, an executable is decrypted and executed in-memory by the stub. If the executable is a native PE file, `RunPE` (process hollowing) is used. For .NET executables, the .NET stub uses `Invoke`. Legitimate files with no known signatures can be written to the disk.

<img src="https://bytecode77.com/images/pages/pe-union/drop.webp" width="500" /> <img src="https://bytecode77.com/images/pages/pe-union/items.webp" width="500" />

## Implementation & execution flow

Obfuscation and evasive features are fundamental to the design of PEunion and do not need further configuration. The exact implementation is fine tuned to decrease detection and is subject to change in future releases.

This graph illustrates the execution flow of the native stub decrypting and executing a PE file. The .NET stub works similarly.

![](https://bytecode77.com/images/pages/pe-union/execution-flow-light.webp)

The **fundamental concept** is that the stub **only** contains code to detect emulators and to decrypt and pass execution to the next layer. The second stage is position independent shellcode that retrieves function pointers from the PEB and handles the payload. To mitigate AV detections, only the stub requires adjustments. Stage 2 contains all the "suspicious" code that is not readable at scantime and not decrypted, if an emulator is detected.

The shellcode is encrypted using a proprietary 4-byte XOR stream cipher. To decrease entropy, the encrypted shellcode is intermingled with null-bytes at randomized offsets. Because the resulting data has no repeating patterns, it is impossible to identify this particular encoding and infer YARA rules from it. Hence, AV detection is limited to the stub itself.

## Obfuscation

Assembly code is obfuscated by nop-like instructions intermingled with the actual code, such as an increment followed by a decrement. Strings are not stored in the data section, but instead constructed on the stack using mov-opcodes.

The C# obfuscator replaces symbol names with barely distinguishable Unicode characters. Both string and integer literals are decrypted at runtime.

<img src="https://bytecode77.com/images/pages/pe-union/obfuscation.webp" height="300" />&nbsp;&nbsp;&nbsp;<img src="https://bytecode77.com/images/pages/pe-union/obfuscation-dotnet.webp" height="300" />

## Right-To-Left Override Tool

The Unicode character `U+202e` allows to create a filename that masquerades the actual extension of a file.

It is a simple renaming technique, where all characters followed by `U+202e` are displayed in reversed order. This way, an executable can be crafted in such a way that it looks like a JPEG file.

![](https://bytecode77.com/images/pages/pe-union/rtlo.webp)

## Audience

In order to use this program, you should:

* be familiar with crypters and the basic concept of what a crypter does
* have a basic understanding of in-memory execution and evasion techniques
* acknowledge that uploading the stub to VirusTotal will decrease the time that the stub remains FUD

I do not take any responsibility for anybody who uses PEunion in illegal malware campaigns. This is an educational project.

## FUD

This project is FUD on the day of release (September 2021). A crypter that is free, publicly available, and open source will not remain undetected for a long time. Adjusting the stub so it does not get detected is a daunting task and all efforts are in vain several days later. Therefore, there will be **no updates** to fix detection issues.

Rather, PEunion offers a fully functional implementation that is easy to modify and extend. If you want PEunion to be FUD, please get familiar with the code of the stub and adjust it until you are satisfied with the result.

However, additional evasion techniques may be implemented in future releases to improve the baseline design.

## Downloads

[![](https://bytecode77.com/public/fileicons/zip.png) PEunion 4.0.0.zip](https://downloads.bytecode77.com/PEunion%204.0.0.zip)
(**ZIP Password:** bytecode77)

## Project Page

[![](https://bytecode77.com/public/favicon16.png) bytecode77.com/pe-union](https://bytecode77.com/pe-union)
