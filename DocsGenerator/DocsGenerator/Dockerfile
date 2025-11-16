FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["DocsGenerator/DocsGenerator.csproj", "DocsGenerator/"]
COPY ["DocsGenerator.ServiceDefaults/DocsGenerator.ServiceDefaults.csproj", "DocsGenerator.ServiceDefaults/"]
RUN dotnet restore "DocsGenerator/DocsGenerator.csproj"

COPY DocsGenerator/ DocsGenerator/
COPY DocsGenerator.ServiceDefaults/ DocsGenerator.ServiceDefaults/

WORKDIR "/src/DocsGenerator"
RUN dotnet publish "DocsGenerator.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y wget graphviz git dos2unix && \
    wget https://www.doxygen.nl/files/doxygen-1.15.0.linux.bin.tar.gz && \
    tar -xzf doxygen-1.15.0.linux.bin.tar.gz && \
    cp doxygen-1.15.0/bin/doxygen /usr/local/bin/doxygen && \
    rm -rf doxygen-1.15.0*

COPY --from=build /app/publish .
RUN mkdir -p /app/docs

COPY DocsGenerator/logos /app/logos

EXPOSE 8080
ENTRYPOINT ["dotnet", "DocsGenerator.dll"]