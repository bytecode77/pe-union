# PEunion

PEunion bundles multiple executables *(or any other file type)* into a single
file. Each file can be configured individually to be compressed, encrypted, etc.
In addition, an URL can be provided for a download to be executed.

The resulting binary is compiled from dynamically generated C# code. No
resources are exposed that can be harvested using tools like
[Resource Hacker](http://www.angusj.com/resourcehacker/). PEunion does not use
managed resources either. Files are stored in `byte[]` code definitions and when
encryption and compression is applied, files become as obscure as they can get.

And on top of that, obfuscation is applied to a maximal extent! Variable names
are obfuscated using barely distinguishable Unicode characters. String literals
for both strings that you provide, as well as constant string literals are
encrypted.

PEunion can be either used as a binder for multiple files, as a crypter for a
single file, or as a downloader.

## Features

This is the application interface. First, you add the files to your project.

Each file can be configured individually. Default settings already include
obfuscation, compression and encryption. Relevant settings are primarily: Where
to drop the file, using what name and whether or not to execute it and so on...

The project can be saved into a .peu file, which includes all project
information. Paths to your files are relative if they are located in the same
directory or a sub directory.

[![](https://bytecode77.com/images/sites/hacking/tools/peunion/001.png)](https://bytecode77.com/images/sites/hacking/tools/peunion/001.png)

PEunion can also be used as a downloader. Simply specify a URL and provide
drop & execution parameters. Of course, bundled files and URL downloads can be
mixed in any constellation.

[![](https://bytecode77.com/images/sites/hacking/tools/peunion/002.png)](https://bytecode77.com/images/sites/hacking/tools/peunion/002.png)

### Settings

For the C# code that is generated, compiler settings can be configured here.
Usually, you will be looking to change the icon and assembly info:

[![](https://bytecode77.com/images/sites/hacking/tools/peunion/003.png)](https://bytecode77.com/images/sites/hacking/tools/peunion/003.png)

The next two pages include settings for obfuscation and startup parameters.
Default obfuscation settings are at maximum, however they can be changed, if
required.

[![](https://bytecode77.com/cache/thumbs/?path=images/sites/hacking/tools/peunion/003.png&height=250)](https://bytecode77.com/images/sites/hacking/tools/peunion/003.png)
[![](https://bytecode77.com/cache/thumbs/?path=images/sites/hacking/tools/peunion/004.png&height=250)](https://bytecode77.com/images/sites/hacking/tools/peunion/004.png)

### Compiling

Finally, the project is compiled into a single executable file. In addition,
generating just the code will compile the .cs file, but not the binary.

[![](https://bytecode77.com/images/sites/hacking/tools/peunion/006.png)](https://bytecode77.com/images/sites/hacking/tools/peunion/006.png)

And any errors that creep in will either prevent building or display a warning:

[![](https://bytecode77.com/images/sites/hacking/tools/peunion/007.png)](https://bytecode77.com/images/sites/hacking/tools/peunion/007.png)

## Additional Tools

There are additional tools and utilities. Currently, there is only one, however
more will follow, such as an exe to docx "converter", etc.

### Right to Left Override

A lesser-known ~~bug~~ feature: Right to left override. By using the `U+202e`
unicode character, file name strings can be reversed, yielding additional
obscurity.

Example: `Colorful A[U+202E]gpj.scr` will be displayed as `Colorful Arcs.jpg` in
File Explorer. Since "scr" (for screensaver) easily goes unseen, it may be
superior over "exe". With the matching icon applied, the file may look just like
an image or document file:

[![](https://bytecode77.com/images/sites/hacking/tools/peunion/008.png)](https://bytecode77.com/images/sites/hacking/tools/peunion/008.png)

## Behind the scenes - Obfuscation!

Starting here, an array with all the files is declared. This is the definition
of all files, what to do with them and the `byte[]` literal contains the
encrypted and compressed file:

[![](https://bytecode77.com/images/sites/hacking/tools/peunion/code1.png)](https://bytecode77.com/images/sites/hacking/tools/peunion/code1.png)

Symbol names for variables, methods and classes are obfuscated using barely
readable characters. This is the difference:

[![](https://bytecode77.com/images/sites/hacking/tools/peunion/code2.gif)](https://bytecode77.com/images/sites/hacking/tools/peunion/code2.gif)

Some variables don't require obfuscation. This is because the C# compiler
doesn't assign names to variables scoped inside a method. When decompiled,
variables will look like str1, str2, str3...

### And String Encryption!

But wait! What is this orange text `"DecryptString(...)"`?

String literals are encrypted using a simple 8-bit XOR. This increases reverse
engineering effort even further. Take a look at this very simple line of code:

[![](https://bytecode77.com/images/sites/hacking/tools/peunion/code3.png)](https://bytecode77.com/images/sites/hacking/tools/peunion/code3.png)

In addition to the "runas" boolean variable being obfuscated, the string literal "runas" is encrypted, too.

[![](https://bytecode77.com/images/sites/hacking/tools/peunion/code4.png)](https://bytecode77.com/images/sites/hacking/tools/peunion/code4.png)

Sophisticated reverse engineers will quickly assume that this means
`ProcessStartInfo.Verb = "runas"`. However, considering the amount of code that
is generated, with absolutely meaningless variable names, and no visible strings
at all - analyzing this binary will become a mission! And to anyone unfamiliar,
a file like this is completely incomprehensive.

And in fact, decompilation will require *some* effort to figure out the payloads
of the binary. Needless to say, that this is no "protection" of the content,
which can be still decrypted by debugging.

[![](https://bytecode77.com/images/sites/hacking/tools/peunion/code5.png)](https://bytecode77.com/images/sites/hacking/tools/peunion/code5.png)

## Downloads

[![](https://bytecode77.com/images/shared/fileicons/zip.png) PEunion 3.1.4 Binaries.zip](https://bytecode77.com/downloads/hacking/tools/PEunion%203.1.4%20Binaries.zip)

## Project Page

[![](https://bytecode77.com/images/shared/favicon16.png) bytecode77.com/hacking/tools/peunion](https://bytecode77.com/hacking/tools/peunion)