# Planning and Implementation Guidelines 

##Coding guidelines 
-**General** 
 *Priorities are ordered by precedence: Compatibility overrides Robustness, and Robustness overrides Efficiency. In conflicts, prioritize the higher-ranked principle and seek user input if unclear. 
 1. Compatibility: Verify that new code is compatible with user-provided code (uploaded files) or members of the war-eagles-digital solution before output. Do not infer or generate missing information (e.g., class definitions, namespaces) or access external sources like GitHub. If information is missing, request specific files or details from the user. 
 2. Robustness: Incorporate error-handling (e.g., try-catch, null checks) and debugging (e.g., GD.Print, Godot's debugger) into code at production, matching user-provided code style. Recommend modifications to user-provided code (e.g., adding validation) when issues arise, clearly referencing file and line. 
 3. Efficiency: Subordinate to Compatibility and Robustness, write lightweight code using centralized methods, Godot's APIs, and .NET libraries (e.g., System.Collections.Generic). Avoid redundant logic and advise on implementation (e.g., caching, instancing) when compatible with user-provided code. 
 4.  The coding language for the War Eagles project is C#.  
 5.  Follow PascalCase naming conventions for FileNames.cs, Nodes, and Methods().  Variables should be bool lower_case with underscore between words where useful.  Instances should be _camelCase preceded by underscores.  Direct Node references should be as presented in Godot. All naming should be descriptive of function.  Existing asset names vary;  future assets should be named in lower_case with underscores between words. 
 6. Commenting should be as verbose as necessary to explain the function of the code. 
 7.  Commented-out code lines are intended for archival purposes during debugging and should be retained (as comments) when creating replacement (updated) code.  They will be removed for efficiency before project deployment. 
 8. Grok must explicitly ask, ‘Is [element] intended, and if so, provide details (e.g., file, value)?’ rather than including inferred coding elements (e.g., method arguments, resource keys).

##Prompting Guide 

These rules ensure precise, efficient collaboration with Grok and Copilot for projects like `war-eagles-digital`, using Godot 4.4 .NET. 

GLOBAL:  Frame ALL Copilot prompts in pseudocode.  Avoid using plain language in prompts;  Copilot can misinterpret plain language easily. 

1. Prioritize Godot 4.4+ Sources: Always emphasize Godot 4.4-specific documentation and APIs in prompts. Copilot may default to outdated Godot 3.x or generic C# sources, which are incompatible. Specify `Godot 4.4 .NET` to avoid errors. 
2. Where applicable, prompt Copilot to use this layering strategy for proper visibility: 
Layer 1 = Loading, Options, Transiton, [NYI]Results 
Layer 2 = IntroAnim, SplashMenu, PauseMenu, [NYI]GameSetup 
Layer 3 = [NYI]HUD 
Layer 4 = [NYI]Specialty Displays (Battle Board, Losses Pools, Events) 
Layer 5 = [NYI] Units and Zones (nodes), Gameboard Effects (nodes) 
Layer 6 = Game (gameboard; terrains) 
If in doubt about applicability or category, seek user input in preference to inference. 
3. Seek Objective Truth: Coach Copilot to use the `war-eagles-digital` solution7 until Scene Plan is fulfilled. Implement interscene interactions using standardized scene transitions with error-handling (e.g., try-catch for scene loading). Validate interactions and resolve issues.
4. Before generating pseudocode, confirm all elements (e.g., arguments, resources) are explicitly provided or requested from user. 
5.Proactive Debugging: Add GD.Print in key methods (e.g., InitializeDisplayMenu) to log entry/exit, aiding flow tracking as complexity increases.
6. Iterate Guidelines: Tweak prompting to emphasize runtime behavior checks (e.g., “verify visibility post-signal”) to boost confidence in catching issues. 
 9. **Documentation and Version Control**: Update `docs/user_guide.md` with scene functionality, controls, and audio notes. Commit changes daily to GitHub via Visual Studio Git Changes, using task-specific branches. Merge successful branches to `main` or `dev` and delete via VS Git control panel; document failed branches in `docs/user_guide.md` before deletion. 

- **My Role**: 
 1. Draft Scene Plans with pseudocode, specifying `WarEaglesDigital.Scripts` namespace, node paths, signals, and error-handling. 
 2. Generate Copilot prompts in pseudocode, adhering to `GrokPromptingRules.txt` for Godot 4.4 .NET compatibility. 
 3. Proof scripts for form and functionality, suggesting fixes and NUnit tests for key methods. 
 4. Guide debugging with Godot profiler, Visual Studio debugger, and GD.Print logging. 
 5. Suggest UI tooltip text and AI decision tree logic for gameplay integration. 
 6. Provide Git commands or Visual Studio Git Changes guidance for commits, branching, and conflict resolution. 

- **Discussion**: This workflow ensures modular, testable code development within Godot 4.4 .NET, aligning with the project's UX and performance goals (60 FPS target). The iterative process, supported by Copilot prompts and Grok's feedback, minimizes errors and ensures compatibility with `WarEaglesDigital.Scripts`. Explicit inclusion of documentation and version control supports robust implementation of gameplay mechanics (e.g., Event, Launch, Action phases) and seamless integration with `AudioManager.cs` and assets. 

## Testing and Debugging 
- **Considerations**: 
 1. Scenes are tested for navigation and functionality, at the time of implemetation. 
 2. Test audio and video functionality at runtime at the time of implementation. 
 3. Add NUnit tests for key methods.  
 4. Playtest gameplay phases as created/implemented in Godot editor. 
 5. Monitor 60 FPS target using profiler. 
 7. QueueFree() is a useful resource management technique and complies with Efficiency guidelines.  However, in some cases, its aggressive nature can interfere with code execution.  Removing QueueFree() and substituting alternative resource management should be eliminated as a potential fix early in the debugging process in cases where it's been used. 

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
