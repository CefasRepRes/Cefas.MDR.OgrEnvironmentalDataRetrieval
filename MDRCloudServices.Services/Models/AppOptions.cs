using MDRCloudServices.Interfaces;

namespace MDRCloudServices.Services.Models;

/// <summary>App Options</summary>
public class AppOptions : IAppOptions
{
    /// <summary>Resource locator format</summary>
    public virtual string? ResourceLocatorFormat { get; set; }

    /// <summary>Admin email address</summary>
    public virtual string AdminEmailAddress { get; set; } = "data.manager@cefas.co.uk";

    /// <summary>Admin role</summary>
    public virtual string AdminRole { get; set; } = "Admin";

    /// <summary>External schema</summary>
    public virtual string? ExternalSchema { get; set; }

    /// <summary>UI Url</summary>
    public string? UiUri { get; set; }

    /// <summary>Build number</summary>
    public string? BuildNumber { get; set; }

    /// <summary>Override the database provider</summary>
    public string? Database { get; set; }
}
