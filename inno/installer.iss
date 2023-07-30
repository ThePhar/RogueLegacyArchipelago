; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppId "{6741975E-782A-438F-8C05-502EDB37E7DB}"
#define MyAppName "Rogue Legacy Randomizer"
#define MyAppVersion "1.0 Dev Pre3"
#define MyAppPublisher "Zach Parks & Cellar Door Games"
#define MyAppURL "https://github.com/ThePhar/RogueLegacyRandomizer"
#define MyAppExeName "Rogue Legacy Randomizer.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
AppCopyright=Copyright (C) 2011-2018 Cellar Door Games
DefaultDirName=C:\Program Files (x86)\{#MyAppName}
DisableProgramGroupPage=no
LicenseFile="..\LICENSE"
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputBaseFilename=Rogue Legacy Randomizer {#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\bin\Debug\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Debug\Archipelago.MultiClient.Net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Debug\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\lib\DS2DEngine.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\lib\FAudio.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\lib\FNA.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\lib\FNA3D.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\lib\InputSystem.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\lib\SDL2.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\lib\SpriteSystem.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\lib\Tweener.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\lib\Gma.System.MouseKeyHook.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\CustomContent\*"; DestDir: "{tmp}\CustomContent"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: ".\redist\xnafx40_redist.msi"; DestDir: "{tmp}"; Flags: ignoreversion deleteafterinstall
Source: "..\ico\RLR.ico"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Setup]
SetupIconFile="..\ico\RLR.ico"
WizardSmallImageFile=".\images\sirphar.bmp"
WizardImageFile=".\images\banner.bmp"
UninstallDisplayIcon={app}\RLR.ico
UninstallDisplayName={#MyAppName}

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "msiexec.exe"; Parameters: "/i ""{tmp}\xnafx40_redist.msi"" /qb"; WorkingDir: {tmp}; StatusMsg: "Installing XNA redistributable..."
Filename: "xcopy.exe"; Parameters: "/Y /E /I ""{code:CopyDir}\Content"" ""{app}\Content"""; StatusMsg: "Copying Content files..."
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent; StatusMsg: "Installing Rogue Legacy Randomizer"
Filename: "xcopy.exe"; Parameters: "/Y /E /I ""{tmp}\CustomContent"" ""{app}\Content"""; StatusMsg: "Overwriting content files with custom content..."

[UninstallDelete]
Type: filesandordirs; Name: "{app}\Content"

[Code]
var
  CopyDirPage: TInputDirWizardPage;

procedure InitializeWizard();
begin
  CopyDirPage := CreateInputDirPage(wpSelectDir, 'Select your vanilla Rogue Legacy source directory.', '',  '', False, '');
  CopyDirPage.Add('Source directory:');
  CopyDirPage.Values[0] := 'C:\Program Files (x86)\Steam\steamapps\common\Rogue Legacy';
end;

function CopyDir(Params: string): string;
begin
  Result := CopyDirPage.Values[0];
end;

function GetUninstallString: string;
var
  sUnInstPath: string;
  sUnInstallString: String;
begin
  Result := '';
  sUnInstPath := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{{#MyAppId}_is1'); { Your App GUID/ID }
  sUnInstallString := '';
  if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString) then
    RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString);
  Result := sUnInstallString;
end;

function IsUpgrade: Boolean;
begin
  Result := (GetUninstallString() <> '');
end;

function InitializeSetup: Boolean;
var
  V: Integer;
  iResultCode: Integer;
  sUnInstallString: string;
begin
  Result := True; { in case when no previous version is found }
  if RegValueExists(HKEY_LOCAL_MACHINE,'Software\Microsoft\Windows\CurrentVersion\Uninstall\{#MyAppId}_is1', 'UninstallString') then  { Your App GUID/ID }
  begin
    V := MsgBox(ExpandConstant('Another version of Rogue Legacy Randomizer was detected. Do you want to uninstall it?'), mbInformation, MB_YESNO); { Custom Message if App installed }
    if V = IDYES then
    begin
      sUnInstallString := GetUninstallString();
      sUnInstallString :=  RemoveQuotes(sUnInstallString);
      Exec(ExpandConstant(sUnInstallString), '', '', SW_SHOW, ewWaitUntilTerminated, iResultCode);
      Result := True;
    end
    else
      Result := False;
  end;
end;