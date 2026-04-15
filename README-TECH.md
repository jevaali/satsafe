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
dotnet ef migrations --project App.DAL.EF --startup-project WebApp add Initial
dotnet ef database   --project App.DAL.EF --startup-project WebApp drop
dotnet ef migrations --project App.DAL.EF --startup-project WebApp remove
dotnet ef database   --project App.DAL.EF --startup-project WebApp update
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
dotnet aspnet-codegenerator controller -name CompanyController -m  Company -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name CompanySettingsController -m  CompanySettings -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name SubscriptionController -m  Subscription -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name ClaimController -m  Claim -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name ClaimStatusController -m  ClaimStatus -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name FormularyController -m  Formulary -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name InsurencePlanController -m  InsurencePlan -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name InventoryController -m  Inventory -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name ControlledSubstanceLogController -m  ControlledSubstanceLog -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name MedicationController -m  Medication -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name AllergyController -m  Allergy -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name PatientController -m  Patient -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name PrescriberController -m  Prescriber -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name DispensingController -m  Dispensing -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name PrescriptionController -m  Prescription -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name PrescriptionItemController -m  PrescriptionItem -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f

~~~

API Controllers

~~~bash
dotnet aspnet-codegenerator controller -name GpsLocationsController     -m GpsLocation     -actions -dc AppDbContext -outDir ApiControllers -api --useAsyncActions  -f
~~~



## Seeded Users (for testing purposes only, not for production use)

systemsales@pharmacy.com
SystemSales123!

systemsupport@pharmacy.com
SystemSupport123!

systemadmin@pharmacy.com
SystemAdmin123!

companyowner@pharmacy.com
CompanyOwner123!

companyadmin@pharmacy.com
CompanyAdmin123!

companymanager@pharmacy.com
CompanyManager123!

companyemployee@pharmacy.com
CompanyEmployee123!

