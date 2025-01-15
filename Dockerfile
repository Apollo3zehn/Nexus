# use "dotnet/sdk" to get the compiler and "ASPNETCORE_URLS" to compensate for the now missing env variable
FROM mcr.microsoft.com/dotnet/sdk:9.0
WORKDIR /app
COPY app .

USER app
ENTRYPOINT ["./Nexus"]