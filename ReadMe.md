\# Description :

the user can send request to check availability of books in some libraries and if there is an available copy of that book he can book it. 

\# Project structure : 

LibraryBooking.sln

├── src/LibraryBooking.Domain          (class library — entities, no deps)

├── src/LibraryBooking.Infrastructure  (class library — DbContext, EF config)

└── src/LibraryBooking.Api             (ASP.NET Core Web API)

