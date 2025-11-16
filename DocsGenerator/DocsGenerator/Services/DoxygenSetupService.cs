using System.Diagnostics;
using System.Text;

namespace DocsGenerator.Services;

public class DoxygenSetupService
{
    private readonly ILogger<DoxygenSetupService> _logger;
    private readonly IConfiguration _configuration;

    public DoxygenSetupService(ILogger<DoxygenSetupService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SetupAsync()
    {
        try
        {
            await CloneProjectRepositoryAsync();
            await CloneDoxygenAwesomeAsync();
            await GenerateDoxyfileAsync();
            await GenerateDocumentationAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Doxygen setup");
            throw;
        }
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
            "clone -b Architect2.0 --single-branch https://github.com/Quark-Hell/Alpha_Engine /src_project/project");
        _logger.LogInformation("Project repository cloned successfully");
    }

    private async Task CloneDoxygenAwesomeAsync()
    {
        var doxygenAwesomePath = "/doxygen-awesome-css";
        if (Directory.Exists(doxygenAwesomePath))
        {
            _logger.LogInformation("doxygen-awesome-css already exists");
            return;
        }

        _logger.LogInformation("Cloning doxygen-awesome-css...");
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
            ["OUTPUT_DIRECTORY"] = "/app/docs",
            ["INPUT"] = "/src_project/project/ALPHA_Engine/Engine",
            ["RECURSIVE"] = "YES",
            ["GENERATE_HTML"] = "YES",
            ["HTML_OUTPUT"] = "html",
            ["FILE_PATTERNS"] = "*.c *.cc *.cxx *.cpp *.c++ *.h *.hpp *.hxx",
            ["EXCLUDE_PATTERNS"] = "*/[Ee][Xx][Tt][Ee][Rr][Nn][Aa][Ll]/* */[Ee][Xx][Tt][Ee][Rr][Nn][Aa][Ll]/",
            ["PROJECT_NAME"] = "\"Alpha Engine\"",
            ["PROJECT_LOGO"] = "/app/logos/Alpha_Engine_Logo_64.png",
            ["GENERATE_TREEVIEW"] = "YES",
            ["DISABLE_INDEX"] = "NO",
            ["FULL_SIDEBAR"] = "NO",
            ["HTML_COLORSTYLE"] = "LIGHT",
            ["HTML_EXTRA_STYLESHEET"] = "/doxygen-awesome-css/doxygen-awesome.css /doxygen-awesome-css/doxygen-awesome-sidebar-only.css",
            ["HTML_EXTRA_FILES"] = "/doxygen-awesome-css/doxygen-awesome-darkmode-toggle.js /doxygen-awesome-css/doxygen-awesome-fragment-copy-button.js /doxygen-awesome-css/doxygen-awesome-paragraph-link.js",

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