# .NET 10 Native AOT Auth Microservice

This plan outlines the creation of a new .NET 10 Native AOT microservice in the current directory, adhering strictly to the architectural guidelines provided in `micro_development_roll.md` and implementing the Signup, Signin, and Middleware flows detailed in `spect-0001-init-version.md`.

## User Review Required

> [!IMPORTANT]
> The architectural rules require strict layer isolation (Controller -> Application -> Core <- Infrastructure) and Native AOT compatibility (using `JsonSerializerContext` for DTOs and no ORMs like Entity Framework or Dapper).

> [!WARNING]
> Since this is a .NET 10 Native AOT project, reflection-based validation libraries (like FluentValidation) are typically problematic unless used carefully. I plan to implement custom lightweight validators or use built-in Native AOT friendly validation patterns in the Middleware to ensure AOT compatibility.

## Open Questions

> [!CAUTION]
> 1. **Project Name:** I will create the project named `AuthMicroservice` directly in the current directory. Is this name acceptable?
> 2. **Database Initialization:** Should the microservice automatically create the SQLite database file and seed the tables (`userStatus`, `user`, `userSession`, etc.) upon startup if they do not exist, or will this be handled externally?
> 3. **Validation Error Array format:** The spec says "forming an array like VAL001 | VAL002 | VAL000N". Does this mean a list of strings, or an array of error objects as defined in the `StandardResponseDTO` structure? I will assume the latter to maintain consistency.

## Proposed Changes

### Project Initialization
- Create a new .NET 10 Web API project configured for Native AOT.
- Install necessary packages (e.g., `Microsoft.Data.Sqlite`).

---

### Core Layer (AOT Optimized)
This layer will contain interfaces, DTOs, and Dictionaries. No external dependencies allowed.

#### [NEW] `Core/DTOs/Requests/SignupRequestDTO.cs`
#### [NEW] `Core/DTOs/Requests/SigninRequestDTO.cs`
#### [NEW] `Core/DTOs/Responses/StandardResponseDTO.cs`
#### [NEW] `Core/DTOs/Responses/AuthResponseDTO.cs`
#### [NEW] `Core/Dictionaries/MessageDictionary.cs`
Contains mapping for `AUTH001`, `AUTH002`, `VAL001`, `OK001`, `OK002`, etc.
#### [NEW] `Core/Interfaces/IUserRepository.cs`
#### [NEW] `Core/Interfaces/IAuthService.cs`
#### [NEW] `Core/Interfaces/ICryptoHelper.cs`
#### [NEW] `Core/Interfaces/IJwtHelper.cs`
#### [NEW] `Core/Serialization/AppJsonSerializerContext.cs`
Partial class extending `JsonSerializerContext` for AOT JSON serialization of all DTOs.

---

### Infrastructure Layer
Data access using raw ADO.NET and SQLite.

#### [NEW] `Infrastructure/Repositories/UserRepository.cs`
Implements `IUserRepository`. Contains fully parameterized hardcoded SQL queries. 

---

### Application Layer
Business logic (Helpers) and Orchestration (Services).

#### [NEW] `Application/Helpers/CryptoHelper.cs`
Implements password hashing and verification.
#### [NEW] `Application/Helpers/JwtHelper.cs`
Implements JWT and Refresh Token generation.
#### [NEW] `Application/Services/AuthService.cs`
Orchestrates `Signup` and `Signin` flows. Contains `try/catch` blocks and throws custom exceptions.

---

### Controller Layer (Minimal APIs)

#### [NEW] `Controllers/AuthEndpoints.cs`
Defines `MapPost("/signup")` and `MapPost("/signin")` calling the `AuthService`.

---

### Middleware

#### [NEW] `Middleware/ExceptionHandlingMiddleware.cs`
Catches custom and system exceptions, formatting them into the `StandardResponseDTO`.
#### [NEW] `Middleware/ValidationMiddleware.cs`
Intercepts requests, validates Data Annotations/rules, and returns `StandardResponseDTO` with validation error codes if invalid.

---

## Verification Plan

### Automated Tests
* We will verify the build process for AOT compatibility (`dotnet publish -c Release`).
* We will not be implementing unit tests unless specifically requested, but the code will be structured to be highly testable.

### Manual Verification
* Run the API locally using `dotnet run`.
* Send an HTTP POST to `/signup` with valid and invalid payloads to test the Signup flow and Middleware validation.
* Send an HTTP POST to `/signin` with correct and incorrect credentials to test the Signin flow and error handling.
* Verify SQLite database entries are correctly created.
