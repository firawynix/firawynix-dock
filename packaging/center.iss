; Instaladores independentes para o Firawynix Center.
; Compilar com /DCenterArch=x64, x86 ou online.
#ifndef CenterArch
  #error Defina /DCenterArch=x64, x86 ou online
#endif

#define DockName "Firawynix Dock"
#define DockVersion "1.1.1"
#define DockAppId "{{887A8A7A-D695-43E4-8B76-DEA0E1F67EA2}"

#if CenterArch == "x64"
  #define PackageName "Firawynix-Dock-Setup-x64"
#elif CenterArch == "x86"
  #define PackageName "Firawynix-Dock-Setup-x86"
#elif CenterArch == "online"
  #define PackageName "Firawynix-Dock-Setup"
#else
  #error CenterArch invalida
#endif

[Setup]
AppId={#DockAppId}
AppName={#DockName}
AppVersion={#DockVersion}
AppVerName={#DockName} {#DockVersion}
VersionInfoVersion=1.1.1.0
AppPublisher=Firawynix
AppPublisherURL=https://lab.firawynix.com.br/dock/
AppSupportURL=https://lab.firawynix.com.br/dock/
DefaultDirName={localappdata}\Programs\Firawynix Dock
DefaultGroupName=Firawynix Dock
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
UninstallDisplayIcon={app}\FirawynixDock.exe
OutputDir=..\dist\center
OutputBaseFilename={#PackageName}
SetupIconFile=..\icon.ico
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
#if CenterArch == "x64"
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
#elif CenterArch == "x86"
ArchitecturesAllowed=x86compatible
#else
ArchitecturesInstallIn64BitMode=x64compatible
#endif

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Files]
#if CenterArch == "x64"
Source: "..\dist\win-x64\FirawynixDock.exe"; DestDir: "{app}"; Flags: ignoreversion
#elif CenterArch == "x86"
Source: "..\dist\win-x86\FirawynixDock.exe"; DestDir: "{app}"; Flags: ignoreversion
#else
Source: "..\dist\win-x64\FirawynixDock.exe"; DestDir: "{app}"; Flags: ignoreversion; Check: Is64BitInstallMode
Source: "..\dist\win-x86\FirawynixDock.exe"; DestDir: "{app}"; Flags: ignoreversion solidbreak; Check: not Is64BitInstallMode
#endif
Source: "..\catalogo-snapshot.json"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Firawynix Dock"; Filename: "{app}\FirawynixDock.exe"
Name: "{autodesktop}\Firawynix Dock"; Filename: "{app}\FirawynixDock.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na área de trabalho"; GroupDescription: "Atalhos adicionais:"; Flags: unchecked

[Run]
Filename: "{app}\FirawynixDock.exe"; Description: "Fixar o Firawynix Dock na barra de tarefas"; Flags: nowait
