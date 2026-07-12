# 1.0 Create restore code flow

flowchart TD
    Start([Start]) --> GetUser[Go to DB and get userUID filter by email and isoCountry]
    GetUser --> CheckUser{Result returns userUID?}
    
    CheckUser -- No --> RespNoCode[Make standard response without restore code and emailSend = false]
    RespNoCode --> Return200_1[Return response with status code 200]
    Return200_1 --> End([End])
    
    CheckUser -- Yes --> CreateCode[Create restore code using Helper called helperTool]
    CreateCode --> SaveDB[Save on DB data to in the other step will validate <br> created date are datetime now]
    SaveDB --> RespWithCode[Make standard response using the created code and email requested with emailSend=true]
    RespWithCode --> Return200_2[Return response with status code 200]
    Return200_2 --> End

# 1.1 create Request DTO

```json
{
  "userEmail": "string", // is required
  "isoCountry": "string" // is required
}
```

# 1.2 create Response DTO to standard DTO

```json
{
  email: emailDTO<string>
}
```

# 1.3 email standard DTO

```json
{
  "emailSend": bool,
  "emailContent": T, //in that ocation are only restore code
  "emailToSend": "string" // that should be an agrupation of emails to send "email1,email2,emailN" for example but in that ocation are the email from DTO requested
  "templateID": "string" // that will be an key from appsettings.json called emailTemplateIdRestore value are "emailRestore" remember that need be injectable ISettings and Settings to use
}
```

# 1.4 method = POST

# 1.5 Restore code logic: that will be an code from helperTool and is a alphanumeric code like DH33 only alphabet not special character in UpperCase and number for more specific scenary 2 letter and 2 number the position are ramdon also the number and letter


# 1.6 standard response not has OK code and message that will be "" and success = true

# 2.0 Validate restore code flow

flowchart TD
    Start([Start]) --> GetDate[go to DB and get created date filter by restore code and email using inner join and isUsed = 0 and isoCountry]
    GetDate --> CodeExist{restoreCode exist?}
    
    CodeExist -- No --> ErrAUTH006[throw an custom exception with error code AUTH006 who's error message says: restore code not exist please try again]
    ErrAUTH006 --> Resp403_1[returns response with status code = 403 forbbiden]
    Resp403_1 --> End([End])
    
    CodeExist -- Yes --> CalcDiff[Save in a variable the difference in minutes between created date and current date]
    CalcDiff --> GetConfig[Read RestoreCodeExpirationInMinutes from settings.json]
    GetConfig --> CheckTime{is the calculated variable < RestoreCodeExpirationInMinutes?}
    
    CheckTime -- No --> ErrAUTH007[throw an custom exception with error code AUTH007 who's error message says: restore code are caducated please try again]
    ErrAUTH007 --> Resp403_2[returns response with status code = 403 forbbiden]
    Resp403_2 --> End
    
    CheckTime -- Yes --> MarkUsed[UPDATE userRestoreCode SET isUsed = 1]
    MarkUsed --> RespOK[Make standard response with code status OK005 and data = null and success = true]
    RespOK --> Resp200[return response with status code 200]
    Resp200 --> End

# 2.1 create Request DTO

```json
{
  "userEmail": "string", // is required
  "isoCountry": "string", // is required
  "restoreCode": "string" // is required
}
```

# 2.2 method = POST

# 2.3 message "OK005" : "you're code are validated successfull you will complete resetore password process"

# 2.4 15 minutes will be configurable by settings.json
