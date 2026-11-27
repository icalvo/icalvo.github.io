using System.Linq;
using System.Threading.Tasks;
using SWGen.FileSystems;

namespace SWGen.Razor;

public static class AbsoluteFilePathExExtensions
{
    public static async ValueTask<AbsolutePathEx?> GetFirstExistingFileInParentDirs(
        this AbsolutePathEx startDirPath, IFileSystem fs, string fileName)
    {
        foreach (var candidate in startDirPath.Unfold(x => x.IsRoot, x => x.Parent)
                     .Select(dirPath => dirPath / fileName))
        {
            if (await fs.File.ExistsAsync(candidate))
            {
                return candidate;
            }
        }

        return null;
    }
}
