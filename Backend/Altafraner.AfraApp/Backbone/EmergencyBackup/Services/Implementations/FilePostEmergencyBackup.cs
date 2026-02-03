using System.Net.Http.Headers;
using System.Text;
using Altafraner.AfraApp.Backbone.EmergencyBackup.Configuration;
using Altafraner.AfraApp.Backbone.EmergencyBackup.Services.Contracts;

namespace Altafraner.AfraApp.Backbone.EmergencyBackup.Services.Implementations;

/// <summary>
///     An implementation of <see cref="IEmergencyBackupService" /> that saves HTML content via HTTP POST requests to a specified endpoint.
/// </summary>
public class FilePostEmergencyBackup : IEmergencyBackupService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FilePostEmergencyBackup> _logger;

    ///
    public FilePostEmergencyBackup(
        IHttpClientFactory httpClientFactory,
        ILogger<FilePostEmergencyBackup> logger
    )
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SaveHtmlAsync(string name, string content)
    {
        _logger.LogInformation("Saving {Name}", name);
        var client = _httpClientFactory.CreateClient(FilePostConfiguration.HttpClientName);

        // Convert the HTML content to a byte array
        var byteArray = Encoding.UTF8.GetBytes(content);
        var byteContent = new ByteArrayContent(byteArray);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue("text/html");

        // Sanitize the file name by replacing non-alphanumeric characters with hyphens
        var fileName =
            string.Concat(name.Select(c => char.IsAsciiLetterOrDigit(c) ? c : '-')) + ".html";

        using var formData = new MultipartFormDataContent();
        formData.Add(byteContent, "file", fileName);

        try
        {
            var response = await client.PostAsync("/upload", formData);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Successfully posted emergency backup {Name}", name);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("Error posting emergency backup {Name}: {Message}", name, e.Message);
            throw;
        }
    }
}
