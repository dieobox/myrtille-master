- [Introduction](#introduction)
- [History](#history)
- [Installation](#installation)
- [Docker](#docker)
- [Remote Desktop Services](#remote-desktop-services)
- [Auto-connect / Start remote application from URL](#auto-connect--start-remote-application-from-url)
- [Syntax](#syntax)
- [Password Hash](#password-hash)
- [File transfer](#file-transfer)
- [Print document](#print-document)
- [Security](#security)
- [Configuration / Performance tweaks / Debug](#configuration--performance-tweaks--debug)
- [Code organization](#code-organization)
- [Build](#build)
- [Startup projects](#startup-projects)
- [Communication](#communication)
- [Overview](#overview)
- [Protocols](#protocols)
- [Multifactor Authentication](#multifactor-authentication)
- [Enterprise Mode](#enterprise-mode)
- [REST APIs](#rest-apis)
- [Notes and limitations](#notes-and-limitations)
- [Troubleshoot](#troubleshoot)

# Introduction
I worked hard to make Myrtille as straightforward as possible, with commented code, but some points may needs additional information.

If you have any issue, question or suggestion that isn't addressed into this synthetic documentation, FAQ or Wiki, please don't hesitate to contact me (cedrozor@gmail.com) or ask the community (https://groups.google.com/forum/#!forum/myrtille_rdp).

## History
Myrtille started back in 2007 as a PoC, with coworkers on our spare time, under the name AppliKr. I (Cedric Coste) was in charge of the websites (admin and frontal), business and database layers. The objective was to demonstrate it was possible to virtualize desktops and applications into a web browser only using native web technologies (no plugin). HTML5 was in early stage and it was a real challenge to do it only with HTML4. We were pioneers on that matter, along with Guacamole (HTML5/VNC), and wanted to take part on the emerging SaaS market.

It was quite a success because, at that time, the zero plugin concept was innovative and unlike solutions using Java, activeX or Flex; the ease of use and security aspects were also improved. But it was also slow and buggy and was left aside.

In 2011, I had the opportunity to create a company, named Steemind, with Catalin Trifanescu (a former coworker). I rewrote everything and used FreeRDP as rdp client (AppliKr was using RDesktop) and speed and stability were way better. The company failed in 2012 because we weren't able to achieve a decent fund raising and our customers were moving to Citrix and other major corporate companies in the VDI business.

I had recently some spare time and, while I was taking my breakfast with some blueberry jam, I decided to extract and improve the Steemind core technology and open source it.
	
I hope you will enjoy Myrtille! :)

Special thanks to Catalin Trifanescu for its support.

## Installation
Starting from version 2.8.0, the Myrtille requirements change: Windows 8.1 or Windows Server 2012 R2 or greater. This is partly because of the automated installation of the roles and features required by myrtille (which requires a modern powershell) and partly to deliver the best user experience possible (fast display and audio support), as HTML5 websockets (supported by all browsers nowadays) are only available in IIS 8.0+ (Windows 8.1 or Windows Server 2012 R2 or greater).

That said, Myrtille will continue to fallback to HTML4 (xhr and long-polling) if websockets are not enabled into IIS (it's optional) or if the websocket traffic is blocked by any network equipment.

The .NET 4.5+ framework can be installed automatically by the myrtille installer, enabled as a feature of IIS (Web Server role > Applications Development > ASP.NET 4.5 on Windows Server 2012) or installed separately (https://www.microsoft.com/en-us/download/details.aspx?id=30653).

Into the roles and features management, ensure you have enabled **HTTP Activation** under ".NET Framework 4.5+ Features" > "WCF Services" (required by the Myrtille services and REST API).

The installer does install myrtille under the IIS default website and creates a custom application pool ("MyrtilleAppPool"). If you want to use another website or application pool, you can change it manually afterward (with the IIS manager).

All releases here: https://github.com/cedrozor/myrtille/releases

## Docker
From version 2.8.0, Myrtille is available as a docker image.

No installation is required, you just need Docker Desktop or Toolbox for Windows (Windows 10 Pro or Windows Server 2016 or greater) with Windows containers enabled and Hyper-V isolation.

Myrtille is fully functional as a container, but there are also some limitations (inherent to Windows containers): print and audio redirection (through RDP) is not supported at the moment (this may change into a future version).

You can pull it from Docker Hub with the following command (use a tag for a specific version, or latest otherwise)<br/>
```
docker pull cedrozor/myrtille(:tag)
```

To list the network adapters available to Docker<br/>
```
docker network ls
```

Run the image in detached mode (optionally provide the resulting container a network adapter able to connect your hosts)<br/>
```
docker run -d (--network="<network adapter>") cedrozor/myrtille(:tag)
```

To list the containers<br/>
```
docker ps -a
```

To open a shell into a container (and be able to explore it, check its **ip address**, logs, etc.)<br/>
```
docker exec -it <container ID> cmd
docker exec -it <container ID> powershell
```

To stop a container<br/>
```
docker stop <container ID>
```

If you intend to have custom settings, manage your hosts or keep track of the logs, you will need to perform additional steps (**data persistence**).

More details into **Dockerfile**.

## Remote Desktop Services

**For best experience**, and be able to go over the default limit of 2 concurrent users, you will need to install the [RDS role](https://www.exitthefastlane.com/2016/05/native-rds-in-server2016-part-1-basics.html) on your remote server(s). Optionally, you can install the [RD Connection Broker](https://www.tech-coffee.net/rds-2016-farm-deploy-the-farm-in-azure/) feature to allow load-balancing across an RDS farm (possibly hosted on Azure) and manage the applications allowed to run (and with which parameters) on session start.

You will also need to enable [multiple sessions per user](https://portal.databasemart.com/kb/a220/how-to-enable-multiple-single-remote-desktop-sessions-in-windows-server-2012.aspx) if you don't want your users to be limited to 1 session only (if you have several users sharing the same account, they will otherwise disconnect each others).

The RDS role offers you a **grace period of 120 days** after which you will need to use an [RD License Server](https://www.exitthefastlane.com/2016/06/native-rds-in-server2016-part-3-rdsh.html) with proper licenses (CALs) to suit your licensing mode ("Per Device" or "Per User").

**PLEASE READ!** starting with Windows Server 2019 (or Windows 10 release 1903), RDS doesn't reconnect a previously disconnected session (due to a network issue or a manual disconnect) when multiple sessions per user is enabled on the RDS host; instead, it creates a new session. A workaround to that behavior is to lock the session before disconnecting the session (see https://nakedsecurity.sophos.com/2019/06/06/microsoft-dismisses-new-windows-rdp-bug-as-a-feature/ and https://social.technet.microsoft.com/Forums/windowsserver/en-US/baa5cb75-2fed-4d90-bbc6-404eab4aa20d/2019-rdsh-not-able-reconnect-to-disconnected-session-every-time-new-session-with?forum=winserverTS).

Consequently, starting from version 2.8.0, Myrtille scales the session display instead of reconnecting the session (with the new browser size), when the browser is resized. To change that default setting, edit (myrtille path)\js\config.js ("display" section). In addition, you can choose whether or not to keep the aspect ratio of the display (default = don't keep).

## Auto-connect / Start remote application from URL
Starting from version 1.3.0, it's possible to connect and run a program automatically, on session start, from an URL. It's a feature comparable to remoteApp (.rdp files).

From version 1.5.0, Myrtille does support hashed passwords (so that the password is not plain text into the url). The objective is to have distributable urls to third parties without compromising on security (by giving real passwords); connections are then only possible through myrtille (because direct connections would require the real passwords) and access control could be added into IIS.

The start remote application from url feature only works on Windows Servers editions (starting from Server 2012) and only if the program is allowed to run (remoteApp policy). See notes and limitations.

On Windows Home Editions, there are several workarounds to start an application automatically on start. You can for example set a Group Policy or define a startup program in registry (HKey_Current_User\Software\Microsoft\Windows\CurrentVersion\Run).

### Syntax
https://myserver/Myrtille/?__EVENTTARGET=&__EVENTARGUMENT=&server=server&domain=domain[optional]&user=user&passwordHash=passwordHash&program=program[optional]&width=width(px)[optional]&height=height(px)[optional]&connect=Connect%21

Don't set the **"&program="** parameter (or leave it empty) for a direct access to the desktop or set the executable path, name and parameters otherwise (double quotes must be escaped).

The pre version 1.5.0 syntax ("&password=*password*") is still supported, but it's advisable to move to the safer syntax.

The parameters values **must be URL encoded**. You can use a tool like http://www.url-encode-decode.com/ (just copy & paste the encoded parameters into the URL).

If you want to connect an Hyper-V VM automatically, add the **"&vmGuid="** parameter (and remove "&domain=" and "&program="). For enhanced mode, also add **"&vmEnhancedMode=checked"**.

For SSH auto-connection, add **"&hostType=1"** (and remove "&domain=" and "&program=").

### Password Hash
To generate a password hash, you can use the powershell script "password51.ps1" on the myrtille gateway (requires access to the machine). The script is located into the myrtille bin folder at runtime or into the "Myrtille.Services" project under Visual Studio.
- Run the script (from its location folder): ". .\password51.ps1" (if needed, see powershell script execution policy: https://technet.microsoft.com/en-us/library/ee176961.aspx)
- Call the encrypt function: "Encrypt-RDP-Password -Password *password*"
- Copy & Paste the result into your URL

From version 2.3.0, you can also generate a password hash from url (thanks jol64). syntax: https://server/myrtille/GetHash.aspx?password=password

The password hash is only valid on the machine which generated it (the myrtille gateway); it won't work on another machine. Its length is 492 chars.

For further information, see https://docs.microsoft.com/en-us/dotnet/standard/security/how-to-use-data-protection

## File transfer
Myrtille supports both local and network file storage. If you want your domain users to have access to their documents whatever the connected server, follow these steps:
- Ensure the machine on which Myrtille is installed is part of the domain
- Create a network share, read/write accessible to the domain users (i.e: \\\MYNETWORKSHARE\Users)
- Create a Group Policy (GPO), or edit the default one, on your domain server with a folder redirection rule (for the "Documents" folder, see https://mizitechinfo.wordpress.com/2014/11/18/simple-step-configure-folder-redirection-in-window-server-2012-r2/)
- In the target tab, select basic configuration to redirect everyone's folder to the same location, with create a folder for each user under the root path (the network share)
- In the settings tab, ensure the user doesn't have exclusive rights to the documents folder (otherwise Myrtille won't be able to access it)

## Print document
From version 1.9.0, myrtille does support local or network printing through a pdf virtual printer, "**Myrtille PDF**", installed on the gateway. This feature can be disabled into bin/Myrtille.Services.exe.config ("FreeRDPPdfPrinter" key).
It works like any other printer, using the print feature of your application. The resulting pdf is downloaded to the browser and can be opened/saved/printed from there.
It can also work standalone (without myrtille integration). If used directly on the gateway (without the "redirected" suffix), or as a network printer, it will ask for the pdf output location (to change that default behavior, see bin/Myrtille.Printer.exe.config).

Alternatively, on Windows 10 / Server 2016, Windows provides a "Microsoft Print to PDF" printer. You can thus create a pdf on the remote server then download it (using the file transfer), but it implies an additional step.

If your browser machine is on the same network as the gateway or the remote server, you also have the option to use a network printer directly from the remote session.

If your remote server have internet access, you can also use a cloud printer (such a Google Print).

## Security
The installer can create a self-signed certificate for myrtille (so you can use it at https://myserver/myrtille), but you can set your own certificate (if you wish) to prevent the users from having to add a security exception into their browser.

## Configuration / Performance tweaks / Debug
Both the gateway and services have their own .NET config files; the gateway also uses XDT transform files to adapt the settings depending on the current solution configuration.

You may also play with the "js/config.js" file settings to fine tune the client configuration depending on your needs.

The most important client settings (js/config.js) are:
- **imageEncoding**: set the format used to render the display; possible values: AUTO, PNG (default, best performance and quality), JPEG or WEBP. For optimized bandwidth, the recommended setting is AUTO (encode in both PNG and JPEG and send the lowest sized); adjusted dynamically to fit the latency (more latency = switch to JPEG)
- **imageQuality**: set the % quality of the rendering (higher = better); not applicable for PNG (lossless); adjusted dynamically to fit the latency (more latency = switch to JPEG and lower the image quality)
- **imageQuantity**: set the % completeness of the rendering (lower = higher drop rate); useful for low server CPU / bandwidth; use with caution as skipping some images may result in display inconsistencies; adjusted dynamically to fit the latency (more latency = switch to JPEG and lower the image quantity)
- **mouseMoveSamplingRate**: set the % sampling of the mouse moves (lower = higher drop rate); useful to reduce the server load in applications that trigger a lot of updates (i.e.: graphical applications)
- **bufferEnabled**: buffer for user inputs; adjusted dynamically to fit the latency (more latency = more bufferization)

Into the gateway settings (Web.config):
- **AllowRemoteClipboard**: allow to access the remote clipboard (default enabled)
- **AllowFileTransfer**: allow to upload/download files (default enabled)
- **AllowPrintDownload**: allow to print to pdf (default enabled)
- **AllowSessionSharing**: allow to share a remote session (default enabled); from version 2.0.0, guests can't interact with the shared session (view only)
- **ClientIPTracking**: track the client IP (default disabled) and denies access in case of IP change; this should be disabled in some network configurations: shared proxy, roaming connection, private browsing, etc.
- **ClientIdleTimeout**: disconnect the session after a period of time if the browser window/tab is closed, or connection is lost, to prevent it from being left open server side; default 60000 ms (1 mn). 0 to disable

Into the services settings (bin/Myrtille.Services.exe.config):
- **RemoteSessionLog**: rdp/ssh client logs (default disabled); stored into the log folder
- **FreeRDPxxx**: rdp client settings; allow to tweak the remote connection options (wallpaper, theme, color depth, audio, etc.). use with caution!
- Multifactor Authentication and Enterprise Mode configuration

Into the PDF virtual printer settings (bin/Myrtille.Printer.exe.config):
- **OutputFile**: default output file name, if the printer is used standalone (without myrtille integration)
- **AskUserForOutputFilename**: whether to display or not a dialog box to prompt for output file name, if the printer is used standalone

## Code organization
- **Myrtille.RDP**: link to the myrtille FreeRDP fork. C++ code. RDP client, modified to forward the user input(s) and encode the session display into the configured image format(s). The modified code in FreeRDP is identified by region tags "#pragma region Myrtille" and "#pragma endregion".
- **Myrtille.SSH**: SSH.NET client. Same logic as for FreeRDP, use named pipes to communicate with the gateway.
- **Myrtille.Common**: C# code. Common helpers. These are static libs which could be used for any project (not only myrtille).
- **Myrtille.Services**: C# code. WCF services, hosted by a Windows Service (or a console application in debug build). start/stop the rdp/ssh client and upload/download file(s) to/from the connected user documents folder.
- **Myrtille.Services.Contracts**: C# code. WCF contracts (interfaces).
- **Myrtille.MFAProviders**: C# code. Multifactor Authentication providers. Currently used with the OASIS adapter but could be any other.
- **Myrtille.Enterprise**: C# code. Integration with Active Directory. Provides a dashboard to administrate the RDP and SSH hosts on a domain.
- **Myrtille.Web**: C# code. IIS Web application; gateway between the browser and the rdp/ssh client; maintain correlation between http(s) and rdp/ssh sessions.
- **Myrtille.Setup**: MSI installer.

## Build
Myrtille uses C#, C++ and vanilla Javascript code (no additional libraries). Microsoft Visual Studio Community 2017 was used as primary development environment, using the .NET 4.5 framework.
If you want Visual Studio to load the Myrtille setup project, you have to install the (official and free) Microsoft Visual Studio 2017 Installer Projects extension (https://marketplace.visualstudio.com/items?itemName=visualstudioclient.MicrosoftVisualStudio2017InstallerProjects).

The Myrtille build have the two classic solution configurations: "Debug" and "Release", on "Any CPU" platform.

Starting from myrtille version 1.1.0, the FreeRDP code (modified for myrtille needs) is no longer part of the myrtille repository.

The (new) myrtille FreeRDP code can be found at https://github.com/cedrozor/FreeRDP

The objectives are:
- Have a loose coupled dependency between myrtille and FreeRDP (so that FreeRDP could be replaced by another RDP client implementation, if needed)
- Benefits from the latest FreeRDP changes (bugfixes, new features, latest RDP protocol support, etc.), by synchronizing the fork with the FreeRDP repository (periodically, with a stable branch or after ensuring the master branch is stable)
- Extends myrtille to other remote access protocols. The gateway is (always was) protocol agnostic. For example, myrtille could be linked to an SSH client (the same way it's linked to an RDP client), modified to handle the user inputs and display updates

Steps to build the FreeRDP fork (and have it working with myrtille):
- Git clone https://github.com/cedrozor/FreeRDP.git into "<myrtille root folder>\Myrtille.RDP\FreeRDP\" (**NOTE** if using TortoiseGit, the contextual menu won't show the "Git clone" option from the "Myrtille.RDP" folder; you will have to do it from elsewhere, outside of the myrtille tree; also, don't create the "FreeRDP" folder manually, just write it into the clone target path)
- Install OpenSSL; precompiled installers here: https://slproweb.com/products/Win32OpenSSL.html (use the **32 bits** full version (not the light one))
- Run cmake as detailed here: https://github.com/FreeRDP/FreeRDP/wiki/Build-on-Windows-Visual-C---2012-(32-and-64-bit) to generate the Visual Studio solution and projects accordingly to your dev environment
- Open and build the generated solution

If you plan to build the myrtille installer, you have first to build the FreeRDP fork.

Then, into the installer project, add the following projects outputs (as primary output):
- winpr
- winpr-tools
- freerdp
- freerdp-client
- wfreerdp-client

The installer build should now work.

### Startup projects
If you want to run Myrtille with Visual Studio, you should set startup projects on the solution in the following order (multiple startup projects):
- Myrtille.Admin.Services (start) * only required by the Myrtille.Admin.Web mockup *
- Myrtille.Services (Start)
- Myrtille.Web (Start)

You can choose the browser you want to use by right-clicking an ASPX page into the "Myrtille.Web" project and click "Browse With...".

The FreeRDP executable is intended to be run from the "Myrtille.RDP" folder.

You can debug FreeRDP, while an rdp session is active, by attaching the debugger to the process "wfreerdp.exe" (native code). Same for SSH with "Myrtille.SSH.exe" (managed code).

Before first run, retrieve (git clone) the myrtille FreeRDP fork (https://github.com/cedrozor/FreeRDP) into the "Myrtille.RDP" folder and build it there in order to generate "wfreerdp.exe".

Hit F5 to start debugging.

## Communication
### Overview
web browser <-> web gateway <-> wcf services <-> client <-> host

### Protocols
web browser <-HTTP(S),XMLHTTP,WS(S)-> web gateway <-WCF-> wcf services <-SYSTEM-> rdp/ssh client <-RDP/SSH-> rdp/ssh server

"SYSTEM" is simply starting the client as a local process, and capture the event of it being stopped.

In order to speed up the communication between the web gateway and the client, I decided to bypass the wcf services layer and set a direct communication between the two by using named pipes.

It has 2 advantages: first, it's a bit faster than using TCP and, second, it guarantees a FIFO unstacking to preserve the images order.	It has also a drawback: both the web gateway and the wcf services must be on the same machine.

After weighing pros and cons, I decided to kept it as is because it also has 2 additional benefits: setting up a named pipe in C++ is a bit simpler than a TCP socket (named pipes are handled as simple files) and having a single setup is easier for the end user.
		
web gateway <-IPC-> rdp/ssh client
	
However, I'm fully aware that it breaks the distributed architecture pattern. Thus, if you want, feel free to move the named pipes management up to the wcf services layer and proxy the data from/to the web gateway.

This is a thing to consider if you want to isolate the web gateway from your intranet (into a DMZ for instance) and still be able to connect a machine on it.

## Multifactor Authentication
Enabling this option allows you to require users to provide a one-time passcode which is validated against a cloud based 2fa authentication platform. This requires a mobile application which can be used to scan QR codes and generate the one-time passcodes; these applications are free from app stores and a popular choice is google authenticator.

In addition to 2fa, an access control could be enforced to allow connections only from given areas (geo ip location) and time (not implemented for now).

The adapter has been written to use a platform provided by Olive Innovations and named OASIS. It's free to use up to 10 users; to use this service follow these instructions:

- Visit https://www.oliveinnovations.com and register for free
- Once logged in, create a User Group (into the menu select User Groups), then click New, input a Name (i.e. Myrtille) and save
- Create a user (choose Users from the menu), then click New, input user details and tick the box to send register by email (**IMPORTANT** the username must be the same as the username you will login with myrtille; when registering via email, the user will have a link to complete the registration) and save. Into the user details page, select the user group created in Step 2
NOTE: If you have enabled Enterprise Mode and wish to sync your Active Directory with OASIS, visit https://www.oliveinnovations.com, go to download area and download the Gateway application; instructions for configuration can be found into the docs on the same website
- Create an application (choose Applications from the menu), then click New, enter a Name and save. You will be directed to the application details page, grant access to the user group created in Step 2
- Within the application page, click the button Application Key, this will display the information to configure myrtille

Once these steps are completed, edit [myrtille path]\bin\Myrtille.Services.exe.config and uncomment the following appSettings:
- `MFAAuthAdapter`, this is the OASIS MFA adapter
- `OASISApiKey`, this is the API Key found when you clicked Application Key in step 5
- `OASISAppID`, this is the App ID found when you clicked Application Key in step 5
- `OASISAppKey`, this is the App Key found when you clicked Application Key in step 5
- Restart Myrtille.Services windows service to use the new settings

The included MFA adapter is written by Olive Innovations Ltd for use with the OASIS platform. This adapter uses a nuget package, OASIS.Integration (https://www.nuget.org/packages/OASIS.Integration/) available open source (https://github.com/OliveInnovations/OASIS/).
If you wish to create your own MFA adapter, `Myrtille.Services.Contracts` contains the interfaces you need.

## Enterprise Mode
When enabled, the enterprise mode authenticates users against a domain and allows administrators to create hosts connections (which can be restricted to the security groups the authenticated users belongs to).

Hosts are presented into a simple dashboard and can be connected in 1 click.

**CAUTION** This requires the myrtille machine to have joined the domain or be able to resolve the domain controller FQDN or IP.

The enterprise mode provides the following additional features:
- Authenticate users against a domain/active directory instead of a host they wish to connect to
- Allow administrators to define a list of hosts; these hosts are the only hosts the users can connect to
- Access to hosts can be restricted based on the groups the authenticated users belongs to
- Administrators can create a single use session url to a specific host (with specific login credentials) which can be shared with external (non domain) users and only be used once

To enable enterprise mode, edit [myrtille path]\bin\Myrtille.Services.exe.config and uncomment the following appSettings:
- `EnterpriseAdapter`, this is the adapter to use for enterprise mode
- `EnterpriseAdminGroup`, this is the security group which will define a user as an administrator who can create, edit, delete hosts, define access to hosts and create single use sessions
- `EnterpriseDomain`, this is the name of your domain (i.e. MYDOMAIN or mydomain.local) if myrtille is part of it or the domain controller FQDN or IP otherwise
- Restart Myrtille.Services windows service to use the new settings

To specify a custom path for the MyrtilleEnterprise database or use another SQL server, amend Myrtille.Services app.config connectionString

If you wish to create your own enterprise adapter (with a different authentication, database or behavior), `Myrtille.Services.Contracts` contains the interfaces you need.

From version 2.8.0, myrtille provides a simplified host management designed for a local administrator, without the need for a domain and without group access restrictions. The default credentials are "**admin**"/"**admin**" (username/password) and are changed upon first login. See comments into (myrtille path)\bin\Myrtille.Services.exe.config.

## REST APIs

Myrtille can support custom REST APIs, to have additional controls over connections and remote sessions.

For example, if you want to hide the connection parameters from the end user (browser), you can use (implement) the **IConnectionService** interface to provide connection details from a connection identifier. This API can also be used to track the connections states (connection/disconnection time, exit code, etc.).

Myrtille also implement a capture API (**ICaptureService**) in order to take screenshots, on demand or periodically, of a remote session display. This could be useful for monitoring user activity on sensitive servers.

For an improved sharing experience, you also have a sharing API (**ISharingService**) to manage the invitations for a remote session, the access privileges, track the active connections, disconnect a guest, etc.

You can check the related endpoints, methods and params into these interfaces (Myrtille.Services.Contracts) and have your own implementations.

See the Myrtille.Admin.Web **mockup** to see how to implement Myrtille into your website and use these APIs.

## Notes and limitations
- Starting from myrtille version 1.2.0, the packaged FreeRDP and OpenSSL binaries use a statically-linked runtime; that means there is no longer need for the Microsoft Visual C++ redistributables (x86). It's still a good idea to install them however as they will be required if the build options are changed.

- On Windows Server 2003, there are reported display issues with FreeRDP when using Chinese language. Disabling the glyph cache fixes the issue (myrtille\bin\Myrtille.Services.exe.config).

- On Windows Server 2008 and Windows Workstations (XP/Vista/7/8/10), the FreeRDP remoteApp and shell features don't work. It's not possible to start a remote application from URL. https://github.com/FreeRDP/FreeRDP/issues/1669

- On Windows Server 2012, you may have issues installing the Microsoft Visual C++ 2015 redistributables (x86) (http://stackoverflow.com/questions/31536606/while-installing-vc-redist-x64-exe-getting-error-failed-to-configure-per-machi). To circumvent that, ensure your system is fully updated (Windows updates) first or try to install the package "Windows8.1-KB2999226-x64.msu" manually.

- Safari 5.1.7 doesn't support the IIS 8+ websocket implementation because it's based on an older version of the websocket standard (hybi instead of RFC6455) (http://stackoverflow.com/questions/32628211/connecting-to-iis-8-from-safari-5-1-7-by-websocket). Myrtille does fallback to long-polling if websockets aren't supported or fail.

- Safari 5.1.7 doesn't support xtermjs (SSH terminal); this version of Safari for Windows is not officially supported anymore, so you should use something else anyway.

- xtermjs (SSH terminal) rendering is not blazing fast; it's functional but don't expect a native putty client, neither in speed and features. Also, remember to refresh (or "cls") the terminal often because it gets slower as the DOM fills with data.

- In order to keep the installation simple, both the myrtille gateway and services are installed on the same machine. They do however conform to a distributed architecture; if needed, given some additionnal code, myrtille services could acts as a proxy, so the gateway could be installed and operate separately (this could be handy if the gateway should go into a DMZ).

- Keyboard is mapped to english/US (latin QWERTY) layout by default. If you have troubles with some characters/keys not working as expected, try to add that keyboard layout to your server (and select it when connected).

- To connect an Hyper-V VM, you have to enter the Hyper-V host as server (hostname or address), the guest VM GUID (https://www.petri.com/get-hyper-v-virtual-machine-process-id-and-guid) and a valid user/password on the Hyper-V host (authentication is done locally). The user must be member of the **"Hyper-V Administrators"** group or granted access to the VM with the **Grant-VMConnectAccess** cmdlet (https://docs.microsoft.com/en-us/powershell/module/hyper-v/grant-vmconnectaccess?view=win10-ps). The VM will then request authentication on its own (and you will be able to use a domain user, if needed).

- When connecting an Hyper-V VM, the display is limited to the VM resolution (default 1024 x 768) whatever the client (browser) resolution. You can change that resolution into the advanced display settings of the VM (even directly within the session, if the connected user have enough privileges).

- If enhanced mode is not supported by the VM, or disabled, the remote clipboard and the "Myrtille PDF" virtual printer are not available into the session. See https://www.tenforums.com/tutorials/57136-turn-off-hyper-v-enhanced-session-mode-windows-10-a.html for Windows 10 and http://www.systemcentercentral.com/what-is-enhanced-session-mode-in-windows-server-2012-r2-hyper-v-and-when-should-i-use-it/ for others versions.

- The "Start program from url" feature doesn't work with an Hyper-V VM connection.

- in HTML4 mode, data is sent to Myrtille using xhrs; these are limited in size (depends on browser and IIS config), so sending a large block of text with the "keyboard" popup may fail in such a case

- the clipboard synchronization requires Chrome (or async clipboard API support) and HTTPS connection and is limited to text only and 1MB max

## Troubleshoot
First at all, ensure the Myrtille prerequisites are met (see "Installation").

Also please read notes and limitations above.

- The installation fails
	- Prerequisites are not downloaded: the installer was run directly using the MSI file; this exclude the bootstrapper (Setup.exe), whose purpose is to check and download/install the prerequisites if necessary
	- Prerequisites download fails: MSI installers use Internet Explorer for downloads; ensure the Internet Explorer enhanced security is disabled (server administration tools) and your system clock is up to date
	- Error 1001: an MSI custom action failed (https://blogs.msdn.microsoft.com/vsnetsetup/2012/03/14/msi-installation-fails-while-installing-a-custom-action-with-error-1001-exception-occurred-while-initializing-the-installation/). Ensure the IIS Management Console is enabled (http://stackoverflow.com/a/23263836).
	- Check the Windows events logs ("System", "Application", etc.)

- I can't access http://myserver/myrtille
	- Ensure IIS is started and "Myrtille.Web" application is running on the "MyrtilleAppPool" application pool.
	- If you have IIS 8 or greater, ensure the websocket protocol is enabled (HTML4 clients will automatically fallback to long-polling).
	- Ensure .NET 4.5 is installed (https://stackoverflow.com/questions/6425442/http-error-404-3-not-found-in-iis-7-5/8487214#8487214) and the "MyrtilleAppPool" is running on it.
	- Ensure you have enabled **HTTP Activation** under ".NET Framework 4.5+ Features" > "WCF Services"
	- You may have to register .NET 4.5 against IIS (https://stackoverflow.com/questions/13749138/asp-net-4-5-has-not-been-registered-on-the-web-server)
	- Ensure the "Myrtille.Web" target folder does have enough privileges (should be set automatically by the installer but may depend on specific configurations)
	- You can have IIS to show a **detailed error information** (in addition to the HTTP error code); see https://stackoverflow.com/questions/2640526/detailed-500-error-message-asp-iis-7-5

- Nothing happens when I click "Connect!"
	- Ensure you entered valid connection information (server address, user credentials, etc.).
	- Ensure the network traffic (websockets and xmlhttp in particular) is not blocked by a firewall, proxy, reverse proxy, VPN or whatever.
	- Ensure the "Myrtille.Services" Windows service (or console application if running under Visual Studio) is started.
	- Ensure the RDP (or SSH) client ("wfreerdp.exe" or "Myrtille.SSH.exe") does exists (into the "Myrtille.RDP" or "Myrtille.SSH" output folder, if running under Visual Studio, or into the "bin" folder otherwise); if not, you need to retrieve (git clone) the myrtille FreeRDP fork into the "Myrtille.RDP" folder and build it there.
	- Ensure the Microsoft Visual C++ 2015 redistributables (x86) are installed (control panel > programs > programs and features); they are required by the RDP client (myrtille\bin\wfreerdp.exe).
	- Try to run wfreerdp.exe (just double-click it, no params), into the myrtille binaries folder. if there is a missing dll, or another issue, Windows should popup an error message (i.e.: MSVCR120.dll = MSVC 12 = Microsoft Visual C++ 2013 redistributables, MSVCP140.dll = MSVC 14 = Microsoft Visual C++ 2015 redistributables, etc.). Depending on your build environment and options, you will need the appropriate redistributables.
	- Check the RDP (or SSH) server configuration (does the user exists, is it a member of the "Remote Desktop Users" group, are Remote Desktop CALs valid?, etc.). You can setup it automatically by importing the "myrtille\bin\RDPSetup.reg" file into registry.
	- Check the RDP (or SSH) server windows event logs.
	- Check the gateway windows event logs, particulary regarding .NET.
	- Retry with debug enabled and check logs (into the "log" folder). You can change their verbosity level in config (but be warned it will affect peformance and flood the disk if set too verbose).

- Reconnection on browser resize (toolbar option) doesn't work
	- this feature requires the RDS disconnection timeout to be set and greater than 5 secs (to give FreeRDP time to reconnect)

- Some characters/keys are not working as expected
	- Keyboard is mapped to english/US layout by default. Try to add that layout to your server (and select it when connected).

- I don't have a mouse (or a right button), how can I Right-Click? (i.e.: on a touchpad or iOS device)
	- You can toggle on the "Right-Click" button into the toolbar, then touch or left-click the screen to trigger a right-click at that position

- Nothing happens when I click on some toolbar buttons
- Nothing happens when I print with the "Myrtille PDF" redirected printer
	- Although myrtille dialogs should work (same domain origin policy), ensure you don't have a popup blocker (Chrome have one by default), or disable it or add an exception for the myrtille domain

- The RDP (or SSH) session continues to run after clicking "Disconnect"
	- Check the RDP (or SSH) server configuration (session disconnect timeout in particular). You can setup it automatically by importing the Myrtille "myrtille\bin\RDPSetup.reg" file into registry.

- Myrtille is slow or buggy
	- Enable the stats bar to have detailed information about the current connection. Check latency and bandwidth, among other things.
	- Ensure debug is disabled or otherwise logs are not set to "Information" level (Myrtille "Web.Config" file, "system.diagnostics" section, default is "Warning"). Check logs, if debug is enabled. **FreeRDP logs can be enabled into bin/Myrtille.Services.exe.config ("RemoteSessionLog" key, at the bottom of the file)**. Logs are located into the log folder.
	- If debug is enabled and you are running Myrtille in debug mode under Visual Studio, you will have the FreeRDP window (session display) and console (rdp events) shown to you. Same for SSH. It may help to debug.
	- Switch from HTML4 to HTML5 rendering, or inversely (should be faster with HTML5).
	- Check your network configuration (is something filtering the traffic?) and capabilities (high latency or small bandwidth?).
	- Ensure buffering is enabled (see configuration / performance tweaks / Debug (#configuration--performance-tweaks--debug))
	- the SSH terminal (xtermjs) becomes laggy after some time; try to refresh or clear ("cls") the screen from time to time
	- Maybe the default settings are not adapted to your configuration. You can tweak the "js/config.js" file as you wish (see extensive comments there).
	- Despite my best efforts to produce quality and efficient code, I may have missed/messed something... Please don't hesitate to tell me or add your contribution! Thanks! :)