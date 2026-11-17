---
trigger: always_on
---

## Code Style & Formatting

- **Quotes**: Always use single quotes (`'`) for JavaScript strings, unless template literals (`` ` ``) are required for interpolation or multiline text.
- **Explicit Braces**: Always use curly braces `{ ... }` for all control flow statements (`if`, `else`, `for`, `while`, `do...while`), even for single-line bodies. Never omit braces.
- **Clean Syntax**: Keep code modular, preserve established JSDoc type annotations, and ensure clean formatting.
- **One Class Per File**: Always have one class per file.
- **Enums**: Use enums wherever possible rather than strings
- **Documentation**: Always document classes and methods, classes should explain what they are used for, and methods should explain what they are used for and the params should be documented to. Complex methods should contain comments explaining different parts of the method and what it is doing