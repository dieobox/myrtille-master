:: update the installer for a quiet install, reinstall all files (except database and logs, which are left unmodified) and suppress the reboot prompt (IIS/.NET doesn't need it)
:: the vbs script used is part of the Windows installer SDK (along with Orca, see https://stackoverflow.com/questions/48315763/how-to-install-orca-which-windows-sdks-contain-the-orca-msi-editing-tool/48316642#48316642)

::@echo off
SET vbsFile=%1%
SET msiFile=%2%

cscript //nologo %vbsFile% %msiFile% "UPDATE Dialog SET Attributes = 1 WHERE Dialog = 'FilesInUse'"
cscript //nologo %vbsFile% %msiFile% "INSERT INTO Property (Property, Value) VALUES ('REINSTALLMODE', 'amus')"
cscript //nologo %vbsFile% %msiFile% "INSERT INTO Property (Property, Value) VALUES ('REBOOT', 'Suppress')"
