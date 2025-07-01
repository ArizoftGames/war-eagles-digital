# War Eagles Digital Workflow v5

## Version Control and Backup Strategy
- **Tasks**:
  1. Commit and push changes to GitHub daily (end of each coding day) using Visual Studio Git Changes.
  2. Manually copy war-eagles-digital directory to portable drive weekly, each Monday, using calendar reminder.
  3. Create task-specific branches (e.g., `dev` for coding, `asset-dev` for assets, `terrain-test` for Terrain3D, `audio-bus-integration` for audio tasks) for modularity.
  4. Clean up branches post-testing or pre-distribution:
     - Merge successful branches (e.g., `audio-bus-integration`) into `main` or `dev` and delete: `git branch -d <branch-name>`, `git push origin --delete <branch-name>`.
     - Delete failed/abandoned branches after documenting findings in `docs/user_guide.md`.
- **My Role**:
  1. Provide Git commands (e.g., `git checkout -b workflow-v5-update`, `git commit -m "Updated workflow to v5"`).
  2. Suggest commit messages, branching strategies, and cleanup steps.
  3. Guide conflict resolution during merges if files are provided.

## Asset Creation Phase
- **Status**: Completed.

## Animations Phase
- **Status**: Completed.

## Coding Phase
- **Status**: Active.
- **Tasks**:
  1. **Credit Roll**:
     - Create `Credits.tscn` with a scrolling `Label` or `RichTextLabel` for credits, triggered by a button in `PauseMenu.tscn` and splash menu.
     - Implement logic to return to `PauseMenu.tscn` using signals or scene transitions.
     - Source credits from a text file (e.g., `res://docs/credits.txt`) for easy updates.
     - Use `AudioManager.cs` to play background music (e.g., `CreditsTheme.ogg` from `Music.csv`).
     - **Discussion**: A credit roll enhances professional presentation, a key factor in player retention per Godot 4.4 documentation (godotengine.org). Using a `RichTextLabel` with `AnimationPlayer` for scrolling ensures smooth visuals, while sourcing text from a file simplifies maintenance. Integrating with `PauseMenu.tscn` leverages existing UI, and `AudioManager`’s bus system (`res://Audio/WE_Bus_layout.tres`) supports seamless audio, aligning with the project’s focus on polish and compatibility.

  2. **Extras Folder**:
     - Create `res://extras/` directory for assets like concept art (.png), lore (.txt), or audio commentary (.ogg).
     - Develop `ExtrasMenu.tscn` with a `GridContainer` or `ItemList`, accessible via `PauseMenu.tscn` and splash menu buttons.
     - Implement return logic to `PauseMenu.tscn`.
     - **Discussion**: An extras folder adds player engagement by showcasing behind-the-scenes content, as noted in itch.io devlogs (itch.io). A dedicated `ExtrasMenu.tscn` with a `GridContainer` provides a clean interface for browsing assets, and clear file paths (e.g., `res://extras/concept_art/`) prevent loading errors. Integration with `PauseMenu.tscn` ensures consistent navigation, supporting the project’s UX goals.

  3. **Options Menu**:
     - Create `OptionsMenu.tscn` with sliders for `Master` and combined `Planes`/`Effects` volumes, interfacing with `AudioManager.cs`’s `SetBusVolume` and `SetEffectsVolume`.
     - Add a button in `PauseMenu.tscn` and splash menu to load `OptionsMenu.tscn`, with return logic.
     - Plan for future settings (e.g., graphics, controls).
     - **Discussion**: An options menu is critical for player customization, as emphasized in GameDeveloper.com UX articles. Sliders controlling `AudioManager`’s bus volumes (`res://Audio/WE_Bus_layout.tres`) ensure robust audio management, while a scalable design accommodates future settings. Linking to `PauseMenu.tscn` maintains navigation consistency, aligning with Godot’s modular scene system for easy testing.

  4. **Help System Presentation**:
     - Create `HelpMenu.tscn` with a `Tree` or tabbed interface for gameplay, controls, and context (e.g., squadron roles from `designations.csv`).
     - Add tooltips and images for clarity, accessible via `PauseMenu.tscn` and splash menu buttons.
     - Implement return logic to `PauseMenu.tscn`.
     - **Discussion**: A clear help system reduces player frustration in strategy games, per “Game Feel” by Steve Swink. Using `designations.csv` for context (e.g., squadron roles) and tooltips enhances accessibility. A `Tree`-based `HelpMenu.tscn` supports structured content, and integration with `PauseMenu.tscn` ensures seamless navigation, leveraging Godot’s UI tools for professional presentation.

  5. **Plan Gameplay Coding**:
     - Outline phase-by-phase mechanics (Event, Launch, Action) for C# scripts (e.g., `EventManager.cs`, `LaunchManager.cs`).
     - Plan event purchases and effects using `events.csv`.
     - Plan auto-assignment of unit designations from `designations.csv` with tooltip support.
     - Include CAP audio using `AudioStreamPlayer3D` on `AirUnit` nodes for fighters/fighter-bombers, playing `[Motor]Level.ogg` (e.g., `RadialLevel.ogg`).
     - **Discussion**: Planning gameplay coding ensures modular, testable mechanics, as recommended in Godot forums (godotdevelopers.org). A detailed outline for Event, Launch, and Action phases, leveraging `events.csv` and `designations.csv`, supports data-driven design. Including CAP audio planning ensures integration with `AudioStreamPlayer3D` toggling during CAP/Landing phases, maintaining performance (60 FPS target) and compatibility with `AudioManager.cs`.

  6. **Plan State Machine Build and AI Behavior**:
     - Outline opponent AI using decision trees in `OpponentAI.cs`, integrating with gameplay phases.
     - Plan AI-driven event purchases and unit deployments, referencing `events.csv` and `designations.csv`.
     - Ensure AI states align with Standard Rules and gameplay plan for balance.
     - **Discussion**: Planning a state machine with decision trees ensures predictable AI behavior, as noted in Gamasutra AI articles. Outlining `OpponentAI.cs` to integrate with gameplay phases and data (`events.csv`, `designations.csv`) supports modularity. Aligning with the gameplay coding plan ensures AI states complement player mechanics, maintaining balance and scalability for 40 aircraft and 20 AA units.

- **My Role**:
  1. Provide C# script outlines (e.g., `Credits.tscn.cs`, `OptionsMenu.cs`, `EventManager.cs`, `OpponentAI.cs`) in `WarEaglesDigital.Scripts`.
  2. Suggest `AudioStreamPlayer3D` settings (e.g., `DopplerTracking = Enabled`) and UI tooltip text.
  3. Draft file structures (e.g., `res://extras/`) and scene transitions for `PauseMenu.tscn`.
  4. Guide debugging with Godot’s profiler and NUnit tests.
  5. Assist with gameplay and AI planning, ensuring alignment.

## Testing and Debugging Plan
- **Tasks**:
  1. Test UI scenes (`Credits.tscn`, `OptionsMenu.tscn`, `HelpMenu.tscn`) for navigation and functionality.
  2. Verify CAP audio (`AudioStreamPlayer3D` on `AirUnit` nodes) with Doppler effects in CAP/Landing phases.
  3. Add NUnit tests for key methods (e.g., `AudioManager.SetBusVolume`, `EventManager` effects).
  4. Playtest gameplay phases (e.g., raid on Berlin) in Godot editor.
  5. Monitor 60 FPS target with 40 aircraft, 10 zones, 20 AA units using profiler.
- **My Role**:
  1. Provide unit test examples and playtest scenarios.
  2. Guide bug tracking and profiler usage.
  3. Assist with export preset testing.

## Performance Optimization
- **Tasks**:
  1. Optimize `AudioStreamPlayer3D` usage (e.g., disable when not in CAP/Landing).
  2. Use LODs for aircraft (10K/2K polys) and zones (20K polys).
  3. Compress textures (.png with S3TC, .dds with DXT5/BC7).
  4. Test performance with profiler, targeting 60 FPS.
- **My Role**:
  1. Suggest optimization strategies (e.g., audio player pooling).
  2. Provide FPS logging code.

## Documentation and User Guide
- **Tasks**:
  1. Add tooltips to UI (e.g., “Adjust Master Volume”).
  2. Update `docs/user_guide.md` weekly with gameplay, controls, and audio notes.
  3. Store drafts in Git (e.g., `docs/v0.1/user_guide.md`).
- **My Role**:
  1. Draft user guide template and tooltip text.
  2. Guide note structuring.

## Distribution Preparation
- **Tasks**:
  1. Maintain licensing spreadsheet for assets (.ogg audio, models).
  2. Create `LICENSE.md` or `CREDITS.md` in repo.
  3. Test export presets (Windows, Android).
  4. Clean Git repo pre-release (merge branches, delete tests).
- **My Role**:
  1. Draft licensing templates.
  2. Guide export setup and platform distribution.

## Next Steps for Your Project
- **Timeline**:
  - Today: Implement `Credits.tscn` with `PauseMenu.tscn` integration.
  - Next 1–2 weeks: Develop `ExtrasMenu.tscn`, `OptionsMenu.tscn`, `HelpMenu.tscn`.
  - Ongoing: Plan gameplay and AI state machine, update `docs/user_guide.md`.
- **My Support**:
  - Provide C# scripts for UI and planning outlines.
  - Assist with scene transitions, gameplay/AI planning, and documentation.