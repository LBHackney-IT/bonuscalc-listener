FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN
WORKDIR /app

# Copy csproj and restore as distinct layers
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

CMD dotnet test
