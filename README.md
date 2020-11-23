# PEunion

## Binder, downloader & crypter

PEunion bundles executables into a single file. Each executable can be configured individually to be compressed, encrypted, etc. In addition, an URL can be specified to download and execute files from the web.

The resulting binary is compiled from dynamically generated C# code. No resources are exposed that can be harvested using tools like [Resource Hacker](http://www.angusj.com/resourcehacker/). PEunion does not use managed resources either. Files are stored in byte[] literals and when encryption and compression is applied, files become as obscure as they can get.

On top of that, obfuscation is maximized during code generation. Variable names are obfuscated using barely distinguishable Unicode characters. String literals from both user provided strings, as well as constant string literals are encrypted.

PEunion can be either used as a binder for multiple files, as a crypter for a single file, or as a downloader.

![](https://bytecode77.com/images/pages/pe-union/001.png)

In the application interface, files can be added to a project. Each file can be configured individually. By default, obfuscation, compression, and encryption are enabled.

The project can be saved into a .peu file, which includes all project information. Paths to your files are relative if they are located in the same directory, or a sub directory.

PEunion can also be used as a downloader. Simply specify a URL and provide drop & execution parameters. Of course, bundled files and URL downloads can be mixed in any constellation.

![](https://bytecode77.com/images/pages/pe-union/002.png)

## Settings

For the C# code that is generated, compiler settings can be configured here. Usually, you will be looking to change the icon and assembly info:

![](https://bytecode77.com/images/pages/pe-union/003.png)

The next two pages include settings for obfuscation and startup parameters. Default obfuscation settings are at maximum, however, they can be changed, if required.

![](https://bytecode77.com/images/pages/pe-union/004.png)
![](https://bytecode77.com/images/pages/pe-union/005.png)

## Compiling

Finally, the project is compiled into a single executable file:

![](https://bytecode77.com/images/pages/pe-union/006.png)

Any errors in project configuration or the build process are displayed in a comprehensive way:

![](https://bytecode77.com/images/pages/pe-union/007.png)

## Additional Tools

There is a menu for additional tools and utilities. Currently, there is only one, however, tools such as an "exe-to-docx-converter", etc. are on the agenda.

### Right to Left Override

A lesser-known ~~bug~~ feature: Right to left override. By using the `U+202e` Unicode character, strings in file names can be reversed, yielding additional obscurity.

Example: `Colorful A[U+202E]gpj.scr` will be displayed as `Colorful Arcs.jpg` in File Explorer. The "scr" (screensaver) file extension is easily overlooked. And with a matching icon, the file may look just like a harmless jpeg image:

![](https://bytecode77.com/images/pages/pe-union/008.png)

## Behind the scenes - Obfuscation

Starting here, an array with the included files is declared. This is the definition for the files and what to do with them. The byte[] literal contains the encrypted and compressed file:

![](https://bytecode77.com/images/pages/pe-union/code1.png)

Symbols for variables, methods and classes are obfuscated using barely readable characters:

![](https://bytecode77.com/images/pages/pe-union/code2.gif)

Some variables don't require obfuscation. This is because the C# compiler doesn't assign names to variables scoped inside a method. When decompiled, variables will look like str1, str2, str3...

### String Encryption

Wondered, what "DecryptString(...)" means?

String literals are encrypted using a simple 8-bit XOR. This increases reverse engineering effort even more. Take a look at this very simple line of code:

![](https://bytecode77.com/images/pages/pe-union/code3.png)

In addition to the "runas" boolean variable being obfuscated, the string literal "runas" is encrypted, too.

![](https://bytecode77.com/images/pages/pe-union/code4.png)

Skilled reverse engineers will quickly assume that this *maybe* means `ProcessStartInfo.Verb = "runas"`. However, considering the amount of code that is generated, with meaningless and unreadable variable names, and no visible strings at all - analyzing this binary isn't particularly pleasing. And to anyone unfamiliar, a file like this is completely incomprehensive.

And in fact, decompilation will require *some* effort to figure out the payloads of the binary. Needless to say, obfuscation does not equal "protection" of the content. It just shouldn't be too easy.

![](https://bytecode77.com/images/pages/pe-union/code5.png)

## Downloads

[![](http://bytecode77.com/public/fileicons/zip.png) PEunion 3.1.5.zip](https://bytecode77.com/downloads/PEunion%203.1.5.zip)

## Project Page

[![](https://bytecode77.com/public/favicon16.png) bytecode77.com/peunion](https://bytecode77.com/peunion)