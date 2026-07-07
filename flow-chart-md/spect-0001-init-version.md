# 1. Signup flow

flowchart TD
    Start([Start]) --> CheckCount["Excecutes query what gets only the count of all rows where user emails and user name and iso country match"]
    CheckCount --> IsDuplicate{"user count > 0"}
    
    IsDuplicate -- Yes --> Throw409["throw an custom exception with status code 409, and error says user duplicated please go to signing"]
    Throw409 --> Middleware["Middleware catch exception an returns JSON with status code 409 conflict and error code AUTH001"]
    Middleware --> End([End])
    
    IsDuplicate -- No --> EncryptPwd["Encrypts the userPassword from request DTO using crypto helper"]
    EncryptPwd --> InsertUser["Inserts user on DB and returns new GUID to create session"]
    InsertUser --> CreateSession["Calls create session function who's returns refresh token and session token as string"]
    CreateSession --> UpsertSession["Upsert to DB with new session created don't returns anything"]
    UpsertSession --> CreateDTO["Creates response DTO formating user information and session data on a standard response DTO and success message code OK001"]
    CreateDTO --> Return200["Returns succesfull with status code 200"]
    Return200 --> End

# 1.1 Signup request DTO

```json
{
  "userFirstName": "string", // required
  "userLastName": "string", // required
  "userName" : "string",
  "userEmail": "string", // required if not has userName
  "userPassword": "string", // min length 8
  "userIsoCountry": "string", // max length 3
}
```

# 1.2 Signup response DTO to standard response

```json
{
  "userFullName": "string", // firstName + LastName
  "userEmail": "string",
  "refreshToken": "string",
  "sessionToken": "string"
}
```

# 1.3 userMessage and code

"OK001": "User created succesfull thanks and welcome"

# 2. Signin flow

flowchart TD
    Start([Start]) --> DBQuery["Go to DB and filter user by email or user name and iso country returns password, user UID and full name (first name + last name)"]
    DBQuery --> UserFound{"Returns any user?"}
    
    UserFound -- No --> Throw409["throw an custom exception with status code 409, and error says user credentials are bad please retry"]
    UserFound -- Yes --> PwdMatch{"Using crypto helper password DTO match with password DB?"}
    
    PwdMatch -- No --> Throw409
    Throw409 --> Middleware["Middleware catch exception an returns JSON with status code 409 conflict and error code AUTH002"]
    Middleware --> End([End])
    
    PwdMatch -- Yes --> CreateJWT["Calls JWT helper to create a session returns refresh token and session token"]
    CreateJWT --> UpsertSession["Upsert to data base with new session"]
    UpsertSession --> FormatRes["Format response with user's data and session's data"]
    FormatRes --> Return200["Returns response with status code 200 and success code OK002"]
    Return200 --> End


# 2.1 Signin request DTO

```json
{
  "userName" : "string",
  "userEmail": "string", // required if not has userName
  "userPassword": "string", // min length 8
  "userIsoCountry": "string", // max length 3
}
```

# 2.2 Signin response DTO to standard response

```json
{
  "userFullName": "string", // firstName + LastName
  "userEmail": "string",
  "refreshToken": "string",
  "sessionToken": "string"
}
```

# 2.3 userMessage and code

"OK001": "Welcome again i'm so happy to see you again"

# 3.0 Middleware flow

flowchart TD
    Start([Start]) --> CatchReq["Catch request and validate Data Annotations from entry JSON vs DTO using a dictionary that matches endpoint with their DTO"]
    CatchReq --> IsJsonOk{"Data Annotations are valid?"}
    
    IsJsonOk -- No --> MakeErrorMsg["Make an error message to user with what's request is wrong like 'password don't have minimum length | First Name is required | etc...'"]
    MakeErrorMsg --> ThrowError["Throw error with message validation using error code: VAL000N depending on what field is wrong. All fields that require Data Annotations have their error code and message, forming an array like VAL001 | VAL002 | VAL000N"]
    ThrowError --> Return400["Returns standard response with errors and status code 400 bad request."]
    Return400 --> End([End])
    
    IsJsonOk -- Yes --> ContinueProcess["Continue with process functions, returns result to controller"]
    ContinueProcess --> IsErrorCatched{"Something wrong or catch an error?"}
    
    IsErrorCatched -- No --> ReturnControllerSuccess["Returns successful execution to caller"]
    ReturnControllerSuccess --> End
    
    IsErrorCatched -- Yes --> MakeStdResponse["Make standard response using custom exception's data that has status code, error user message and errors array. If none, default status code is 500 and default message code is AUT000"]
    MakeStdResponse --> ReturnErrorResponse["Returns response with errors using status code catched."]
    ReturnErrorResponse --> End
