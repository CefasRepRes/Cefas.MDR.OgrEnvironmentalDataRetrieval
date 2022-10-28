using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace MDRCloudServices.DataLayer.Models;

[DataContract]
public class ServiceLink
{
    [DataMember]
    public string? Href { get; set; }

    [DataMember]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? LayerName { get; set; }

    [DataMember]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? CQL { get; set; }

    [DataMember]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? Payload { get; set; }

    [DataMember]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool? MayManage { get; set; }
}
