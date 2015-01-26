using OrchardVNext.Environment.Configuration;
using Microsoft.Framework.DependencyInjection;
using System;
using OrchardVNext.Environment.Descriptor.Models;
using System.Linq;
using OrchardVNext.Environment.Descriptor;

namespace OrchardVNext.Environment.ShellBuilders {
    /// <summary>
    /// High-level coordinator that exercises other component capabilities to
    /// build all of the artifacts for a running shell given a tenant settings.
    /// </summary>
    public interface IShellContextFactory {
        /// <summary>
        /// Builds a shell context given a specific tenant settings structure
        /// </summary>
        ShellContext CreateShellContext(ShellSettings settings);

        /// <summary>
        /// Builds a shell context for an uninitialized Orchard instance. Needed
        /// to display setup user interface.
        /// </summary>
        ShellContext CreateSetupContext(ShellSettings settings);
    }

    public class ShellContextFactory : IShellContextFactory {
        private readonly IShellDescriptorCache _shellDescriptorCache;
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IShellContainerFactory _shellContainerFactory;

        public ShellContextFactory(
            IShellDescriptorCache shellDescriptorCache,
            ICompositionStrategy compositionStrategy,
            IShellContainerFactory shellContainerFactory) {
            _shellDescriptorCache = shellDescriptorCache;
            _compositionStrategy = compositionStrategy;
            _shellContainerFactory = shellContainerFactory;
        }

        ShellContext IShellContextFactory.CreateShellContext(
            ShellSettings settings) {

            Logger.Debug("Creating shell context for tenant {0}", settings.Name);

            var knownDescriptor = _shellDescriptorCache.Fetch(settings.Name);
            if (knownDescriptor == null) {
                Logger.Information("No descriptor cached. Starting with minimum components.");
                knownDescriptor = MinimumShellDescriptor();
            }

            var blueprint = _compositionStrategy.Compose(settings, knownDescriptor);
            var provider = _shellContainerFactory.CreateContainer(settings, blueprint);

            try {
                return new ShellContext {
                    Settings = settings,
                    Blueprint = blueprint,
                    LifetimeScope = provider,
                    Shell = provider.GetService<IOrchardShell>()
                };
            }
            catch (Exception ex) {
                Logger.Error(ex.ToString());
                throw;
            }
        }

        private static ShellDescriptor MinimumShellDescriptor() {
            return new ShellDescriptor {
                SerialNumber = -1,
                Features = new[] {
                    new ShellFeature {Name = "OrchardVNext.Framework"}
                },
                Parameters = Enumerable.Empty<ShellParameter>(),
            };
        }

        ShellContext IShellContextFactory.CreateSetupContext(ShellSettings settings) {
            Logger.Debug("No shell settings available. Creating shell context for setup");

            var descriptor = new ShellDescriptor {
                SerialNumber = -1,
                Features = new[] {
                    new ShellFeature { Name = "OrchardVNext.Setup" },
                },
            };

            var blueprint = _compositionStrategy.Compose(settings, descriptor);
            var provider = _shellContainerFactory.CreateContainer(settings, blueprint);

            return new ShellContext {
                Settings = settings,
                Blueprint = blueprint,
                LifetimeScope = provider,
                Shell = provider.GetService<IOrchardShell>()
            };
        }
    }
}