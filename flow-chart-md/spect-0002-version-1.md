# 1. Valid session flow

flowchart TD
    Start([Start]) --> ValJWT[Using JWT helper validate sign with secret key & check expiration. Returns two bools: sign and expired]
    
    ValJWT --> SignOk{JWT sign is okay?}
    
    SignOk -- No --> ErrSign[Throw custom exception AUTH003: token malformed, please re-signin]
    ErrSign --> Resp401_Sign[Return middleware standard response error: 401 Unauthorized]
    Resp401_Sign --> End([End])
    
    SignOk -- Yes --> IsExp{JWT is expired?}
    
    IsExp -- No --> RespOkNull[Make standard response: success = true, data = null. Does not renew JWT]
    RespOkNull --> Resp200[Return response: 200 OK]
    Resp200 --> End
    
    IsExp -- Yes --> GetDB[Go to DB, get userSessionExpiresDate filtered by refresh token. If missing, return null]
    GetDB --> ResNull{Result is null?}
    
    ResNull -- Yes --> ErrNoSess[Throw custom exception AUTH004: session doesn't exist, please re-signin]
    ErrNoSess --> Resp401[Return middleware standard response error: 401 Unauthorized]
    Resp401 --> End
    
    ResNull -- No --> CheckRefExp[Save expires date from DB to variable and check if expired]
    CheckRefExp --> IsRefExp{Is refresh token expired?}
    
    IsRefExp -- Yes --> ErrRefExp[Throw custom exception AUTH005: session expired, please re-signin]
    ErrRefExp --> Resp401_2[Return middleware standard response error: 401 Unauthorized]
    Resp401_2 --> End
    
    IsRefExp -- No --> GetNewData[Using joins, go to DB to get necessary data for new JWT and Refresh Token]
    GetNewData --> CreateTokens[Using JWT helper, create new token and new refresh token]
    CreateTokens --> UpdateDB[Go to DB, update user session with new refresh token, filtered by old refresh token]
    UpdateDB --> RespNewTokens[Make standard response with refreshDTO: return new token and new refresh token]
    RespNewTokens --> Resp200_2[Return response: 200 OK]
    Resp200_2 --> End

# 1.1 validate Session DTO request

```json
{
  "token": "string",
  "refreshToken": "string"
}
```

# 1.2 validate session DTO to standard response

```json
{
  "token": "string",
  "refreshToken": "string"
}
```

# 1.3 method = POST

Note: modify expires time of JWT from 2 hours to 15 minutes

# 2. Logout flow

flowchart TD
    Start([Start]) --> DelSess[Delete from session table filter by refresh token]
    DelSess --> StdResp[Make standard response with success = true]
    StdResp --> Resp200[Returns response with status code 200]
    Resp200 --> End([End])

# 2.1 logout request
"refreshToken": "string" from query url params

# 2.2 logut message for standard response because DTO for data are null
"Have a good day i hope see you again"

# 2.3 method = DELETE
