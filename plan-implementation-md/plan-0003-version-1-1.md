# Implementation Plan - Configuration & Settings Migration

Update the AuthMicroservice project to align with Section 7 of `micro_development_roll.md`. All custom configurations will be stored inside the `"Settings"` block in `appsettings.json`, mapped to a concrete `Settings` class implementing `ISettings`, and injected via Dependency Injection.

## Proposed Changes

### Core Component

#### [NEW] [ISettings.cs](file:///home/skegdeveloper/Documents/MGP-MICROSRV-AOT-NET10/AUTH/AuthMicroservice/Core/Interfaces/ISettings.cs)
- Create the `ISettings` interface defining:
  - `string ConnectionString { get; }`
  - `string TokenSecret { get; }`
  - `int TokenExpirationInMinutes { get; }`

#### [NEW] [Settings.cs](file:///home/skegdeveloper/Documents/MGP-MICROSRV-AOT-NET10/AUTH/AuthMicroservice/Core/Settings/Settings.cs)
- Create a concrete `Settings` class that implements `ISettings`.

---

### Infrastructure & Helpers

#### [MODIFY] [UserRepository.cs](file:///home/skegdeveloper/Documents/MGP-MICROSRV-AOT-NET10/AUTH/AuthMicroservice/Infrastructure/Repositories/UserRepository.cs)
- Replace `IConfiguration` injection with `ISettings` injection.
- Use `settings.ConnectionString` for SQLite connections.

#### [MODIFY] [JwtHelper.cs](file:///home/skegdeveloper/Documents/MGP-MICROSRV-AOT-NET10/AUTH/AuthMicroservice/Application/Helpers/JwtHelper.cs)
- Replace `IConfiguration` injection with `ISettings` injection.
- Use `settings.TokenSecret` for JWT signing and verification.
- Use `settings.TokenExpirationInMinutes` to determine the session token expiration (previously hardcoded to 15 minutes).

---

### Configuration & Main Entry Point

#### [MODIFY] [appsettings.json](file:///home/skegdeveloper/Documents/MGP-MICROSRV-AOT-NET10/AUTH/AuthMicroservice/appsettings.json)
- Add the parent `"Settings"` block with:
  - `"ConnectionString": "Data Source=auth.db"`
  - `"TokenSecret": "SuperSecretKeyForDevelopmentPurposesOnly123!"`
  - `"TokenExpirationInMinutes": 60`

#### [MODIFY] [Program.cs](file:///home/skegdeveloper/Documents/MGP-MICROSRV-AOT-NET10/AUTH/AuthMicroservice/Program.cs)
- Bind the `"Settings"` configuration section to the concrete `Settings` class.
- Register `ISettings` as a singleton in the DI container.

## Verification Plan

### Automated Build & Test
- Run `dotnet build` on the `AuthMicroservice` project to ensure AOT-compatible compilation succeeds.
