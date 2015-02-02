using OrchardVNext.Environment.Configuration;
using OrchardVNext.Environment.ShellBuilders;

namespace OrchardVNext.Environment {
    public interface IOrchardHost {
        void Initialize();

        ShellContext CreateShellContext(ShellSettings settings);

        /// <summary>
        /// Can be used to build an temporary self-contained instance of a shell's configured code.
        /// Services may be resolved from within this instance to configure and initialize its storage.
        /// </summary>
        IWorkContextScope CreateStandaloneEnvironment(ShellSettings shellSettings);
    }

    public class DefaultOrchardHost : IOrchardHost {
        private readonly IShellContextFactory _shellContextFactory;
        public DefaultOrchardHost(IShellContextFactory shellContextFactory) {
            _shellContextFactory = shellContextFactory;
        }

        void IOrchardHost.Initialize() {
            Logger.Information("Initialize Host");


            Logger.Information("Host Initialized");
        }

        /// <summary>
        /// Creates a shell context based on shell settings
        /// </summary>
        public ShellContext CreateShellContext(ShellSettings settings) {
            if (settings.State == TenantState.Uninitialized) {
                Logger.Debug("Creating shell context for tenant {0} setup", settings.Name);
                return _shellContextFactory.CreateSetupContext(settings);
            }

            Logger.Debug("Creating shell context for tenant {0}", settings.Name);
            return _shellContextFactory.CreateShellContext(settings);
        }

        public IWorkContextScope CreateStandaloneEnvironment(ShellSettings shellSettings) {
            Logger.Debug("Creating standalone environment for tenant {0}", shellSettings.Name);
            
            var shellContext = CreateShellContext(shellSettings);
            var workContext = shellContext.LifetimeScope.CreateWorkContextScope();
            return new StandaloneEnvironmentWorkContextScopeWrapper(workContext, shellContext);
        }

        // To be used from CreateStandaloneEnvironment(), also disposes the ShellContext LifetimeScope.
        private class StandaloneEnvironmentWorkContextScopeWrapper : IWorkContextScope {
            private readonly ShellContext _shellContext;
            private readonly IWorkContextScope _workContextScope;

            public WorkContext WorkContext {
                get { return _workContextScope.WorkContext; }
            }

            public StandaloneEnvironmentWorkContextScopeWrapper(IWorkContextScope workContextScope, ShellContext shellContext) {
                _workContextScope = workContextScope;
                _shellContext = shellContext;
            }

            public TService Resolve<TService>() {
                return _workContextScope.Resolve<TService>();
            }

            public bool TryResolve<TService>(out TService service) {
                return _workContextScope.TryResolve<TService>(out service);
            }

            public void Dispose() {
                _workContextScope.Dispose();
                _shellContext.Dispose();
            }
        }
    }
}