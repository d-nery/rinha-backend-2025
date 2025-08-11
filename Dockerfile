FROM mcr.microsoft.com/dotnet/sdk:10.0-preview-alpine-aot AS build
WORKDIR /src
COPY rinha.csproj .
RUN dotnet restore "./rinha.csproj"
COPY . .
RUN dotnet publish "./rinha.csproj" -c Release -o /app/publish /p:UseAppHost=true

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-preview-alpine AS final
WORKDIR /app
EXPOSE 5000
COPY --from=build /app/publish .
ENTRYPOINT ["./rinha"]