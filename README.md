# Myrtille
Myrtille provides a simple and fast access to remote desktops, applications and SSH servers through a web browser, without any plugin, extension or configuration.

Technically, Myrtille is an HTTP(S) to RDP and SSH gateway.

## How does it work?
It works by forwarding the user inputs (keyboard, mouse, touchscreen) from a web browser to an HTTP(S) gateway, then up to an RDP (or SSH) client which maintains a session with an RDP (or SSH) server.

The display resulting, or not, of such actions is streamed back to the browser, from the rdp (or ssh) client and through the gateway.

The implementation is quite straightforward in order to maintain the best speed and stability as possible. Some optimizations, such as inputs buffering and display quality tweaking, help to mitigate with latency and bandwidth issues.

More information in the DOCUMENTATION.md file.

## Features
- HTTP(S) to RDP and SSH gateway (new in version 2.0.0!)
- Hyper-V VM direct connection
- Multifactor Authentication
- Active Directory integration (hosts management)
- Session sharing (collaborative mode)
- Start remote application from URL
- File transfer (local and roaming accounts)
- PDF Virtual Printer
- Audio support
- HTML4 and HTML5 support
- Responsive design
- clipboard synchronization
- PNG, JPEG and WEBP compression
- Realtime connection information
- On-screen/console/logfile debug information
- REST APIs (i.e.: hide connection information from the browser, tracks connections, monitor remote sessions, etc.)
- fully parameterizable

## Requirements
- Browser: any HTML4 or HTML5 browser (starting from IE6!). No extension or administrative rights required. The clipboard synchronization requires Chrome (or async clipboard API support) and HTTPS connection
- Gateway (myrtille): Windows 8.1 or Windows Server 2012 R2 or greater (IIS 8.0+, .NET 4.5+ and WCF/HTTP activation enabled)
- RDP server: any RDP enabled machine (preferably Windows Server but can also be Windows XP, 7, 8, 10 or Linux xRDP server)
- SSH server: any SSH server (tests were made using the built-in Windows 10 OpenSSH server)

## Resources
Myrtille does support multiple connections/tabs (can be disabled into web.config, read comments there).

There is no limitation about the maximal number of concurrent users beside what the rdp (or ssh) server(s) can handle (number of CALs, CPU, RAM?).

Regarding the gateway, a simple dual core CPU with 4GB RAM can handle up to 50 simultaneous sessions (about 50MB RAM by rdp client process, even less for ssh).

Each session uses about 20KB/sec bandwidth with the browser.

## Build
Microsoft Visual Studio 2017. See DOCUMENTATION.md.

## Installation
All releases here: https://github.com/cedrozor/myrtille/releases

See DOCUMENTATION.md for more details.

## Docker

From version 2.8.0, Myrtille is available as a docker image.

You can pull it from Docker Hub with the following command (use a tag for a specific version, or latest otherwise)<br/>
```
docker pull cedrozor/myrtille(:tag)
```

Run the image in detached mode (optionally provide the resulting container a network adapter able to connect your hosts)<br/>
```
docker run -d (--network="<network adapter>") cedrozor/myrtille(:tag)
```

See DOCUMENTATION.md for more details.

## Remote Desktop Services

This is the primary requirement for RDP connections. Please read DOCUMENTATION.md for more information about the RDS role and features, and how to configure it for the best experience with Myrtille.

## Usage
Once Myrtille is installed on your server, you can use it at http://myserver/myrtille. Set the rdp (or ssh) server address, user domain (if any, for rdp), name and password then click "Connect!" to log in. "Disconnect" to log out. A simplified hosts management dashboard is also available to pre-configure connections with a 1 click access.

Multifactor Authentication and Active Directory integration (Enterprise Mode) are disabled by default. Please read documentation for activation of these features.

You can connect a remote desktop and **start a program automatically from an url** (see DOCUMENTATION.md). From version 1.5.0, Myrtille does support encrypted credentials (aka "password 51" into .rdp files) so the urls can be distributed to third parties without compromising on security.

The installer provides the option to create or not a self-signed certificate for https://myserver/myrtille. Like for all self-signed certificates, you will have to add a security exception into your browser (just ignore the warning message and proceed to the website). **Usage of https is recommended** to secure your remote connection.
Of course, you can avoid that by installing a certificate provided by a trusted Certification Authority (see DOCUMENTATION.md).

If you want connection information, you can enable stat (displayed on screen or browser console). If you want debug information, you can enable debug (most traces are disabled (commented) into the .js files but can be enabled (uncommented) as needed).

You can also choose the rendering mode, HTML4 or HTML5 (HTML4 may be useful, for example, if websockets are blocked by a proxy or firewall).

On touchscreen devices, you can pop the device keyboard with the "Keyboard" button. Then enter some text and click "Send". This can be used, for example, to paste the local clipboard content and send it to the server (then it be copied from there, within the remote session).
Alternatively, you can run **osk.exe** (the Windows on screen keyboard, located into %SystemRoot%\System32) within the remote session. It can even be run automatically on session start (https://www.cybernetman.com/kb/index.cfm/fuseaction/home.viewArticles/articleId/197).

The remote clipboard content can also be retrieved locally with the "Clipboard" button (text format only).

You can upload/download file(s) to/from the user documents folder with the "Files" button. Note that it requires the rdp server to be localhost (same machine as the http server) or a domain to be specified. Not available for SSH.

You can print any document on a local or network printer by using the "Myrtille PDF" (redirected) virtual printer. Simply use the print feature of your application, then open/print the downloaded pdf.

From version 2.1.0, you can connect an Hyper-V VM directly (console session). It can be useful if remote desktop access is not enabled on the VM (i.e: Linux VMs), if the VM doesn't have a network connection (or is on a different network for security reasons, or use DHCP) or simply to be able to connect the VM during system startup or shutdown.
See [notes and limitations](https://github.com/cedrozor/myrtille/blob/master/DOCUMENTATION.md#notes-and-limitations) for information to connect an Hyper-V VM and the differences with a standard RDP connection.

## Third-party
Myrtille uses the following licensed software:
- RDP client: FreeRDP (https://github.com/FreeRDP/FreeRDP), licensed under Apache 2.0 license. Myrtille uses a fork of FreeRDP (https://github.com/cedrozor/FreeRDP), to enforce a loose coupling architecture and always use the latest version of FreeRDP (the fork is periodically synchronized with the FreeRDP master branch).
- OpenSSL toolkit 1.0.2n (https://github.com/openssl/openssl), licensed under BSD-style Open Source licenses. Precompiled versions of OpenSSL can be obtained here: https://wiki.openssl.org/index.php/Binaries.
- WebP encoding: libWebP 0.5.1 (https://developers.google.com/speed/webp/), licensed under BSD-style Open Source license. Copyright (c) 2010, Google Inc. All rights reserved.
- HTML5 websockets: Microsoft.WebSockets 0.2.3.1 (https://www.nuget.org/packages/Microsoft.WebSockets/0.2.3.1), licensed under MIT license. Copyright (c) Microsoft 2012.
- Logging: Log4net 2.0.8 (https://logging.apache.org/log4net/), licensed under Apache 2.0 license.
- Multifactor Authentication: OASIS.Integration 1.6.1 (https://www.nuget.org/packages/OASIS.Integration/1.6.1), licensed under Apache 2.0 license. Source code available at https://github.com/OliveInnovations/OASIS. Copyright Olive Innovations Ltd 2017.
- PDF Virtual Printer: PdfScribe 1.0.5 (https://github.com/stchan/PdfScribe), licensed under LGPL v3 license.
- Redirection Port Monitor: RedMon 1.9 (http://pages.cs.wisc.edu/~ghost/redmon/index.htm), licensed under GPL v3 license.
- Postscript Printer Drivers: Microsoft Postscript Printer Driver V3 (https://docs.microsoft.com/en-us/windows-hardware/drivers/print/microsoft-postscript-printer-driver), copyright (c) Microsoft Corporation. All rights reserved.
- Postscript and PDF interpreter/renderer: Ghostscript 9.23 (https://www.ghostscript.com/download/gsdnld.html), licensed under AGPL v3 license.
- SSH client: SSH.NET 2016.1.0 (https://github.com/sshnet/SSH.NET/), licensed under MIT License.
- HTML Terminal Emulator: xtermjs (https://github.com/xtermjs/xterm.js/), licensed under MIT License.
- WAV audio support: NAudio (https://github.com/naudio/NAudio), licensed under Microsoft Public License (MS-PL).
- MP3 audio support: NAudio.Lame (https://github.com/Corey-M/NAudio.Lame), licensed under MIT License.
- MP3 audio support: Lame (http://lame.sourceforge.net/), licensed under LGPL license.
- Remote Desktop Services API wrapper: Cassia (https://github.com/danports/cassia), licensed under MIT License.

See DISCLAIMERS.md file.

The Myrtille code in FreeRDP is surrounded by region tags "#pragma region Myrtille" and "#pragma endregion".

libWebP are official Google's WebP precompiled binaries, and are left unmodified.

## License
Myrtille is licensed under Apache 2.0 license.
See LICENSE file.

## Author
Cedric Coste.
- Website:  https://www.cedric-coste.com
- LinkedIn:	https://fr.linkedin.com/in/cedric-coste-a1b9194b
- Twitter:	https://twitter.com/cedrozor
- Google+:	https://plus.google.com/111659262235573941837
- Facebook:	https://www.facebook.com/profile.php?id=100011710352840

## Contributors
- Catalin Trifanescu (AppliKr developer: application server. Steemind cofounder)
- Fabien Janvier (AppliKr developer: website css, clipping algorithm, websocket server)
- UltraSam (AppliKr developer: rdp client, http gateway)
- Paul Oliver (Olive Innovations Ltd: MFA, enterprise mode, SSH terminal)

## Sponsors

- Blackfish Software (http://www.blackfishsoftware.com/) - the makers of IE Tab - swipe on touchscreen devices
- ElasticServer (http://www.elasticserver.co/) - print a remote document using the browser print dialog
- Coduct GmbH (https://www.coduct.com/) - reconnect on browser resize, keeping the display aspect ratio
- Practice-Labs (https://practice-labs.com/) - audio support, REST APIs, improved iframes integration
- Schleupen AG (https://www.schleupen.de/) - clipboard synchronization, disconnect API, drain of disconnected sessions
- Microarea SpA (https://www.microarea.it/) - application pool API, reduced memory usage
- Your company here (contact me!)

## Fun

Ever wanted to run Myrtille in your Tesla supercar? :) https://www.youtube.com/watch?v=YwNlf6Bm_so

## Links
- Website:	https://www.myrtille.io (support & consulting services)
- Sources:	https://github.com/cedrozor/myrtille
- Tracker:	https://github.com/cedrozor/myrtille/issues
- Wiki:		https://github.com/cedrozor/myrtille/wiki
- Forum:	https://groups.google.com/forum/#!forum/myrtille_rdp (community)
- Donate:	https://www.paypal.me/costecedric