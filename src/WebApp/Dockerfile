FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

COPY . .
RUN dotnet publish src/WebApp/WebApp.csproj -c Release -o /app --self-contained --runtime linux-musl-x64 /p:PublishReadyToRun=true

FROM mcr.microsoft.com/dotnet/runtime-deps:7.0-alpine
WORKDIR /app
COPY --from=build /app .

HEALTHCHECK CMD wget -qO- -t1 http://localhost:80/healthz || exit 1
ENTRYPOINT ["./Knapcode.FactorioTools.WebApp"]
