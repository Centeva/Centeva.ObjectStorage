using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Centeva.ObjectStorage.UnitTests;
public class FolderHelperTests
{
    [Fact]
    public void GetImpliedFolders_EmptyWhenNoFolderPaths()
    {
        List<StorageEntry> paths =
        [
            new("/one"),
            new("/two")
        ];

        FolderHelper.GetImpliedFolders(paths, "/").Should().BeEmpty();
    }

    [Fact]
    public void GetImpliedFolders_ExcludesExplicitFolders()
    {
        List<StorageEntry> paths =
        [
            new("/one/"),
            new("/two/three/")
        ];

        FolderHelper.GetImpliedFolders(paths, "/").Select(x => x.Path.Full).Should().Contain([
            "/two/"
        ]);
    }

    [Fact]
    public void GetImpliedFolders_IncludesUniqueFolderPaths()
    {
        List<StorageEntry> paths =
        [
            new("/one/two"),
            new("/one/three"),
            new("/four/five/six")
        ];

        FolderHelper.GetImpliedFolders(paths, "/").Select(x => x.Path.Full).Should().Contain(
        [
            "/one/",
            "/four/",
            "/four/five/"
        ]);
    }
}
