using System;
using Microsoft.Framework.DependencyInjection;
using OrchardVNext.Mvc.Routes;

namespace OrchardVNext.Setup {
    public class SetupMode : IModule {
        public void Configure(IServiceCollection serviceCollection) {
            serviceCollection.AddScoped<IRoutePublisher, RoutePublisher>();
        }
    }
}