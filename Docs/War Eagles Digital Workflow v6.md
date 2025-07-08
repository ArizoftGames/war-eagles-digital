# War Eagles Digital Workflow v6

# Planning and Implementation Guidelines

##Coding guidelines
-**General**
  *Priorities are ordered by precedence: Compatibility overrides Robustness, and Robustness overrides Efficiency. In conflicts, prioritize the higher-ranked principle and seek user input if unclear.
  1. Compatibility: Verify that new code is compatible with user-provided code (uploaded files) or members of the war-eagles-digital solution before output. Do not infer or generate missing information (e.g., class definitions, namespaces) or access external sources like GitHub. If information is missing, request specific files or details from the user.
  2. Robustness: Incorporate error-handling (e.g., try-catch, null checks) and debugging (e.g., GD.Print, Godot’s debugger) into code at production, matching user-provided code style. Recommend modifications to user-provided code (e.g., adding validation) when issues arise, clearly referencing file and line.
  3. Efficiency: Subordinate to Compatibility and Robustness, write lightweight code using centralized methods, Godot’s APIs, and .NET libraries (e.g., System.Collections.Generic). Avoid redundant logic and advise on implementation (e.g., caching, instancing) when compatible with user-provided code.
  4.  Follow PascalCase naming conventions for FileNames.cs, Nodes, and Methods().  Variables should be bool lower_case with underscore between words where useful.  Instances should be _camelCase preceded by underscores.  Direct Node references should be as presented in Godot. All naming should be descriptive of function.  Existing asset names vary;  future assets should be named in lower_case with underscores between words.
  5. Commenting should be as verbose as necessary to explain the function of the code.
 
  

## Coding Workflow
- **Tasks**:
  1. **Scene Planning**: Collaborate with Grok to define scene goals, objectives, and solutions. Specify required assets. Grok drafts a Scene Plan with high-level pseudocode, outlining key logic, `Node3D` nodes, signals, and error-handling within the `WarEaglesDigital.Scripts` namespace.
  2. **Scene Creation**: Create scene(s) in Godot 4.4 .NET editor, populating scene tree(s) with PascalCasing for node names. Source assets and create `.tres` files. Attach C# scripts in `WarEaglesDigital.Scripts` namespace.
  3. **Copilot Prompt Development**: Provide Grok with Scene Plan and raw `.tscn` text output from Godot. Upon consensus that the scene meets the Scene Plan, Grok generates a Copilot prompt in pseudocode, specifying `WarEaglesDigital.Scripts` namespace, node paths, signals, and error-handling. Ensure that prompts specify good resource amangement techniques where appropriate.
  4. **Prompt Review and Code Generation**: Review/edit Copilot prompt, verifying `WarEaglesDigital.Scripts` namespace, node paths, signal connections, and error-handling. Input prompt to Copilot, checking output for errors (e.g., truncation, missing functionality) using checklist:
     - Confirm `WarEaglesDigital.Scripts` namespace usage.
     - Validate node paths against scene tree.
     - Ensure signal connections are defined.
     - Add try-catch blocks if missing.
  5. **Script Proofing and Feedback**: Upload Copilot-generated script to Grok for proofing. Grok reviews for form and functionality (e.g., namespace, signals, error-handling) using a template and provides structured feedback with suggested fixes, including NUnit test suggestions for key methods.
  6. **Script Integration and Debugging**: Apply script changes in Visual Studio, resolving IntelliSense errors using GD.Print and Visual Studio debugger. Utilize Grok and Copilot as appropriate to aid debugging.
  7. **Scene Testing**: Compile and run scene in Godot 4.4, using profiler to monitor 60 FPS target. Test functionality with predefined test cases from Scene Plan. Debug compilation/runtime errors with Grok and Copilot, ensuring inputs yield expected results.
  8. **Iteration and Integration**: Repeat Steps 3–7 until Scene Plan is fulfilled. Implement interscene interactions using standardized scene transitions with error-handling (e.g., try-catch for scene loading). Validate interactions and resolve issues.
  9. **Documentation and Version Control**: Update `docs/user_guide.md` with scene functionality, controls, and audio notes. Commit changes daily to GitHub via Visual Studio Git Changes, using task-specific branches. Merge successful branches to `main` or `dev` and delete via VS Git control panel; document failed branches in `docs/user_guide.md` before deletion.

- **My Role**:
  1. Draft Scene Plans with pseudocode, specifying `WarEaglesDigital.Scripts` namespace, node paths, signals, and error-handling.
  2. Generate Copilot prompts in pseudocode, adhering to `GrokPromptingRules.txt` for Godot 4.4 .NET compatibility.
  3. Proof scripts for form and functionality, suggesting fixes and NUnit tests for key methods.
  4. Guide debugging with Godot profiler, Visual Studio debugger, and GD.Print logging.
  5. Suggest UI tooltip text and AI decision tree logic for gameplay integration.
  6. Provide Git commands or Visual Studio Git Changes guidance for commits, branching, and conflict resolution.

- **Discussion**: This workflow ensures modular, testable code development within Godot 4.4 .NET, aligning with the project’s UX and performance goals (60 FPS target). The iterative process, supported by Copilot prompts and Grok’s feedback, minimizes errors and ensures compatibility with `WarEaglesDigital.Scripts`. Explicit inclusion of documentation and version control supports robust implementation of gameplay mechanics (e.g., Event, Launch, Action phases) and seamless integration with `AudioManager.cs` and assets.

## Testing and Debugging
- **Considerations**:
  1. Scenes are tested for navigation and functionality, at the time of implemetation.
  2. Test audio and video functionality at runtime at the time of implementation.
  3. Add NUnit tests for key methods. 
  4. Playtest gameplay phases as created/implemented in Godot editor.
  5. Monitor 60 FPS target with 40 aircraft, 10 zones, 20 AA units using profiler.
- **My Role**:
  1. Provide unit test examples and playtest scenarios.
  2. Guide bug tracking and profiler usage.
  3. Assist with export preset testing.

## Performance Optimization
- **Tasks**:
  1. Optimize `AudioStreamPlayer3D` and AnimationPlayer/AnimationTree usage.
  2. Compress textures (.png with S3TC, .dds with DXT5/BC7) when created.
  3. Test performance with profiler, targeting 60 FPS. 
- **My Role**:
  1. Suggest optimization strategies.
  2. Provide FPS logging code.

## Documentation and User Guide
- **Tasks**:
  1. Add tooltips to UI and gameplay assets/elements while coding node functions.
  2. Update `docs/user_guide.md` at the conclusion of each scene or task with gameplay, controls, and audio notes.
  3. Store drafts in Git.
- **My Role**:
  1. Draft user guide template and tooltip text.
  2. Guide note structuring.

## Version Control and Backup Strategy
- **Tasks**:
  1. Commit and push changes to GitHub daily (end of each coding day) using Visual Studio Git Changes.
  2. Manually copy war-eagles-digital directory to portable drive weekly, each Monday, using calendar reminder.
  3. Create task-specific branches for modularity.
  4. Clean up branches post-testing or pre-distribution:
     - Merge successful branches into `main` or `dev` and delete using VS Git control panel.  Fall back to git console commands only if VS controls fail.
     - Delete failed/abandoned branches after documenting findings in `docs/user_guide.md`.
- **My Role**:
  1. Provide Git commands or VS operations guidance as needed.
  2. Suggest commit messages, branching strategies, and cleanup steps.
  3. Guide conflict resolution during merges if files are provided.

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

  2. **Extras Folder**:
     - Create `res://extras/` directory for assets (complete).
     - Develop `ExtrasMenu.tscn` with a `GridContainer` or `ItemList`.
     - Implement return logic to `PauseMenu.tscn`.
     - **Discussion**: An extras folder adds player engagement by showcasing behind-the-scenes content, as noted in itch.io devlogs (itch.io). A dedicated `ExtrasMenu.tscn` with a `GridContainer` provides a clean interface for browsing assets, and clear file paths prevent loading errors. Integration with `PauseMenu.tscn` ensures consistent navigation, supporting the project’s UX goals.

  3. **Options Menu**:
     - Create `OptionsMenu.tscn` with sliders for `Master` and combined `Planes`/`Effects` volumes, interfacing with `AudioManager.cs`’s `SetBusVolume` and `SetEffectsVolume`.
     - Add a button in `PauseMenu.tscn` and splash menu to load `OptionsMenu.tscn`, with return logic.
     - Plan for future settings (e.g., graphics, controls).
     - **Discussion**: An options menu is critical for player customization, as emphasized in GameDeveloper.com UX articles. Sliders controlling `AudioManager`’s bus volumes (`res://Audio/WE_Bus_layout.tres`) ensure robust audio management, while a scalable design accommodates future settings. Linking to `PauseMenu.tscn` maintains navigation consistency, aligning with Godot’s modular scene system for easy testing.

  4. **Help System Presentation**:
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

## Developer Playtest Subphase

## External Playtest Subphase

#Distribution Preparation Phase
- **Tasks**:
  1. Maintain licensing spreadsheet for assets.
  2. Create `LICENSE.md` or `CREDITS.md` in repo.
  3. Test export presets for Windows.
  4. Clean Git repo pre-release (merge branches, delete tests).
- **My Role**:
  1. Draft licensing templates.
  2. Guide export setup and platform distribution.

#Beta Distribution