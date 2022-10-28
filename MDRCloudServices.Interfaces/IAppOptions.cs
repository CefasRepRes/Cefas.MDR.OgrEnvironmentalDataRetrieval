namespace MDRCloudServices.Interfaces;

public interface IAppOptions
{
    string AdminRole { get; set; }
    string AdminEmailAddress { get; set; }
    string? ResourceLocatorFormat { get; set; }
    string? UiUri { get; set; }
    string? BuildNumber { get; set; }
}
