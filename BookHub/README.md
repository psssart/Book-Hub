## Useful commands in .net console CLI

Install tooling

~~~bash
dotnet tool update -g dotnet-ef
dotnet tool update -g dotnet-aspnet-codegenerator 
~~~

## EF Core migrations

Run from solution folder  

~~~bash
dotnet ef migrations --project App.DAL.EF --startup-project WebApp add FOOBAR
dotnet ef database   --project App.DAL.EF --startup-project WebApp update
dotnet ef database   --project App.DAL.EF --startup-project WebApp drop
~~~


0## MVC controllers

Install from nuget:  
- Microsoft.VisualStudio.Web.CodeGeneration.Design
- Microsoft.EntityFrameworkCore.SqlServer


Run from WebApp folder!  

~~~bash
cd WebApp

dotnet aspnet-codegenerator controller -name PublishersController        -actions -m  App.Domain.Entities.Publisher        -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name BooksController        -actions -m  App.Domain.Entities.Book        -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name WarehousesController        -actions -m  App.Domain.Entities.Warehouse        -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name BooksWarehousesController        -actions -m  App.Domain.Address_Tables.BookWarehouses        -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name GenresController        -actions -m  App.Domain.Entities.Genre        -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name BooksGenresController        -actions -m  App.Domain.Address_Tables.BookGenre        -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name AuthorsController        -actions -m  App.Domain.Entities.Author        -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name BooksAuthorsController        -actions -m  App.Domain.Address_Tables.BookAuthor        -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name RatingsController        -actions -m  App.Domain.Entities.Rating        -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name UsersSubscriptionsController        -actions -m  App.Domain.Address_Tables.UserSubscription        -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name PurchasesController        -actions -m  App.Domain.Entities.Purchase        -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name PurchasedBooksController        -actions -m  App.Domain.Address_Tables.PurchasedBook        -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name DiscussionsController        -actions -m  App.Domain.Entities.Discussion        -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name TopicsController        -actions -m  App.Domain.Entities.Topic        -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name MessagesController        -actions -m  App.Domain.Entities.Message        -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f

# use area (EXAMPLE)
dotnet aspnet-codegenerator controller -name ContestsController        -actions -m  App.Domain.Contest        -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f


Download .NET 8.0: https://dotnet.microsoft.com/en-us/download/dotnet/8.0
dotnet tool update -g dotnet-aspnet-codegenerator
# Generate User Authentication pages
# Do not use that, it's already generated!
# dotnet aspnet-codegenerator identity -f --userClass=App.Domain.Identity.AppUser -gl
cd ..
~~~

3f9c0db6-32ae-4151-b721-4ea591001a68
8c33745e-db4f-4664-8db0-181f4b474b44

Api controllers
~~~bash
dotnet aspnet-codegenerator controller -name ContestsController  -m  App.Domain.Contest        -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
~~~

## Docker

~~~bash
docker buildx build --progress=plain --force-rm --push -t akaver/webapp:latest . 

# multiplatform build on apple silicon
# https://docs.docker.com/build/building/multi-platform/
docker buildx create --name mybuilder --bootstrap --use
docker buildx build --platform linux/amd64 -t akaver/webapp:latest --push .


~~~
