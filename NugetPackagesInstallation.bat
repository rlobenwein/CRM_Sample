@echo off
REM Instalação de pacotes .NET

dotnet add package bootstrap
dotnet add package EPPlus
dotnet add package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Identity.UI
dotnet add package Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.Extensions.ApiDescription.Client
dotnet add package Microsoft.VisualStudio.Azure.Containers.Tools.Targets
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
dotnet add package Newtonsoft.Json
dotnet add package NSwag.ApiDescription.Client
dotnet add package Pomelo.EntityFrameworkCore.MySql

echo Instalação de pacotes concluída.
pause
