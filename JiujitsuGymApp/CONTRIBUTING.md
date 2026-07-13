# Contributing

## Purpose
This file documents repository-level conventions and guidelines contributors must follow. It ensures consistent code style, tooling and file organization across the project.

## Development Setup
1. **Server:** requires the .NET SDK. From the repository root:
   - `dotnet build` — build the app
   - `dotnet run` — run the app locally
   - `dotnet ef database update` — apply pending EF Core migrations
2. **Client:** requires Node.js. From the `ClientApp/` folder:
   - `npm install` — install dependencies
   - `npm run dev` — start the Vite dev server
   - `npm run build` — production build

## Branching & Commits
- Do not commit directly to `master` for non-trivial changes; use a feature branch and open a pull request.
- Commit messages follow the [Conventional Commits](https://www.conventionalcommits.org/) style used in this repo:
  - Format: `<type>(<scope>): <summary>` — e.g. `feat(user): Add UserService for user CRUD operations`, `refactor(admin): Use UserService in AdminController`.
  - Common types: `feat`, `fix`, `refactor`, `docs`, `test`, `chore`.
- Keep commits focused: one logical change per commit.

## Server-Side Conventions (C#)
- **Thin controllers:** business logic (validation, entity creation/updates, role management) belongs in service classes under `Services/`, not in controllers. Controllers orchestrate: bind input, call a service, translate the result into an HTTP response.
- Register services for dependency injection in `Program.cs`.
- Use `ILogger<T>` for logging; log significant admin/user actions with structured parameters (e.g. `_logger.LogInformation("Admin created new user: {Email}", dto.Email)`).
- Database access goes through `ApplicationDbContext` (EF Core). Schema changes must be made via migrations (`dotnet ef migrations add <Name>`), never by hand-editing the database.

## DTOs (Data Transfer Objects)
- All data transferred between the server and client MUST use dedicated DTO classes located in the `Dtos/` folder.
- Controllers must NEVER return anonymous objects or domain/identity models (e.g. `User`, `IdentityUser`) directly to the client.
- **Input DTOs** (e.g. `CreateUserDto`) are used for incoming request bodies and MUST include Data Annotations for validation (e.g. `[Required]`, `[EmailAddress]`, `[StringLength]`).
- **Output DTOs** (e.g. `UserDto`) are the sole JSON contract for API responses and view models passed to Razor views.
- DTO class names should follow the pattern: `<Entity>Dto` for output and `Create<Entity>Dto` / `Update<Entity>Dto` for input.
- Mapping from domain model to DTO must happen inside a service or a dedicated mapping layer — never leak domain models across the boundary.

## JavaScript preference for ClientApp
- Client-side UI components and related files under `ClientApp/src/` SHOULD be authored in plain JavaScript (`.js`) rather than TypeScript (`.ts`).
- Existing TypeScript files may remain while migration is in progress; new components must be created using `.js` files.
- Rationale: team's hiring and code-review expectations prioritize JavaScript familiarity.

## File & Folder Conventions
- UI components: `ClientApp/src/components/<feature>/<component>.js`
- Shared components: `ClientApp/src/components/shared/<component>.js`
- Types (if retained): `ClientApp/src/types/` may keep `.ts` type declarations temporarily but new code should avoid relying on them.
- Entry point: `ClientApp/src/main.js`.

## Build & Tooling
- The client is built with Vite (`ClientApp/vite.config.js`); ensure it accepts `.js` files in `ClientApp/src/`.
- TypeScript tooling is configured to allow JavaScript during migration (`allowJs` in `tsconfig.json`).
- ESLint and Prettier are not yet configured. When adding them, use `eslint:recommended` as the baseline and Prettier for formatting; wire them up as `npm run lint` / `npm run format` scripts so they can be enforced in CI.

## Testing
- There is no test project yet. When adding server-side tests, use C# with xUnit in a dedicated test project.
- Client-side unit tests may be added with Jest/Testing Library but are optional.
- Accessibility tests and end-to-end tests using Playwright are encouraged and unaffected by JS vs TS choice.
- Add unit/integration tests when moving business logic out of controllers or components.

## Migration guidance (TS → JS)
- For each `.ts` file remaining in the `ClientApp/src/` folder:
  1. Convert to `.js` and remove TypeScript-only syntax (type annotations, interfaces, decorators typed usages where needed).
  2. Replace `export class X extends LitElement` with valid ES module export in JS if necessary.
  3. Update imports across the codebase to reference `.js` paths if your bundler resolves extensions differently.
  4. Use `allowJs` in `tsconfig.json` temporarily if the build requires it.

## Pull Request requirements
- Include a brief migration note if converting files from TS to JS.
- Ensure the solution builds (`dotnet build`) and the client builds (`npm run build`) before requesting review.
- Ensure lint/format checks pass in CI once that tooling is configured.
- Add unit/integration tests when moving business logic out of components.

## Exceptions
- Critical type-safety-heavy modules may remain in TypeScript if justified in a PR and agreed by maintainers.

## Contact
- For questions about the migration strategy or tooling updates, open an issue or contact the maintainers via the repository's issue tracker.
