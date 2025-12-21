using DocsGenerator.Data;
using DocsGenerator.Models;
using System.Diagnostics;
using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace DocsGenerator.Services;

public class DoxygenSetupService
{
    private readonly ILogger<DoxygenSetupService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly string owner = "Quark-Hell";
    private readonly string repo = "Alpha_Engine";
    private readonly string branch = "Architect2.0";

    private string? _lastGithubHash;
    private string? _lastGithubCommitName;
    private string? _lastDbHash;
    private DateTime _lastGitCommitCreated;

    private string _folderName;
    private string _lastDocsPath;

    public DoxygenSetupService(ILogger<DoxygenSetupService> logger, IConfiguration configuration, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _scopeFactory = scopeFactory;

        UpdateFolderPath("");
    }

    private void UpdateFolderPath(string folderName)
    {
        _folderName = $"{folderName}";
        _lastDocsPath = $"/app/docs/{_folderName}";
    }

    public async Task DbInsertAsync(ProjectVersion pv)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.ProjectVersions.Add(pv);
        await db.SaveChangesAsync();
        _logger.LogInformation("Inserted ProjectVersion with ID: {Id}", pv.Id);
    }

    public async Task SetupAsync()
    {
        try
        {
            await FetchFromGuthub();
            _lastDbHash = await GetLastHashFromDB();

            if (_lastGithubHash == null)
            {
                throw new InvalidOperationException("Cannot get hash from GitHub");
            }

            if (_lastDbHash == null || _lastDbHash != _lastGithubHash)
            {
                UpdateFolderPath($"{_lastGithubHash}");

                await CloneProjectRepositoryAsync();
                await CloneDoxygenAwesomeAsync();
                await GenerateDoxyfileAsync();
                await GenerateDocumentationAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Doxygen setup");
            throw;
        }
    }

    private async Task FetchFromGuthub()
    {
        using var http = new HttpClient();

        http.DefaultRequestHeaders.Add("User-Agent", "DocsGenerator");

        var url = $"https://api.github.com/repos/{owner}/{repo}/commits/{branch}";

        try
        {
            var response = await http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var root = doc.RootElement;

            _lastGithubHash = doc.RootElement.GetProperty("sha").GetString()!;
            _lastGithubCommitName = root.GetProperty("commit").GetProperty("message").GetString()!;

            _lastGitCommitCreated = root.GetProperty("commit").GetProperty("committer").GetProperty("date").GetDateTime();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while fetching commit info: " + ex.Message);
            _lastGithubHash = null;
            _lastGithubCommitName = null;
        }
    }

    private async Task<string?> GetLastHashFromDB()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var last = await db.ProjectVersions
    .OrderByDescending(p => p.Id)
    .FirstOrDefaultAsync();

        return last?.CommitHash;
    }

    private async Task CloneProjectRepositoryAsync()
    {
        var projectPath = "/src_project/project";
        if (Directory.Exists(projectPath))
        {
            _logger.LogInformation("Project repository already exists at {Path}", projectPath);
            return;
        }

        _logger.LogInformation("Cloning project repository...");
        await RunCommandAsync("git",
            $"clone --depth 1 -b {branch} --single-branch https://github.com/{owner}/{repo} /src_project/project");
        _logger.LogInformation("Project repository cloned successfully");
    }

    private async Task CloneDoxygenAwesomeAsync()
    {
        var doxygenAwesomePath = "/doxygen-awesome-css";
        if (Directory.Exists(doxygenAwesomePath))
        {
            return;
        }

        await RunCommandAsync("git",
            "clone https://github.com/jothepro/doxygen-awesome-css.git /doxygen-awesome-css");
        _logger.LogInformation("doxygen-awesome-css cloned successfully");
    }

    private async Task GenerateDoxyfileAsync()
    {
        var doxyfilePath = "/src_project/Doxyfile";
        if (File.Exists(doxyfilePath))
        {
            _logger.LogInformation("Doxyfile already exists");
            return;
        }

        _logger.LogInformation("Generating Doxyfile...");
        await RunCommandAsync("doxygen", "-g /src_project/Doxyfile");

        // Apply all configuration changes
        var config = new Dictionary<string, string>
        {
            // Graphviz settings
            ["DOT_TRANSPARENT"] = "YES",
            ["DOT_IMAGE_FORMAT"] = "svg",

            // Doxygen settings
            ["OUTPUT_DIRECTORY"] = $"{_lastDocsPath}",
            ["INPUT"] = "/src_project/project/ALPHA_Engine/Engine",
            ["RECURSIVE"] = "YES",
            ["GENERATE_HTML"] = "YES",
            ["HTML_OUTPUT"] = "html",
            ["FILE_PATTERNS"] = "*.c *.cc *.cxx *.cpp *.c++ *.h *.hpp *.hxx",
            ["EXCLUDE_PATTERNS"] = "*/[Ee][Xx][Tt][Ee][Rr][Nn][Aa][Ll]/* */[Ee][Xx][Tt][Ee][Rr][Nn][Aa][Ll]/",
            ["PROJECT_NAME"] = "\"Alpha Engine\"",
            ["PROJECT_LOGO"] = "/app/resources/logos/Alpha_Engine_Logo_64.png",
            ["GENERATE_TREEVIEW"] = "YES",
            ["DISABLE_INDEX"] = "NO",
            ["HTML_HEADER"] = "/app/resources/html/header.html",
            ["FULL_SIDEBAR"] = "NO",
            ["SEARCHENGINE"] = "YES",
            ["HTML_COLORSTYLE"] = "LIGHT",
            ["HTML_EXTRA_STYLESHEET"] = "/doxygen-awesome-css/doxygen-awesome.css /doxygen-awesome-css/doxygen-awesome-sidebar-only.css",
            ["HTML_EXTRA_FILES"] =
    "/doxygen-awesome-css/doxygen-awesome-darkmode-toggle.js " +
    "/doxygen-awesome-css/doxygen-awesome-fragment-copy-button.js " +
    "/doxygen-awesome-css/doxygen-awesome-paragraph-link.js",


            ["SHOW_FILES"] = "NO",
            ["SHOW_NAMESPACES"] = "YES",
            ["SHOW_FILE_NAMES"] = "NO",
            ["FILE_VERSION_FILTER"] = "",
            ["STRIP_FROM_PATH"] = "/src_project/project/ALPHA_Engine/Engine",
            ["STRIP_FROM_INC_PATH"] = "/src_project/project/ALPHA_Engine/Engine",
            ["FULL_PATH_NAMES"] = "NO",


            // Graphviz detailed settings
            ["HAVE_DOT"] = "YES",
            ["DOT_GRAPH_MAX_NODES"] = "50",
            ["DOT_FONTPATH"] = "/usr/share/fonts/truetype/dejavu",
            ["DOT_FONTNAME"] = "DejaVu Sans",
            ["DOT_FONTSIZE"] = "10",
            ["DOT_BG_COLOR"] = "transparent",
            ["DOT_EDGE_COLOR"] = "\"#cccccc\"",
            ["DOT_NODE_COLOR"] = "\"#eeeeee\"",
            ["DOT_TEXT_COLOR"] = "\"#dddddd\"",
            ["DOT_FONTCOLOR"] = "\"#dddddd\"",
            ["DOT_LAYOUT"] = "dot",
            ["DOT_GRAPH_DIRECTION"] = "TB"
        };

        await ModifyDoxyfileAsync(doxyfilePath, config);

        // Add extra stylesheet line
        await File.AppendAllTextAsync(doxyfilePath,
            "HTML_EXTRA_STYLESHEET += /doxygen-awesome-css/doxygen-awesome-sidebar-only-darkmode-toggle.css\n");

        await File.AppendAllTextAsync(doxyfilePath, "DISABLE_INDEX = NO\n");
        await File.AppendAllTextAsync(doxyfilePath, "GENERATE_TREEVIEW = YES\n");
        await File.AppendAllTextAsync(doxyfilePath, "LAYOUT_FILE = /app/DoxygenLayout.xml\n");

        _logger.LogInformation("Doxyfile configured successfully");
    }

    private async Task ModifyDoxyfileAsync(string doxyfilePath, Dictionary<string, string> config)
    {
        var lines = await File.ReadAllLinesAsync(doxyfilePath);
        var modifiedLines = new List<string>();

        foreach (var line in lines)
        {
            var modified = line;
            foreach (var kvp in config)
            {
                // Match lines that start with the key (with optional # and whitespace)
                if (line.TrimStart().StartsWith("#") || line.TrimStart().StartsWith(kvp.Key))
                {
                    var parts = line.Split('=', 2);
                    if (parts.Length > 0)
                    {
                        var key = parts[0].TrimStart('#', ' ', '\t').Trim();
                        if (key == kvp.Key)
                        {
                            modified = $"{kvp.Key} = {kvp.Value}";
                            break;
                        }
                    }
                }
            }
            modifiedLines.Add(modified);
        }

        await File.WriteAllLinesAsync(doxyfilePath, modifiedLines);
    }

    private async Task GenerateDocumentationAsync()
    {
        _logger.LogInformation("Generating documentation...");
        await RunCommandAsync("doxygen", "/src_project/Doxyfile");
        _logger.LogInformation("Documentation generated successfully");

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var version = new ProjectVersion {
            CommitHash = _lastGithubHash,
            CommitName = _lastGithubCommitName,
            Branch = branch,
            CreatedAt = _lastGitCommitCreated,
            DocsPath = $"/docs/{_folderName}"
        };

        await DbInsertAsync(version);
    }

    private async Task<string> RunCommandAsync(string command, string arguments)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };
        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                output.AppendLine(e.Data);
                _logger.LogInformation(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                error.AppendLine(e.Data);
                _logger.LogError(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Command '{command} {arguments}' failed with exit code {process.ExitCode}. Error: {error}");
        }

        return output.ToString();
    }
}