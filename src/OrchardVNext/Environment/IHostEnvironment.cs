using Microsoft.AspNet.FileSystems;

namespace OrchardVNext.Environment {
    /// <summary>
    /// Abstraction of the running environment
    /// </summary>
    public interface IHostEnvironment {
        IFileSystem FileSystem { get; }
        string MapPath(string virtualPath);
    }
}