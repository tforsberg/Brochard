using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using OrchardVNext.Environment.Configuration;
using OrchardVNext.Environment.ShellBuilders;

namespace OrchardVNext.Environment {
    public class OrchardContainerMiddleware {
        private readonly RequestDelegate _next;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IOrchardHost _orchardHost;


        public OrchardContainerMiddleware(
            RequestDelegate next,
            IShellSettingsManager shellSettingsManager,
            IOrchardHost orchardHost) {
            _next = next;
            _shellSettingsManager = shellSettingsManager;
            _orchardHost = orchardHost;
        }

        public async Task Invoke(HttpContext httpContext) {
            var currentApplicationServices = httpContext.ApplicationServices;
            var currentRequestServices = httpContext.RequestServices;

            var shellSettings = _shellSettingsManager.LoadSettings();
            if (shellSettings.Any()) {
                var shellSetting = shellSettings
                    .SingleOrDefault(x => x.RequestUrlPrefix == httpContext.Request.Host.Value);

                if (shellSetting != null) {
                    using (var shell = _orchardHost.CreateShellContext(shellSetting)) {
                        httpContext.RequestServices = shell.LifetimeScope;

                        shell.Shell.Activate();
                        await _next.Invoke(httpContext);
                    }
                }
                else {
                    // TODO: Throw a 404.
                    await _next.Invoke(httpContext);
                }
            }
            else {
                using (var shell = _orchardHost.CreateShellContext(
                    new ShellSettings { Name = ShellSettings.DefaultName, State = TenantState.Uninitialized })) {
                    httpContext.RequestServices = shell.LifetimeScope;

                    shell.Shell.Activate();
                    await _next.Invoke(httpContext);
                }
            }
        }
    }
}