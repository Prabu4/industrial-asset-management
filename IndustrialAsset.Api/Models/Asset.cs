
namespace IndustrialAsset.Api.Models;

public class Asset
{
    public int Id { get; set; }

    public string AssetCode { get; set; } = string.Empty;

    public string Equipment { get; set; } = string.Empty;

    public string Area { get; set; } = string.Empty;

    public DateOnly LastMaintenance { get; set; }

    public string Status { get; set; } = "Operational";
}
