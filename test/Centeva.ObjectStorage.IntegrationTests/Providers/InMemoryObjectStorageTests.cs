using Centeva.ObjectStorage.Builtin;
using System.Text;

#pragma warning disable xUnit1051 // ReadToEndAsync does not support CancellationToken on .NET Framework

namespace Centeva.ObjectStorage.IntegrationTests.Providers;

public class InMemoryObjectStorageFixture : ObjectStorageFixture
{
    public override IObjectStorage CreateStorage(TestSettings settings)
    {
        return new InMemoryObjectStorage();
    }

    public override void Cleanup()
    {
    }
}

public class InMemoryObjectStorageTests : CommonObjectStorageTests, IClassFixture<InMemoryObjectStorageFixture>
{
    public InMemoryObjectStorageTests(InMemoryObjectStorageFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void ImplementsISupportsMetadata()
    {
        _sut.Should().BeAssignableTo<ISupportsMetadata>();
    }

    [Fact]
    public async Task Instances_DoNotShareObjects()
    {
        var path = await WriteToRandomPathAsync();

        var other = new InMemoryObjectStorage();

        (await other.ExistsAsync(path, CancellationToken)).Should().BeFalse();
        (await other.ListAsync(cancellationToken: CancellationToken)).Should().BeEmpty();
    }

    [Fact]
    public async Task GetAsync_ReturnsContentTypeAndMetadataFromWrite()
    {
        var metadata = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };
        var path = await WriteToRandomPathAsync("", ".json", new WriteOptions { ContentType = "application/json", Metadata = metadata });

        var entry = await _sut.GetAsync(path, CancellationToken);

        entry.Should().NotBeNull();
        entry!.ContentType.Should().Be("application/json");
        entry.Metadata.Should().BeEquivalentTo(metadata);
    }

    [Fact]
    public async Task GetAsync_WithFolderPath_ReturnsNull()
    {
        var path = await WriteToRandomPathAsync(Guid.NewGuid().ToString());

        var entry = await _sut.GetAsync(path.Folder, CancellationToken);

        entry.Should().BeNull();
    }

    [Fact]
    public async Task WriteAsync_ToExistingPath_ReplacesContentAndMetadata()
    {
        var path = await WriteToRandomPathAsync("", ".txt", new WriteOptions { ContentType = "text/plain", Metadata = new Dictionary<string, string> { { "old", "1" } } });

        await _sut.WriteAsync(path, new MemoryStream(Encoding.UTF8.GetBytes("new")), cancellationToken: CancellationToken);

        (await ReadAllTextAsync(path)).Should().Be("new");
        var entry = await _sut.GetAsync(path, CancellationToken);
        entry!.SizeInBytes.Should().Be(3);
        entry.ContentType.Should().BeNull();
        entry.Metadata.Should().BeEmpty();
    }

    [Fact]
    public async Task WriteAsync_CopiesContent_SoLaterChangesToSourceStreamAreNotStored()
    {
        var path = RandomStoragePath();
        var bytes = Encoding.UTF8.GetBytes("original");
        var source = new MemoryStream(bytes);

        await _sut.WriteAsync(path, source, cancellationToken: CancellationToken);
        bytes[0] = (byte)'X';

        (await ReadAllTextAsync(path)).Should().Be("original");
    }

    [Fact]
    public async Task WriteAsync_WithNullStream_ThrowsArgumentNullException()
    {
        var action = () => _sut.WriteAsync(RandomStoragePath(), null!, cancellationToken: CancellationToken);

        (await action.Should().ThrowAsync<ArgumentNullException>()).Which.ParamName.Should().Be("contentStream");
    }

    [Fact]
    public async Task WriteAsync_WithEmptyStream_StoresEmptyObject()
    {
        var path = RandomStoragePath();

        await _sut.WriteAsync(path, new MemoryStream(), cancellationToken: CancellationToken);

        (await _sut.ExistsAsync(path, CancellationToken)).Should().BeTrue();
        (await _sut.GetAsync(path, CancellationToken))!.SizeInBytes.Should().Be(0);
        (await ReadAllTextAsync(path)).Should().BeEmpty();
    }

    [Fact]
    public async Task OpenReadAsync_ReturnsReadOnlyStream_UnaffectedByLaterWrites()
    {
        var path = await WriteToRandomPathAsync();

        using var stream = await _sut.OpenReadAsync(path, CancellationToken);
        await _sut.WriteAsync(path, new MemoryStream(Encoding.UTF8.GetBytes("replaced")), cancellationToken: CancellationToken);

        stream!.CanWrite.Should().BeFalse();
        using var reader = new StreamReader(stream);
        (await reader.ReadToEndAsync()).Should().Be(_testFileContent);
    }

    [Fact]
    public async Task ListAsync_WithoutIncludeMetadata_OmitsMetadata()
    {
        var folder = Guid.NewGuid() + "/";
        var path = await WriteToRandomPathAsync(folder, ".txt", new WriteOptions { Metadata = new Dictionary<string, string> { { "key", "value" } } });

        var withoutMetadata = await _sut.ListAsync(folder, cancellationToken: CancellationToken);
        var withMetadata = await _sut.ListAsync(folder, new ListOptions { IncludeMetadata = true }, CancellationToken);

        withoutMetadata.Single(x => x.Path.Equals(path)).Metadata.Should().BeNull();
        withMetadata.Single(x => x.Path.Equals(path)).Metadata.Should().Contain("key", "value");
    }

    [Fact]
    public async Task ListAsync_WithoutRecurse_ReturnsEachSubfolderOnce()
    {
        var folder = Guid.NewGuid() + "/";
        var file = await WriteToRandomPathAsync(folder);
        await WriteToRandomPathAsync(folder + "sub/");
        await WriteToRandomPathAsync(folder + "sub/");
        await WriteToRandomPathAsync(folder + "sub/deeper/");

        var list = (await _sut.ListAsync(folder, cancellationToken: CancellationToken)).Select(x => x.Path.Full).ToList();

        list.Should().BeEquivalentTo(new[] { file.Full, "/" + folder + "sub/" });
    }

    [Fact]
    public async Task ListAsync_WithRecurse_ReturnsNestedFilesAndImpliedFolders()
    {
        var folder = Guid.NewGuid() + "/";
        var file = await WriteToRandomPathAsync(folder);
        var nested = await WriteToRandomPathAsync(folder + "sub/deeper/");

        var list = (await _sut.ListAsync(folder, new ListOptions { Recurse = true }, CancellationToken)).Select(x => x.Path.Full).ToList();

        list.Should().BeEquivalentTo(new[] { file.Full, nested.Full, "/" + folder + "sub/", "/" + folder + "sub/deeper/" });
    }

    [Fact]
    public async Task ListAsync_DoesNotMatchSiblingFolderSharingNamePrefix()
    {
        var name = Guid.NewGuid().ToString();
        var inFolder = await WriteToRandomPathAsync(name + "/");
        await WriteToRandomPathAsync(name + "-sibling/");

        var list = (await _sut.ListAsync(name + "/", new ListOptions { Recurse = true }, CancellationToken)).Select(x => x.Path.Full).ToList();

        list.Should().BeEquivalentTo(new[] { inFolder.Full });
    }

    [Fact]
    public async Task DeleteAsync_OnlyRemovesTheGivenObject()
    {
        var folder = Guid.NewGuid() + "/";
        var deleted = await WriteToRandomPathAsync(folder);
        var kept = await WriteToRandomPathAsync(folder);

        await _sut.DeleteAsync(deleted, CancellationToken);

        (await _sut.ExistsAsync(deleted, CancellationToken)).Should().BeFalse();
        (await _sut.ExistsAsync(kept, CancellationToken)).Should().BeTrue();
    }

    [Fact]
    public async Task RenameAsync_PreservesContentTypeAndMetadata()
    {
        var metadata = new Dictionary<string, string> { { "key", "value" } };
        var source = await WriteToRandomPathAsync("", ".json", new WriteOptions { ContentType = "application/json", Metadata = metadata });
        var destination = RandomStoragePath();

        await _sut.RenameAsync(source, destination, CancellationToken);

        var entry = await _sut.GetAsync(destination, CancellationToken);
        entry!.ContentType.Should().Be("application/json");
        entry.Metadata.Should().BeEquivalentTo(metadata);
    }

    [Fact]
    public async Task RenameAsync_ToSamePath_KeepsObject()
    {
        var path = await WriteToRandomPathAsync();

        await _sut.RenameAsync(path, path, CancellationToken);

        (await ReadAllTextAsync(path)).Should().Be(_testFileContent);
    }

    [Fact]
    public async Task RenameAsync_WithMissingSource_DoesNothing()
    {
        var source = RandomStoragePath();
        var destination = RandomStoragePath();

        await _sut.RenameAsync(source, destination, CancellationToken);

        (await _sut.ExistsAsync(destination, CancellationToken)).Should().BeFalse();
    }

    [Fact]
    public async Task UpdateMetadataAsync_ReplacesMetadataAndKeepsContent()
    {
        var path = await WriteToRandomPathAsync("", ".txt", new WriteOptions { ContentType = "text/plain", Metadata = new Dictionary<string, string> { { "old", "1" } } });
        var metadata = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };

        await ((ISupportsMetadata)_sut).UpdateMetadataAsync(path, new UpdateStorageEntryRequest { Metadata = metadata }, CancellationToken);
        metadata["key3"] = "added after the update";

        var entry = await _sut.GetAsync(path, CancellationToken);
        entry!.Metadata.Should().BeEquivalentTo(new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } });
        entry.ContentType.Should().Be("text/plain");
        (await ReadAllTextAsync(path)).Should().Be(_testFileContent);
    }

    [Fact]
    public async Task UpdateMetadataAsync_WithMissingObject_ThrowsFileNotFoundException()
    {
        var action = () => ((ISupportsMetadata)_sut).UpdateMetadataAsync(RandomStoragePath(), new UpdateStorageEntryRequest(), CancellationToken);

        await action.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task Operations_HonorCanceledToken()
    {
        var path = await WriteToRandomPathAsync();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await _sut.Invoking(x => x.ListAsync(null, default, cts.Token)).Should().ThrowAsync<OperationCanceledException>();
        await _sut.Invoking(x => x.GetAsync(path, cts.Token)).Should().ThrowAsync<OperationCanceledException>();
        await _sut.Invoking(x => x.WriteAsync(path, new MemoryStream(), null, cts.Token)).Should().ThrowAsync<OperationCanceledException>();
        await _sut.Invoking(x => x.DeleteAsync(path, cts.Token)).Should().ThrowAsync<OperationCanceledException>();

        (await _sut.ExistsAsync(path, CancellationToken)).Should().BeTrue();
    }

    [Fact]
    public async Task ConcurrentWrites_AreAllStored()
    {
        var folder = Guid.NewGuid() + "/";

        var paths = await Task.WhenAll(Enumerable.Range(0, 200).Select(_ => Task.Run(() => WriteToRandomPathAsync(folder), CancellationToken)));

        var list = (await _sut.ListAsync(folder, cancellationToken: CancellationToken)).Select(x => x.Path).ToList();
        list.Should().BeEquivalentTo(paths);
    }

    private async Task<string> ReadAllTextAsync(StoragePath path)
    {
        using var stream = await _sut.OpenReadAsync(path, CancellationToken);
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        return await reader.ReadToEndAsync();
    }
}
