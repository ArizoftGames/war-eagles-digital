Current completed structure of War Eagles by scene:

Audio Bus Layout: res//Audio/WE_Bus_layout.tres; Busses: Master, fed by Music, Planes, and Effects;  Planes and Effects will be consolidated from player POV and are separate for animation mixing with the game.

Autoloads:

GameManager.cs - centralizes global actions and frequently called methods.
	 _Input(Input Event @event) {detects explicitly defined input events and calls methods based on them}, PauseGame(), ScreenShot(), AccessConsole() {Not yet implemented}
UnitDatabase.cs - reads data fron csv parsers and constructs dictionaries.
	GetAirUnitByName(string unit), GetAntiAircraftUnitByName(string unit), GetZoneByTargetName		(string targetName), GetAceByPilot(string pilot), GetEventByName(string name), 		GetZoneAsDictionary(string targetName), GetAirUnitAsDictionary(string unitName), 		GetEventAsDictionary(string name)

AudioManager.cs - instances AudioStreamPlayers to play music and effects; music may be played by (Player Nationality + AI Mood) or by use case.  Connects to busses (Music or Effects).
	PlayMusicByNationMood(string nationality, string mood), PlayMusicByUseCase(string useCase),  		PlaySoundEffect(string key), StopMusic(), SetBusVolume(string busName, float linearVolume), 		GetBusVolume(string busName), SetEffectsVolume(float linearVolume) *For future Options UI; sets 	Planes and Effects bus volumes

IntroAnim.tscn: Current main scene.  Plays disclaimer screen and introductory animation (skippable) and then presents main menu with buttons for Continue, New [Game], Load, Tutorial, Credits, Options, Extras, Help, and Quit.

PauseMenu.tscn: Will be activated at any time during gameplay with ESC.  Immediately presents main menu with buttons for Continue, Save, Load, New [Game], Credits, Options, Extras, Help, and Quit.

CreditRoll.tscn: Displays credits.  ESC, Spacebar, or internal Back button all return to PauseMenu.tscn.