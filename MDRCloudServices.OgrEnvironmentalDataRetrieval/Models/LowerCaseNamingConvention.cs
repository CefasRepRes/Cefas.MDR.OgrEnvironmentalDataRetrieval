using YamlDotNet.Serialization;

namespace MDRCloudServices.OgrEnvironmentalDataRetrieval.Models;

/// <summary>Lower case naming convention</summary>
public class LowerCaseNamingConvention : INamingConvention
{
    /// <summary>Apply naming convention</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public string Apply(string value) => value.ToLowerInvariant();
}
