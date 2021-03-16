#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5003
EXPOSE 5003/tcp

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Milvasoft.SampleAPI/Milvasoft.SampleAPI.csproj", "Milvasoft.SampleAPI/"]
RUN dotnet restore "Milvasoft.SampleAPI/Milvasoft.SampleAPI.csproj"
COPY . .
WORKDIR "/src/Milvasoft.SampleAPI"
RUN dotnet build "Milvasoft.SampleAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Milvasoft.SampleAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .


ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8

ENTRYPOINT ["dotnet", "Milvasoft.SampleAPI.dll"]