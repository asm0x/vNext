#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/SCO/SCO.csproj", "src/SCO/"]
RUN dotnet restore "src/SCO/SCO.csproj"
COPY . .
WORKDIR "/src/src/SCO"
RUN dotnet build "SCO.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SCO.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SCO.dll"]