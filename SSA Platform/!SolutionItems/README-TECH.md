# Tech notes

Based on .NET. Everything is running in docker (db and webapp).


### Install or update tooling for .net

~~~bash
dotnet tool update -g dotnet-ef
dotnet tool update -g dotnet-aspnet-codegenerator
dotnet tool update -g Microsoft.Web.LibraryManager.Cli
~~~

## JS Libs

Add htmx and alpine to js libs.
~~~bash
libman install htmx.org --files dist/htmx.min.js 
libman install alpinejs --files dist/cdn.min.js 
~~~

### Generate database migration

Run from solution folder.

~~~bash
dotnet ef migrations --project App.Infrastructure --startup-project WebApp add Initial
dotnet ef database   --project App.Infrastructure --startup-project WebApp update
dotnet ef database   --project App.Infrastructure --startup-project WebApp drop
dotnet ef migrations --project App.Infrastructure --startup-project WebApp remove
~~~

## Generate identity UI

Install Microsoft.VisualStudio.Web.CodeGeneration.Design to WebApp.  
Run from inside the WebApp directory.

~~~bash
dotnet aspnet-codegenerator identity -dc DAL.App.EF.AppDbContext -f  
~~~

## Generate controllers

Run from inside the WebApp directory.    
Don't forget to add ***Microsoft.VisualStudio.Web.CodeGeneration.Design*** package to the WebApp project as a NuGet package reference.

MVC Web Controllers (disable global warnings as errors - otherwise only one controller will be generated, then compile starts to fail)

~~~bash
dotnet aspnet-codegenerator controller -name AlertController -m  Alert -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name ConjunctionEventController -m  ConjunctionEvent -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name NotificationSettingController -m  NotificationSetting -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name OrbitController -m  Orbit -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name RiskAssessmentController -m  RiskAssessment -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name SpaceObjectController -m  SpaceObject -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name TleRecordController -m  TleRecord -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name UserOperatorController -m  UserOperator -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name OperatorController -m  Operator -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name SatelliteController -m  Satellite -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f


~~~

API Controllers

~~~bash
dotnet aspnet-codegenerator controller -name GpsLocationsController     -m GpsLocation     -actions -dc AppDbContext -outDir ApiControllers -api --useAsyncActions  -f
~~~

