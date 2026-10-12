
using IndustrialAsset.Api.Controllers;
using IndustrialAsset.Api.Data;
using IndustrialAsset.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IndustrialAsset.Api.Tests;

public class AssetsControllerTests
{
    private static AssetDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AssetDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AssetDbContext(options);
    }

    private static Asset CreateSampleAsset() => new()
    {
        AssetCode = "AST-TEST",
        Equipment = "Centrifugal Pump",
        Area = "Process Area A",
        LastMaintenance = new DateOnly(2026, 10, 1),
        Status = "Operational"
    };

    [Fact]
    public async Task GetAssets_ReturnsAllAssets()
    {
        using var context = CreateContext();
        context.Assets.Add(CreateSampleAsset());
        await context.SaveChangesAsync();

        var controller = new AssetsController(context);

        var result = await controller.GetAssets();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var assets = Assert.IsAssignableFrom<IEnumerable<Asset>>(ok.Value);
        Assert.Single(assets);
    }

    [Fact]
    public async Task GetAsset_WhenMissing_ReturnsNotFound()
    {
        using var context = CreateContext();
        var controller = new AssetsController(context);

        var result = await controller.GetAsset(999);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateAsset_WithValidData_ReturnsCreated()
    {
        using var context = CreateContext();
        var controller = new AssetsController(context);
        var asset = CreateSampleAsset();

        var result = await controller.CreateAsset(asset);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var savedAsset = Assert.IsType<Asset>(created.Value);

        Assert.Equal("AST-TEST", savedAsset.AssetCode);
        Assert.Equal(1, await context.Assets.CountAsync());
    }

    [Fact]
    public async Task UpdateAsset_WithValidData_ReturnsNoContent()
    {
        using var context = CreateContext();
        var asset = CreateSampleAsset();
        context.Assets.Add(asset);
        await context.SaveChangesAsync();

        var controller = new AssetsController(context);
        var updatedAsset = CreateSampleAsset();
        updatedAsset.Equipment = "Updated Pump";

        var result = await controller.UpdateAsset(asset.Id, updatedAsset);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(
            "Updated Pump",
            (await context.Assets.FindAsync(asset.Id))!.Equipment);
    }

    [Fact]
    public async Task DeleteAsset_WhenExisting_ReturnsNoContent()
    {
        using var context = CreateContext();
        var asset = CreateSampleAsset();
        context.Assets.Add(asset);
        await context.SaveChangesAsync();

        var controller = new AssetsController(context);

        var result = await controller.DeleteAsset(asset.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(await context.Assets.ToListAsync());
    }

    
[Fact]
public async Task CreateAsset_WithMissingRequiredFields_ReturnsBadRequest()
{
    using var context = CreateContext();
    var controller = new AssetsController(context);

    var asset = CreateSampleAsset();
    asset.Equipment = "";

    var result = await controller.CreateAsset(asset);

    Assert.IsType<BadRequestObjectResult>(result.Result);
}

[Fact]
public async Task CreateAsset_WithDuplicateAssetCode_ReturnsConflict()
{
    using var context = CreateContext();
    context.Assets.Add(CreateSampleAsset());
    await context.SaveChangesAsync();

    var controller = new AssetsController(context);
    var duplicate = CreateSampleAsset();

    var result = await controller.CreateAsset(duplicate);

    Assert.IsType<ConflictObjectResult>(result.Result);
}

[Fact]
public async Task UpdateAsset_WhenMissing_ReturnsNotFound()
{
    using var context = CreateContext();
    var controller = new AssetsController(context);

    var result = await controller.UpdateAsset(999, CreateSampleAsset());

    Assert.IsType<NotFoundObjectResult>(result);
}

[Fact]
public async Task DeleteAsset_WhenMissing_ReturnsNotFound()
{
    using var context = CreateContext();
    var controller = new AssetsController(context);

    var result = await controller.DeleteAsset(999);

    Assert.IsType<NotFoundObjectResult>(result);
}

[Fact]
public async Task CreateAsset_WithInvalidStatus_ReturnsBadRequest()
{
    using var context = CreateContext();
    var controller = new AssetsController(context);

    var asset = CreateSampleAsset();
    asset.Status = "Unknown";

    var result = await controller.CreateAsset(asset);

    Assert.IsType<BadRequestObjectResult>(result.Result);
}

}
