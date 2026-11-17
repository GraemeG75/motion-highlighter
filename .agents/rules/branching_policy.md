---
trigger: always_on
---

# Git Branching & Milestone Policy

- **Branch Source**: For every milestone, submilestone, or major feature, **ALWAYS** create a new branch in code with the source coming directly from the `main` branch. Never branch off unmerged or stale feature branches unless explicitly directed.
- **Branch Naming Convention**:
  - Milestone branches: `milestone-<number>-<slug>` (e.g., `milestone-14-conveyors-inserters`, `milestone-15-pipe-networks`).
  - Feature/Fix branches: `feat/<slug>`, `fix/<slug>`, or `task/<slug>`.
- **Standard Lifecycle**:
  1. Ensure local `main` is clean and up to date:
     ```bash
     git checkout main
     git pull origin main
     ```
  2. Create and switch to the dedicated milestone branch:
     ```bash
     git checkout -b milestone-<number>-<slug>
     ```
  3. Commit discrete changes atomically following `commit_policy.md`:
     `type(scope): description`
  4. Verify all acceptance criteria:
     - `dotnet build Factorius.slnx` builds cleanly with 0 errors and 0 new warnings.
     - `dotnet run --project tests/Factorius.Tests/Factorius.Tests.csproj` passes 100% of test specifications.
     - 4-language i18n parity confirmed across English (`en`), French (`fr`), German (`de`), and Spanish (`es`).
     - Coding style rules obeyed (explicit braces, enums, one class per file, full XML doc comments).
  5. Merge verified milestone back into `main` via pull request or fast-forward merge once complete.
