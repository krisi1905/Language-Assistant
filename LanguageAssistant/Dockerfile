

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY . .

RUN dotnet restore "LanguageAssistant.csproj"

RUN dotnet publish "LanguageAssistant.csproj" -c Release -o /app/publish --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 10000

ENTRYPOINT ["dotnet", "LanguageAssistant.dll"]
