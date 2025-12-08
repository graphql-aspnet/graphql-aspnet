# CLAUDE.md

This file provides guidance to Claude Code when working in this repository.
All instructions in this document **must be followed exactly**.

---

## 1. Rule Precedence

When there is any conflict, the following precedence order MUST be respected:

1. **Compiler errors and warnings**
2. **StyleCop ruleset** (e.g. `styles.ruleset` or equivalent configuration)
3. **.editorconfig** settings
4. **Rules in this CLAUDE.md**

Claude must not generate code that violates higher-precedence rules in order to satisfy lower-precedence ones.

---

## 2. Code Style & Formatting

### 2.1 StyleCop ruleset is authoritative for analyzers

- Treat the configured **StyleCop ruleset** as the authoritative source for:
  - naming conventions
  - documentation requirements
  - ordering rules
  - spacing and layout rules that StyleCop checks
  - any other StyleCop analyzer rules
- Respect the **severity levels** defined in the ruleset (Error / Warning / Info / Hidden).
- Do **not**:
  - introduce new `#pragma warning disable` or `#pragma warning restore` directives,
  - add or change suppression attributes (e.g. `[SuppressMessage]`),
  - modify the ruleset to weaken or disable rules,
  unless explicitly instructed to do so.

When StyleCop enforces a pattern, Claude must follow that pattern even if it conflicts with generic preferences or defaults.

### 2.2 `.editorconfig` is the source of truth for formatting

- **Always enforce the formatting and style rules defined in `.editorconfig`.**
- Use `.editorconfig` for:
  - indentation
  - whitespace and spacing
  - brace layout
  - file and encoding settings
  - analyzer severities defined there
- If `.editorconfig` and this document conflict, **`.editorconfig` wins** (subject to the StyleCop precedence above).

### 2.3 Additional C# conventions (only where not covered above)

The following rules apply **only when not contradicted by the StyleCop ruleset or `.editorconfig`**:

- Use `is` and `is not` for null checks — never `== null` or `!= null`.
- Do not use Hungarian notation.
- Use explicit type names for object creation. For example:
  ```csharp
  MyService service = new MyService();

### 2.4 File Encoding Requirements

- All new source files **must be created with a UTF-8 Byte Order Mark (BOM)**.
- When generating files, Claude must ensure:
  - Encoding = UTF-8 with BOM  
  - No alternative encodings (ASCII, UTF-16, UTF-32, or UTF-8 without BOM) are used.
- Existing files should **retain** their current encoding unless explicitly instructed to convert them.