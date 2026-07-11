# Plan 0004: Feature - Restore Code Flow (spect-0003)

## Overview

Two new POST endpoints under `/api/v1/auth/` for password recovery:

1. **Create Restore Code** — generates a 4-char alphanumeric code (2 letters + 2 digits) and returns an email DTO
2. **Validate Restore Code** — validates code existence, expiry (15min), marks as used

---

## New files (6)

| File | Purpose |
|------|---------|
| `Core/DTOs/Requests/CreateRestoreCodeRequestDTO.cs` | `UserEmail` (required), `UserIsoCountry` (required, max 3) |
| `Core/DTOs/Requests/ValidateRestoreCodeRequestDTO.cs` | `UserEmail`, `UserIsoCountry`, `RestoreCode` (all required) |
| `Core/DTOs/Responses/EmailDTO.cs` | Generic: `EmailSend`, `EmailContent<T>`, `EmailToSend`, `TemplateID` |
| `Core/DTOs/Responses/CreateRestoreCodeResponseDTO.cs` | Wraps `EmailDTO<string>` in `Email` property |
| `Core/Interfaces/IRestoreCodeHelper.cs` | `string GenerateRestoreCode()` |
| `Application/Helpers/RestoreCodeHelper.cs` | 2 random uppercase letters + 2 random digits, shuffled |

## Modified files (11)

| File | Change |
|------|--------|
| `Core/Dictionaries/MessageDictionary.cs` | Add `AUTH006`, `AUTH007`, `OK005`, `VAL009`, `VAL010` |
| `Core/Interfaces/ISettings.cs` | Add `string EmailTemplateIdRestore { get; }` |
| `Core/Settings/Settings.cs` | Add property |
| `Core/Interfaces/IUserRepository.cs` | +4 methods |
| `Core/Interfaces/IAuthService.cs` | +2 methods |
| `Core/Serialization/AppJsonSerializerContext.cs` | Register 5 new types |
| `Infrastructure/Repositories/UserRepository.cs` | Implement 4 new repo methods |
| `Application/Services/AuthService.cs` | Implement both service methods |
| `Controllers/AuthEndpoints.cs` | Add 2 `MapPost` routes |
| `Program.cs` | Register `IRestoreCodeHelper` → `RestoreCodeHelper` in DI |
| `appsettings.json` | Add `"EmailTemplateIdRestore": "emailRestore"` under `Settings` |

## New dictionary codes

```
AUTH006 → "restore code not exist please try again"
AUTH007 → "restore code are caducated please try again"
OK005   → "you're code are validated successfull you will complete restore password process"
VAL009  → "Email is required"
VAL010  → "Restore Code is required"
```

## Validation annotations

- `CreateRestoreCodeRequestDTO`: `UserEmail` ([Required=VAL009]), `UserIsoCountry` ([Required=VAL007], [MaxLength(3)=VAL008])
- `ValidateRestoreCodeRequestDTO`: `UserEmail` ([Required=VAL009]), `UserIsoCountry` ([Required=VAL007], [MaxLength(3)=VAL008]), `RestoreCode` ([Required=VAL010])

## Create Restore Code flow

1. Query `user` by `userEmail` + `userIsoCountry` (no statusId filter)
2. If not found → `Success=true`, `Message=""`, `Data={ Email: { EmailSend=false, EmailContent="", EmailToSend="", TemplateID="" } }`
3. If found → generate 4-char code via `RestoreCodeHelper`, insert into `userRestoreCode`, return `EmailSend=true`, `EmailContent=code`, `EmailToSend=userEmail`, `TemplateID=settings.EmailTemplateIdRestore`
4. Return type: `StandardResponseDTO<CreateRestoreCodeResponseDTO>`

## Validate Restore Code flow

1. Query `userRestoreCode` JOIN `user` by `restoreCode` + `email` + `isoCountry`, filter `isUsed=0`, return `dateCreated`
2. If null → throw `CustomAuthException(403, "AUTH006")`
3. If `createdDate < 15 min ago` fails → throw `CustomAuthException(403, "AUTH007")`
4. If valid → `UPDATE userRestoreCode SET userRestoreCodeIsUsed = 1`, then return `StandardResponseDTO<object?>` with `Message=OK005`, `Success=true`, `Data=null`
5. Return type: `StandardResponseDTO<object?>`

## AOT serializer registrations

```
CreateRestoreCodeRequestDTO
ValidateRestoreCodeRequestDTO
CreateRestoreCodeResponseDTO
EmailDTO<string>
StandardResponseDTO<CreateRestoreCodeResponseDTO>
```
