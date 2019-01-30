FROM microsoft/dotnet:3.0-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:3.0-sdk AS build
WORKDIR /src
COPY ["SharpBird.Api/SharpBird.Api.csproj", "SharpBird.Api/"]
RUN dotnet restore "SharpBird.Api/SharpBird.Api.csproj"
COPY . .
RUN dotnet test
WORKDIR "/src/SharpBird.Api"
RUN dotnet build "SharpBird.Api.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SharpBird.Api.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SharpBird.Api.dll"]