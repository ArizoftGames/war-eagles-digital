# War Eagles Digital Workflow v8

# War Eagles Digital Edition Creation Plan and Workflow 

## Asset Creation Phase
- **Status**: Completed.

## Animations Phase
- **Status**: Completed.

## Coding Phase
- **Status**: Active.
- **Tasks**:

### UI Preparation and Initial Implementation Sub-Phase *additional UI element implementation is to be expected during gameplay coding*
*This sub-phase serves as an iterative testing and refinement of the Dev>Grok>Copilot Coding Workflow described in Guidelines, with the end goal of minimizing debugging when starting with a largely unknown desired endstate and planning the requirements, composing a prompt, and using Cpilot’s solution-wide visibility to provide base code.*
 1. **Credit Roll**: Completed.

 2. **Extras Folder**: Completed. v 0.1.0
    
3. **Options Menu**: v 0.1.1 upon completion

  Divided into 4 windows, each accessed by a selecting the corresponding Item in MainMenu.OptionsButton (SplashMenu.tscn or PauseMenu.tscn):
	 Index 0. Video and Display: Completed. (Excursus:  Implement Loading.cs as main scene to load settings in config.cfg;  Options windows will read/write to user://War Eagles/config.cfg.  Completed.)
	 Index 1. Audio: Completed. (Excursus: implement MusicManager and EffectsManager as autoloads vis single AudioManager, and UI button clicks via ui_buttons global group. Completed.) 
	 Index 2. Controls – Create ControlsMenuPanel.cs to populate the ControlsMenu branch of Options.tscn.  This should provide a mechanism to enable X-Box controller support and a (so far) fixed input map for KBM and controller including camera controls. Address cost/benefit of implementing custom keybinding.(Excursus: Implement controller detection, integration, and activation.)
	 Index 3. Gameplay Options – Create GameplayMenuPanel.cs to populate the GameplayMenu branch of Options.tscn.  Contains buttons to set persistent gameplay and UI options per player preference;  write to and read from config.cfg reference in Loading.cs and located in user://War Eagles/. 


4. **Help System Presentation**:
 - Create `HelpMenu.tscn` with a `Tree` or tabbed interface for gameplay, controls, and context 
     - Add tooltips and images for clarity, accessible via `PauseMenu.tscn` and splash menu buttons.
     - Implement return logic to `PauseMenu.tscn`.
     - Units and Zone pages present 3d models in embedded VP and stats in label
     - Sections for Game Concepts, Moving Units, Combat Mechanics, Game Setup,  Phases, Units, Zones, Controls, Menus from user_guide.md
     - **Discussion**: A clear help system reduces player frustration in strategy games, per “Game Feel” by Steve Swink. Using tooltips enhances accessibility. 

.  **HUD/in-game interface**
 - Create `HUD.tscn` with a `Control` node for in-game UI.
 - Include:
     - Phase/turn indicator (center)
     - Nationality indicator ([roundel].png)
     - Current VP tally
     - Button links to player/opponent losses pools (dropdown?)
     - Units (Zones) dropdown?  Allow camera focus change?
     - Pause button - instantiates `PauseMenu.tscn` over the current scene.
     - Consider Save (Quicksave?) and Quit buttons (Quit already implemented via X key and on Pause Menu)
     - Hud will begin using HUD_**_Theme.tres as determined by nationality [NYI], but is customizable by player via Video and Display.  Placeholder with res://Data/Resources/HUD_WE_Theme.tres.
     - **Discussion**: A well-designed HUD enhances player immersion and usability.  Early HUD implentation will aid visualization, design and testing of gameplay elements.

### Coding Planning Sub-phase

**Plan Gameplay Coding**:
     - Outline phase-by-phase mechanics (Event, Launch, Action) for C# scripts.
     - Ensure that phase-by-phase coding plan reflects and results in gameplay as described in StandardRules.txt.
     - Plan event purchases and effects.
     - Stress maintaining efficient code and good resource management practices throughout 
     - **Discussion**: Planning gameplay coding ensures modular, testable mechanics, as recommended in Godot forums (godotdevelopers.org).

**Plan State Machine Build and AI Behavior**:
     - Outline opponent AI using decision trees in `OpponentAI.cs`, integrating with gameplay phases.
     - Plan AI-driven event purchases and unit deployments.
     - Ensure AI states align with Standard Rules and gameplay plan for balance.
     - **Discussion**: Planning a state machine with decision trees ensures predictable AI behavior, as noted in Gamasutra AI articles. Outlining `OpponentAI.cs` to integrate with gameplay phases and data supports modularity. Aligning with the gameplay coding plan ensures AI states complement player mechanics.

## Coding Implementation Sub-phase

- **TBD**, largely defined during Coding Planning Sub-phase

- **My Role**:
  1. Provide C# script outlines in `WarEaglesDigital.Scripts`.
  2. Suggest UI tooltip text.
  3. Draft file structures and scene transitions for `PauseMenu.tscn`.
  4. Guide debugging with Godot’s profiler and NUnit tests.
  5. Assist with gameplay and AI planning, ensuring alignment.


#Playtesting Phase

## Mechanic and Systems Validation Subphase
- **Tasks**:
 - Ensure inputs have expected results
 - Ensure expected progression of game occurrences and scene interactions
 - Check and validate visual and audio presentation
 - Iteratively validate system resource usage and framerate via Profiler.

## Developer Playtest Subphase v 0.4.0 upon completion

## Remote Playtest Subphase v.0.5.0 upon completion

#Distribution Preparation Phase v 0.6.0; designate Beta
- **Tasks**:
 1. Maintain licensing spreadsheet for assets.
 2. Create `LICENSE.md` or `CREDITS.md` in repo.
 3. Test export presets for Windows.
 4. Clean Git repo pre-release (merge branches, delete tests).
- **My Role**:
 1. Draft licensing templates.
 2. Guide export setup and platform distribution.

#Beta Distribution
