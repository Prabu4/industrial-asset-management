using IndustrialAsset.Api.Data;
using IndustrialAsset.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndustrialAsset.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetsController : ControllerBase
{
    private readonly AssetDbContext _context;

    public AssetsController(AssetDbContext context)
    {
        _context = context;
    }

    // GET: api/assets
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Asset>>> GetAssets()
    {
        var assets = await _context.Assets
            .AsNoTracking()
            .OrderBy(asset => asset.Id)
            .ToListAsync();

        return Ok(assets);
    }

    // GET: api/assets/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Asset>> GetAsset(int id)
    {
        var asset = await _context.Assets.FindAsync(id);

        if (asset is null)
        {
            return NotFound(new { message = $"Asset with ID {id} was not found." });
        }

        return Ok(asset);
    }

    // POST: api/assets
    [HttpPost]
    public async Task<ActionResult<Asset>> CreateAsset(Asset asset)
    {
        asset.AssetCode = asset.AssetCode.Trim();
        asset.Equipment = asset.Equipment.Trim();
        asset.Area = asset.Area.Trim();
        asset.Status = asset.Status.Trim();

        if (string.IsNullOrWhiteSpace(asset.AssetCode) ||
            string.IsNullOrWhiteSpace(asset.Equipment) ||
            string.IsNullOrWhiteSpace(asset.Area))
        {
            return BadRequest(new
            {
                message = "Asset code, equipment and plant area are required."
            });
        }

        if (asset.AssetCode.Length > 20 ||
            asset.Equipment.Length > 100 ||
            asset.Area.Length > 100 ||
            asset.Status.Length > 30)
        {
            return BadRequest(new
            {
                message = "One or more fields exceed the allowed length."
            });
        }

        if (asset.LastMaintenance == default)
        {
            return BadRequest(new
            {
                message = "A valid last maintenance date is required."
            });
        }

        var validStatuses = new[]
        {
            "Operational",
            "Maintenance",
            "Out of Service"
        };

        if (!validStatuses.Contains(asset.Status))
        {
            return BadRequest(new
            {
                message = "Status must be Operational, Maintenance or Out of Service."
            });
        }

        var duplicateCode = await _context.Assets
            .AnyAsync(a => a.AssetCode == asset.AssetCode);

        if (duplicateCode)
        {
            return Conflict(new
            {
                message = $"Asset code '{asset.AssetCode}' already exists."
            });
        }

        // The database generates the ID.
        asset.Id = 0;

        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetAsset),
            new { id = asset.Id },
            asset);
    }

    // PUT: api/assets/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsset(int id, Asset updatedAsset)
    {
        var asset = await _context.Assets.FindAsync(id);

        if (asset is null)
        {
            return NotFound(new { message = $"Asset with ID {id} was not found." });
        }

        updatedAsset.AssetCode = updatedAsset.AssetCode.Trim();
        updatedAsset.Equipment = updatedAsset.Equipment.Trim();
        updatedAsset.Area = updatedAsset.Area.Trim();
        updatedAsset.Status = updatedAsset.Status.Trim();

        if (string.IsNullOrWhiteSpace(updatedAsset.AssetCode) ||
            string.IsNullOrWhiteSpace(updatedAsset.Equipment) ||
            string.IsNullOrWhiteSpace(updatedAsset.Area))
        {
            return BadRequest(new
            {
                message = "Asset code, equipment and plant area are required."
            });
        }

        if (updatedAsset.AssetCode.Length > 20 ||
            updatedAsset.Equipment.Length > 100 ||
            updatedAsset.Area.Length > 100 ||
            updatedAsset.Status.Length > 30)
        {
            return BadRequest(new
            {
                message = "One or more fields exceed the allowed length."
            });
        }

        if (updatedAsset.LastMaintenance == default)
        {
            return BadRequest(new
            {
                message = "A valid last maintenance date is required."
            });
        }

        var validStatuses = new[]
        {
            "Operational",
            "Maintenance",
            "Out of Service"
        };

        if (!validStatuses.Contains(updatedAsset.Status))
        {
            return BadRequest(new
            {
                message = "Status must be Operational, Maintenance or Out of Service."
            });
        }

        var duplicateCode = await _context.Assets
            .AnyAsync(a => a.AssetCode == updatedAsset.AssetCode && a.Id != id);

        if (duplicateCode)
        {
            return Conflict(new
            {
                message = $"Asset code '{updatedAsset.AssetCode}' already exists."
            });
        }

        asset.AssetCode = updatedAsset.AssetCode;
        asset.Equipment = updatedAsset.Equipment;
        asset.Area = updatedAsset.Area;
        asset.LastMaintenance = updatedAsset.LastMaintenance;
        asset.Status = updatedAsset.Status;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/assets/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsset(int id)
    {
        var asset = await _context.Assets.FindAsync(id);

        if (asset is null)
        {
            return NotFound(new { message = $"Asset with ID {id} was not found." });
        }

        _context.Assets.Remove(asset);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}