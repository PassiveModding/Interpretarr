FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ENV ASPNETCORE_URLS=http://+:80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["./Interpretarr/Interpretarr.csproj", "Interpretarr/"]
RUN dotnet restore "Interpretarr/Interpretarr.csproj"
COPY ./Interpretarr Interpretarr
WORKDIR "/src/Interpretarr"
RUN dotnet build "Interpretarr.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Interpretarr.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Set default port to 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "Interpretarr.dll"]
