#Current completed structure of War Eagles by scene:

##Audio Bus Layout: res//Audio/WE_Bus_layout.tres; Busses: Master, fed by Music, Planes, and Effects;  Planes and Effects will be consolidated from player POV and are separate for animation mixing with the game.

##Global groups for resource management:
audio_players
glb_models
terrains

##UI Themes: res://Data/Resources/UI_Theme .tres  *UI menu pages*
		   res://Data/Resources/MainMenuTheme.tres  *Main Menu; displayed in PauseMenu.tscn and IntroAnim.tscn*
		   res://Data/Resources/HUD_Theme.tres  *Minimal UI for gameplay; template theme [NYI]*
		   res://Data/Resources/HUD_GE_Theme.tres  *German HUD theme; used in gameplay*
		   res://Data/Resources/HUD_HC_Theme.tres  *High Contrast HUD theme; used in gameplay*
		   res://Data/Resources/HUD_JP_Theme.tres  *Japanese HUD theme; used in gameplay*
		   res://Data/Resources/HUD_SU_Theme.tres  *Soviet HUD theme; used in gameplay*
		   res://Data/Resources/HUD_SY_Theme.tres  *System HUD theme; used in gameplay*
		   res://Data/Resources/HUD_US_Theme.tres  *US HUD theme; used in gameplay*
		   res://Data/Resources/HUD_UK_Theme.tres  *British HUD theme; used in gameplay*
		   res://Data/Resources/HUD_WE_Theme.tres  *War Eagles HUD theme; palette match to UI_Menu*

Global function keys: ESC and Spacebar pause and open PauseMenu.
X releases resources and quits immediately.

		    
##Autoloads:

GameManager.cs - centralizes global actions and frequently called methods.
	 _Input(Input Event @event) {detects explicitly defined input events and calls methods based on them}, PauseGame(), ScreenShot(), AccessConsole() {Not yet implemented}, TransitionTo(string NextScenePath)-covers awkward scene transitions (up to ~ 1 sec), SetGameSettings(){NYI}, GetGameSettings() {NYI}
UnitDatabase.cs - reads data fron csv parsers and constructs dictionaries.
	GetAirUnitByName(string unit), GetAntiAircraftUnitByName(string unit), GetZoneByTargetName		(string targetName), GetAceByPilot(string pilot), GetEventByName(string name), 		GetZoneAsDictionary(string targetName), GetAirUnitAsDictionary(string unitName), 		GetEventAsDictionary(string name)

AudioManager.cs - instances AudioStreamPlayers to play music and effects; music may be played by (Player Nationality + AI Mood) or by use case.  Connects to busses (Music or Effects).
	PlayMusicByNationMood(string nationality, string mood), PlayMusicByUseCase(string useCase),  		PlaySoundEffect(string key), StopMusic(), SetBusVolume(string busName, float linearVolume), 		GetBusVolume(string busName), SetEffectsVolume(float linearVolume) *For future Options UI; sets 	Planes and Effects bus volumes

##Scenes:

Loading.tscn:  Current main scene. Displays "Loading..." texz and WE Icon. Fades seamlessly into IntroAnim.  Detects and writes config.cfg, uses existing user edited setting if present.  Makes if necessary and writes to "%APPDATA%\Roaming\War Eagles\War Eagles"

IntroAnim.tscn: Plays disclaimer screen and introductory animation (skippable) and then fades seamlessly into SplashMenu. 

SplashMenu.tscn:  Presents main menu with buttons for Continue, New [Game], Load, Tutorial, Credits, Options, Extras, Help, and Quit.

PauseMenu.tscn: Will be activated at any time during gameplay with ESC or spacebar.  Immediately presents main menu with buttons for Continue, Save, Load, New [Game], Credits, Options, Extras, Help, and Quit.

CreditRoll.tscn: Displays credits via Rich Text Label over a slideshow.  ESC, Spacebar, or internal Back button all return to PauseMenu.tscn.

Extras.tscn: Displays and certain game files and support assets, and  makes if necessary and exports them to %DOCUMENTS%\War Eagles by request.  ESC, Spacebar, or internal Back button all return to PauseMenu.tscn.

Transition.tscn: instantiated by GameManager.TransitionTo(string NestSCenePath) to cover awkward scene transitions. Fades MenuBG.png in and out over 2 seconds.