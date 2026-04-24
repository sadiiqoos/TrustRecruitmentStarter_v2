FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TrustRecruitment.sln ./
COPY src/TrustRecruitment.Web/TrustRecruitment.Web.csproj src/TrustRecruitment.Web/
COPY src/TrustRecruitment.Business/TrustRecruitment.Business.csproj src/TrustRecruitment.Business/
COPY src/TrustRecruitment.Data/TrustRecruitment.Data.csproj src/TrustRecruitment.Data/

RUN dotnet restore src/TrustRecruitment.Web/TrustRecruitment.Web.csproj

COPY . .
WORKDIR /src/src/TrustRecruitment.Web
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TrustRecruitment.Web.dll"]