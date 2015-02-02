using System;
using Microsoft.Framework.DependencyInjection;

namespace OrchardVNext {
    public static class WorkContextExtensions {
        public static IWorkContextScope CreateWorkContextScope(this IServiceProvider serviceProvider) {
            return serviceProvider.GetService<IWorkContextAccessor>().CreateWorkContextScope();
        }
    }
}