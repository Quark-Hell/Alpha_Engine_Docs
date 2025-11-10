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

RUN apt-get update && apt-get install -y \
    doxygen \
    graphviz \
    git \
    dos2unix \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
RUN mkdir -p /app/docs

COPY DocsGenerator/generated-docs.sh /generated-docs.sh
RUN dos2unix /generated-docs.sh && chmod +x /generated-docs.sh

EXPOSE 8080
ENTRYPOINT ["/generated-docs.sh"]