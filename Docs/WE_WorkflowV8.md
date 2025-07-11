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
  1. **Credit Roll**: Completed.

  2. **Extras Folder**: Completed. v 0.1.0
     
  3. **Options Menu**: v 0.1.1 upon completion
     - Create `OptionsMenu.tscn` with sliders for `Master` and combined `Planes`/`Effects` volumes, interfacing with `AudioManager.cs`’s `SetBusVolume` and `SetEffectsVolume`.
     - Add a button in `PauseMenu.tscn` and splash menu to load `OptionsMenu.tscn`, with return logic.
     - Plan for future settings (e.g., graphics, controls).
     - **Discussion**: An options menu is critical for player customization, as emphasized in GameDeveloper.com UX articles. Sliders controlling `AudioManager`’s bus volumes (`res://Audio/WE_Bus_layout.tres`) ensure robust audio management, while a scalable design accommodates future settings. Linking to `PauseMenu.tscn` maintains navigation consistency, aligning with Godot’s modular scene system for easy testing.

  4. **Help System Presentation**: v0.2.0 upon completion
     - Create `HelpMenu.tscn` with a `Tree` or tabbed interface for gameplay, controls, and context 
     - Add tooltips and images for clarity, accessible via `PauseMenu.tscn` and splash menu buttons.
     - Implement return logic to `PauseMenu.tscn`.
     - **Discussion**: A clear help system reduces player frustration in strategy games, per “Game Feel” by Steve Swink. Using tooltips enhances accessibility. 

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

## Coding Implementation Sub-phase v 0.3.0 upon completion

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