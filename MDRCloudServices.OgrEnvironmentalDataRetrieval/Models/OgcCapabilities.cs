namespace MDRCloudServices.OgrEnvironmentalDataRetrieval.Models;

/// <summary>Capabilities</summary>
public class OgcCapabilities
{
    /// <summary>Title</summary>
    public string? Title { get; set; }

    /// <summary>Description</summary>
    public string? Description { get; set; }

    /// <summary>Links</summary>
    public List<OgcLink> Links { get; set; } = new();

    /// <summary>Keywords</summary>
    public List<string> Keywords { get; set; } = new();

    /// <summary>Provider</summary>
    public CapabilityProvider? Provider { get; set; }

    /// <summary>Contact</summary>
    public CapabilityContact? Contact { get; set; }
}

/// <summary>Link</summary>
public class OgcLink
{
    /// <summary>Href</summary>
    public string? Href { get; set; }

    /// <summary>Href language</summary>
    public string? Hreflang { get; set; } = "en";

    /// <summary>Rel</summary>
    public string? Rel { get; set; }

    /// <summary>Type</summary>
    public string? Type { get; set; }

    /// <summary>Title</summary>
    public string? Title { get; set; }
}

/// <summary>Capability provider</summary>
public class CapabilityProvider
{
    /// <summary>Name </summary>
    public string? Name { get; set; }

    /// <summary>Url</summary>
    public string? Url { get; set; }
}

/// <summary>Capability contact</summary>
public class CapabilityContact
{
    /// <summary>Email</summary>
    public string? Email { get; set; }

    /// <summary>Phone</summary>
    public string? Phone { get; set; }

    /// <summary>Fax</summary>
    public string? Fax { get; set; }

    /// <summary>Hours</summary>
    public string? Hours { get; set; }

    /// <summary>Instructions</summary>
    public string? Instructions { get; set; }

    /// <summary>Address</summary>
    public string? Address { get; set; }

    /// <summary>Post code</summary>
    public string? PostalCode { get; set; }

    /// <summary>City</summary>
    public string? City { get; set; }

    /// <summary>County</summary>
    public string? Stateorprovince { get; set; }

    /// <summary>Country</summary>
    public string? Country { get; set; }
}
