To launch the application, in appsettings there needs to be a connection string added. It looks like this:

"ConnectionStrings": { "DefaultConnection": "connection string here" }

Also there need to be the jwt options section included:
"JwtOptions": {
    "Secret": "here put a very long and secret key",
    "Issuer": "YourIssuer",
    "Audience": "YourAudience",
    "ExpiryMinutes": 60
  }

  