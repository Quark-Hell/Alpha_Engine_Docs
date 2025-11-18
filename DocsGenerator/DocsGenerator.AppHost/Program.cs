var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithPgAdmin();
var postgresdb = postgres.WithDataVolume().AddDatabase("docsdb");

var docsPath = Path.Combine(builder.AppHostDirectory, "generated-docs");
Directory.CreateDirectory(docsPath);

var docsGenerator = builder.AddDockerfile("docsgenerator", "..", "DocsGenerator/Dockerfile")
    .WithReference(postgresdb)
    .WithHttpEndpoint(port: 5001, targetPort: 8080, name: "api")
    .WithBindMount(docsPath, "/app/docs");

var nginxConfigPath = Path.Combine(builder.AppHostDirectory, "nginx.conf");

var docsWeb = builder.AddContainer("docs-nginx", "nginx", "alpine")
    .WithBindMount(docsPath, "/usr/share/nginx/html/docs", isReadOnly: true)
    .WithBindMount(nginxConfigPath, "/etc/nginx/nginx.conf", isReadOnly: true)
    .WithHttpEndpoint(port: 8080, targetPort: 80, name: "docs");

builder.Build().Run();