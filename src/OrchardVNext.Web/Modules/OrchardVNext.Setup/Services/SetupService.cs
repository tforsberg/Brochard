using System.Collections.Generic;
using System.Linq;
using OrchardVNext.Environment;
using OrchardVNext.Environment.Configuration;
using OrchardVNext.Environment.Descriptor.Models;
using OrchardVNext.Environment.Extensions;
using OrchardVNext.Environment.ShellBuilders;
using OrchardVNext.Localization;

namespace OrchardVNext.Setup.Services {
    public class SetupService : ISetupService {
        private readonly ShellSettings _shellSettings;
        private readonly IOrchardHost _orchardHost;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContainerFactory _shellContainerFactory;
        private readonly ICompositionStrategy _compositionStrategy;
        //private readonly IProcessingEngine _processingEngine;
        private readonly IExtensionManager _extensionManager;

        public SetupService(
            ShellSettings shellSettings,
            IOrchardHost orchardHost,
            IShellSettingsManager shellSettingsManager,
            IShellContainerFactory shellContainerFactory,
            ICompositionStrategy compositionStrategy,
            //IProcessingEngine processingEngine,
            IExtensionManager extensionManager) {
            _shellSettings = shellSettings;
            _orchardHost = orchardHost;
            _shellSettingsManager = shellSettingsManager;
            _shellContainerFactory = shellContainerFactory;
            _compositionStrategy = compositionStrategy;
            //_processingEngine = processingEngine;
            _extensionManager = extensionManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ShellSettings Prime() {
            return _shellSettings;
        }

        public string Setup(SetupContext context) {
            string executionId;

            // The vanilla Orchard distibution has the following features enabled.
            string[] hardcoded = {
                // Framework
                "OrchardVNext.Framework",
                };

            context.EnabledFeatures = hardcoded.Union(context.EnabledFeatures ?? Enumerable.Empty<string>()).Distinct().ToList();

            var shellSettings = new ShellSettings(_shellSettings);


            var shellDescriptor = new ShellDescriptor {
                Features = context.EnabledFeatures.Select(name => new ShellFeature { Name = name })
            };

            var shellBlueprint = _compositionStrategy.Compose(shellSettings, shellDescriptor);

            // creating a standalone environment. 
            // in theory this environment can be used to resolve any normal components by interface, and those
            // components will exist entirely in isolation - no crossover between the safemode container currently in effect

            // must mark state as Running - otherwise standalone enviro is created "for setup"
            shellSettings.State = TenantState.Running;
            using (var environment = _orchardHost.CreateStandaloneEnvironment(shellSettings)) {
                executionId = CreateTenantData(context, environment);
            }

            _shellSettingsManager.SaveSettings(shellSettings);

            return null;
        }

        private string CreateTenantData(SetupContext context, IWorkContextScope environment) {
            return string.Empty;
        }
    }
}