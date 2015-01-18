using Microsoft.AspNet.FileSystems;
using Microsoft.Framework.Runtime;

namespace OrchardVNext.Environment {
    public abstract class HostEnvironment : IHostEnvironment {
        private readonly IApplicationEnvironment _applicationEnvrionment;

        public HostEnvironment(IApplicationEnvironment applicationEnvrionment) {
            _applicationEnvrionment = applicationEnvrionment;

            FileSystem = new PhysicalFileSystem(applicationEnvrionment.ApplicationBasePath);
        }

        public IFileSystem FileSystem { get; }

        public string MapPath(string virtualPath) {
            return virtualPath.Replace("~/", string.Empty);
        }
    }
}