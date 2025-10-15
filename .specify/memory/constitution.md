<!--
Sync Impact Report:
- Version change: 1.0.0 → 1.1.0
- Modified principles:
  * V. Git Commit Standards → Enhanced with mandatory per-task commit requirement
- Changes made:
  * Strengthened Principle V to require commits IMMEDIATELY after each task completion
  * Added explicit workflow: update tasks.md → mark task complete → commit all related changes
  * Clarified that task completion is NOT final until committed to git
  * Emphasized atomic commits aligned with task boundaries
- Templates requiring updates:
  * ✅ plan-template.md - No changes needed (constitution check already references this)
  * ✅ spec-template.md - No changes needed (user story driven, commit strategy is implementation detail)
  * ✅ tasks-template.md - Already mentions "Commit after each task or logical group" (line 246) - ALIGNED
  * ✅ commands/*.md - No agent-specific references
- Follow-up TODOs: None
- Rationale for MINOR version bump:
  * This is a materially expanded guidance on existing Principle V
  * Does not remove or redefine existing principles (not MAJOR)
  * Adds mandatory workflow requirement that affects development process (not just a clarification, so not PATCH)
  * Enhances governance of when commits must occur (from "when tasks complete" to "immediately after each task")
-->

# AccountBox Constitution

**Project**: AccountBox - 账号管理系统
**Type**: Web Application (Frontend + Backend)

## Core Principles

### I. Frontend Development Standards (MANDATORY)

**Framework & Runtime**:
- MUST use React with function components and Hooks pattern exclusively
- MUST use TypeScript with strict mode enabled
- MUST use Vite 5+ as build tool
- MUST use pnpm as package manager

**Code Conventions**:
- Component definition: MUST use function components with `const Component = () => {}` arrow function syntax
- Naming conventions:
  * Component names: PascalCase (e.g., `UserProfile.tsx`)
  * Utility functions: camelCase (e.g., `formatDate.ts`)
  * Constants: UPPER_SNAKE_CASE (e.g., `API_ENDPOINTS.ts`)
- Props: MUST define TypeScript interfaces for all component props

**Quality Tools**:
- Code formatting: MUST configure Prettier with pre-commit auto-formatting
- Linting: MUST use ESLint with React Hooks plugin
- Type checking: TypeScript strict mode MUST be enabled

**UI Standards**:
- UI Framework: MUST use shadcn/ui components (consult shadcn MCP for usage)
- CSS Framework: MUST use Tailwind CSS as primary styling solution

**Directory Structure**:
```
src/
├── components/     # Reusable components (Atomic Design)
├── features/       # Feature modules
├── hooks/          # Custom React Hooks
├── services/       # API services
├── stores/         # State management
├── types/          # TypeScript type definitions
├── utils/          # Utility functions
└── assets/         # Static resources
```

**Rationale**: Enforces modern React best practices, type safety, and maintainable project structure. Vite ensures fast development and optimized builds. Strict tooling prevents common errors and ensures code consistency.

---

### II. Backend Development Standards (MANDATORY)

**Technology Stack & Architecture**:
- Language & Runtime: MUST use .NET 10.0 or higher LTS version
- Architecture Style: MUST follow Clean Architecture or Layered Architecture principles with clear separation of concerns
- Core Frameworks:
  * Web API: MUST use ASP.NET Core
  * Data Access: MUST use Entity Framework Core with Code First approach
- Dependency Injection: MUST use built-in DI container following explicit dependency principle

**Code Style & Quality**:
- Naming: MUST follow Microsoft official naming guidelines. Interface names MUST start with capital `I`
- Formatting: MUST configure and use `.editorconfig` file. CI/CD MUST integrate `dotnet format` for enforcement
- Design Principles: MUST adhere to SOLID principles. Methods MUST NOT exceed 3 parameters; use parameter objects when necessary

**Rationale**: Ensures maintainable, testable architecture. Clean Architecture isolates business logic from infrastructure. SOLID principles prevent tightly coupled code. Parameter limits enforce single responsibility and reduce complexity.

---

### III. Code Quality & Testing (NON-NEGOTIABLE)

**API Design**:
- Return Format: All API endpoints MUST return unified `ApiResponse<T>` wrapper format
- Serialization: MUST use System.Text.Json as default JSON serializer
- Routing: Controllers and action methods MUST use convention-based or attribute routing

**Testing Requirements**:
- Testing Framework: MUST use xUnit for unit testing
- Test Coverage: Unit test coverage MUST reach minimum 85%
- Code Analysis: MUST enable SonarAnalyzer.CSharp for static analysis. NO Blocker or Critical level issues allowed

**Rationale**: 85% coverage threshold ensures critical paths are tested while avoiding diminishing returns. Unified API responses simplify client handling. Static analysis catches issues before runtime.

---

### IV. Security & Data Protection (MANDATORY)

**Input Validation**:
- All API inputs MUST use Data Annotations or FluentValidation for model validation

**Data Access**:
- All database operations MUST use asynchronous programming patterns
- MUST NOT use raw SQL string concatenation. MUST use parameterized queries or EF Core LINQ to prevent SQL injection

**Security Requirements**:
- Passwords MUST be hashed using BCrypt or Argon2id (Argon2id RECOMMENDED for key derivation scenarios). MUST NEVER store passwords in plaintext
- Sensitive configuration (connection strings, API Keys) MUST be managed through Azure Key Vault or environment variables. MUST NOT commit to code repository

**Rationale**: Password hashing with BCrypt or Argon2id protects against rainbow table attacks. Argon2id is the 2015 Password Hashing Competition winner and provides superior protection for key derivation use cases. Parameterized queries eliminate SQL injection vectors. Secret management prevents credential leakage in version control.

---

### V. Git Commit Standards (MANDATORY)

**Repository Rules**:
- `.git` directory files are PROHIBITED from modification

**Per-Task Commit Requirement** (MANDATORY):
- IMMEDIATELY after completing EACH task, the following workflow MUST be executed:
  1. Update `specs/[###-feature-name]/tasks.md` to mark the task as complete (change `[ ]` to `[x]`)
  2. Stage all related changes: `git add .`
  3. Commit with descriptive message following Conventional Commits format
  4. Task completion is NOT considered final until changes are committed to git

**Commit Convention**:
- MUST follow Conventional Commits format:
  ```
  <type>[optional scope]: <description>

  [optional body]

  [optional footer(s)]
  ```
- Valid types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`, `perf`, `ci`, `build`, `revert`
- Scope: optional but recommended (e.g., `[api]`, `[ui]`, `[auth]`, `[crypto]`, `[setup]`)
- Description: concise summary in imperative mood (e.g., "add feature" not "added feature")
- Body (optional): detailed explanation for complex changes
- Footer (optional): references to issues, breaking changes

**Commit Examples**:
- git commit的内容用中文
```bash
# After completing T001 (backend structure setup)
git add . && git commit -m "chore[setup]: create backend project structure

- Created AccountBox.Api, Core, Data, Security projects
- Organized under backend/src/ directory

Closes T001"

# After completing T007 (Argon2 implementation)
git add . && git commit -m "feat[crypto]: implement Argon2id key derivation service

- Added Argon2Service with configurable parameters
- Memory: 64MB, Iterations: 4, Parallelism: 2
- Supports key derivation for master password hashing

Closes T007"
```

**Rationale**:
- Per-task commits create atomic, traceable changes aligned with task boundaries
- Immediate commits prevent work loss and enable precise rollback if needed
- Conventional Commits enable automated changelog generation and semantic versioning
- Clear commit history allows easy identification of when/why changes were made
- Task-commit alignment enables accurate progress tracking and code archaeology

---

## Project Initialization Requirements

**Context7 MCP Integration**:
- During project initialization, MUST call `context7 mcp` to fetch up-to-date documentation for dependencies
- This ensures access to latest library documentation and best practices

**Rationale**: Reduces reliance on outdated documentation. Context7 provides contextual, version-specific guidance during development.

---

## Development Workflow

**Pre-Commit Checks**:
- Frontend: Prettier formatting, ESLint checks, TypeScript compilation
- Backend: `dotnet format` verification, build success, test pass

**Code Review Requirements**:
- All PRs MUST pass automated checks (linting, tests, coverage)
- All PRs MUST reference related issue or task number
- At least one approval required before merge

**CI/CD Quality Gates**:
- Build MUST succeed
- All tests MUST pass
- Code coverage MUST meet 85% threshold
- No Blocker or Critical SonarQube issues

**Rationale**: Automated checks catch issues early. Coverage gate ensures new code is tested. SonarQube prevents technical debt accumulation.

---

## Governance

**Constitution Authority**:
- This constitution supersedes all other development practices and guidelines
- All team members MUST adhere to principles outlined herein
- Deviations require explicit justification and approval

**Amendment Procedure**:
- Amendments MUST be proposed via pull request to `.specify/memory/constitution.md`
- Amendments require team consensus (all active contributors approve)
- Breaking changes (removing/redefining principles) increment MAJOR version
- New principles or expanded guidance increment MINOR version
- Clarifications and typo fixes increment PATCH version

**Compliance Review**:
- All code reviews MUST verify compliance with constitutional principles
- Non-compliant code MUST be rejected with reference to specific principle
- Complexity violations (e.g., >3 parameters) MUST be justified in PR description

**Version Management**:
- Version format: MAJOR.MINOR.PATCH (Semantic Versioning)
- Each amendment MUST update `Last Amended` date
- Version increments trigger updates to dependent templates (plan, spec, tasks)

**Runtime Guidance**:
- For development-time guidance (non-constitutional), refer to `CLAUDE.md` or agent-specific files
- Constitutional principles are architectural; runtime guidance is tactical

**Rationale**: Clear governance prevents erosion of standards over time. Version tracking enables historical analysis. Explicit compliance checks enforce accountability.

---

**Version**: 1.1.0 | **Ratified**: 2025-10-15 | **Last Amended**: 2025-10-15
