To launch the application, in appsettings there needs to be a connection string added. It looks like this:

"ConnectionStrings": { "DefaultConnection": "connection string here" }

How i split the project: I split it into Models project, which cotains the modelled entities and DTOs, Application project which includes service classes, Repositories which include repository classes and API which contains API endpoints. I decided to divide the project like this because I think the seperation of reposinibilities of adding, seraching devices etc is important: I use the repositories to directly work with the database, while I use the service layer to map the data to DTOs and perform other data operations such as validation. This makes it easier for me to first of all debug code when something is not working and in general makes it easier to differentiate between interacting with the databas and my application.

UPDATE: I deleted the repository project and instead I just use the context in the Service classes directly. This reduces redundancy and makes the code and project structure clearer.