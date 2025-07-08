# Planning and Implementation Guidelines

##Coding guidelines
-**General**
  *Priorities are ordered by precedence: Compatibility overrides Robustness, and Robustness overrides Efficiency. In conflicts, prioritize the higher-ranked principle and seek user input if unclear.
  1. Compatibility: Verify that new code is compatible with user-provided code (uploaded files) or members of the war-eagles-digital solution before output. Do not infer or generate missing information (e.g., class definitions, namespaces) or access external sources like GitHub. If information is missing, request specific files or details from the user.
  2. Robustness: Incorporate error-handling (e.g., try-catch, null checks) and debugging (e.g., GD.Print, Godot’s debugger) into code at production, matching user-provided code style. Recommend modifications to user-provided code (e.g., adding validation) when issues arise, clearly referencing file and line.
  3. Efficiency: Subordinate to Compatibility and Robustness, write lightweight code using centralized methods, Godot’s APIs, and .NET libraries (e.g., System.Collections.Generic). Avoid redundant logic and advise on implementation (e.g., caching, instancing) when compatible with user-provided code.
  4.  Follow PascalCase naming conventions for FileNames.cs, Nodes, and Methods().  Variables should be bool lower_case with underscore between words where useful.  Instances should be _camelCase preceded by underscores.  Direct Node references should be as presented in Godot. All naming should be descriptive of function.  Existing asset names vary;  future assets should be named in lower_case with underscores between words.
  5. Commenting should be as verbose as necessary to explain the function of the code.

##Prompting Guide

These rules ensure precise, efficient collaboration with Grok and Copilot for projects like `war-eagles-digital`, using Godot 4.4 .NET.

GLOBAL:  Frame ALL Copilot prompts in pseudocode.  Avoid using plain language in prompts;  Copilot can misinterpret plain language easily.

1. Prioritize Godot 4.4+ Sources: Always emphasize Godot 4.4-specific documentation and APIs in prompts. Copilot may default to outdated Godot 3.x or generic C# sources, which are incompatible. Specify `Godot 4.4 .NET` to avoid errors.
2. Seek Objective Truth: Coach Copilot to use the `war-eagles-digital` solution’s scripts and `WarEaglesDigital.Scripts` namespace accurately. Avoid interpolation; rely on provided code and user observations to ensure solutions match reality.
3.  Conserve Queries:  Compose Copilot prompts to accomplish as much as possible per prompt.  Balance with Copilot’s ability to provide quality code for large tasks.  Leverage Copilot’s ability to access the entire solution, and use clearly-marked placeholders RATHER THAN ASSUMED DATA where exact data hasn’t been provided, to allow the user to insert the data before inputting the prompt.
4. Support Hybrid Workflow: Favor programmatic solutions for dynamic logic and editor-based solutions for scene layout and tasks like animation. Prompt Grok to validate editor integration and maintain naming consistency.
5.  Encourage Precise Feedback: Request detailed user observations (e.g., animation timing, console logs) to compensate for limited runtime traces in Godot 4.4. Clarify vague terms to accelerate debugging.
6. Respect User Control: Adapt to user decisions and provide clear instructions for applying changes. Confirm results before proceeding to new tasks.
 
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
