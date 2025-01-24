FROM mcr.microsoft.com/dotnet/sdk:6.0

ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN
WORKDIR /app

# Install PostgreSQL client
RUN apt-get update && apt-get install -y postgresql-client

# Copy wait script
COPY wait-for-postgres.sh .
RUN chmod +x wait-for-postgres.sh

# Copy csproj and restore
COPY ./BonusCalcListener.sln ./
COPY ./BonusCalcListener/BonusCalcListener.csproj ./BonusCalcListener/
COPY ./BonusCalcListener.Tests/BonusCalcListener.Tests.csproj ./BonusCalcListener.Tests/
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config
COPY /database/init-testdb.sql /docker-entrypoint-initdb.d/

RUN dotnet restore ./BonusCalcListener/BonusCalcListener.csproj
RUN dotnet restore ./BonusCalcListener.Tests/BonusCalcListener.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out BonusCalcListener.Tests/BonusCalcListener.Tests.csproj

CMD ["./wait-for-postgres.sh", "test-database", "dotnet", "test"]