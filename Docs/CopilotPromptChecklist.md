# Copilot Prompt Review Template & Checklist

## Prompt Metadata
- **Scene/Feature Name:**  
- **Related Scene File (.tscn):**  
- **Relevant Namespace:** `WarEaglesDigital.Scripts`
- **Date:**  
- **Reviewed by:**  

---

## Prompt Content

### 1. Pseudocode Structure
- [ ] Is the prompt written in clear, structured pseudocode?
- [ ] Are placeholders used for any missing or user-supplied data (never assumed)?

### 2. Namespace & Node Paths
- [ ] Does the prompt specify the `WarEaglesDigital.Scripts` namespace?
- [ ] Are all node paths accurate and match the scene tree?
- [ ] Are node names in PascalCase as per guidelines?

### 3. Signals & Connections
- [ ] Are all required signals listed and described?
- [ ] Are signal connections specified (including sender and receiver nodes)?

### 4. Error Handling & Robustness
- [ ] Are try-catch blocks or error-handling strategies described for risky operations?
- [ ] Are null checks or validation steps included where needed?

### 5. Resource Management
- [ ] Are resource loading/unloading and cleanup steps described?
- [ ] Are best practices for memory and asset management included?

### 6. Coding Standards
- [ ] Are naming conventions (PascalCase for nodes/methods, _camelCase for instances, lower_case for assets) followed?
- [ ] Is the prompt verbose enough to explain the function of each code section?

### 7. Testing & Debugging
- [ ] Are test cases or expected behaviors described?
- [ ] Are debugging/logging steps (e.g., GD.Print) included where useful?

### 8. Documentation & Comments
- [ ] Are comments or docstrings suggested for complex logic?
- [ ] Is there a note to update `docs/user_guide.md` if needed?

---

## Final Review
- [ ] Does the prompt avoid plain language and use only pseudocode?
- [ ] Is the prompt concise but complete, with no ambiguous instructions?
- [ ] Are all required files, assets, and dependencies referenced or placeholdered?

---

**Reviewer Notes:**  
(Add any observations, questions, or clarifications needed before submitting to Copilot.)

---
