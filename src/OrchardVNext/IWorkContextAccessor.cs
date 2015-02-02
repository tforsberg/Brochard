using System;

namespace OrchardVNext {
    public interface IWorkContextAccessor {
        WorkContext GetContext();
        IWorkContextScope CreateWorkContextScope();
    }

    public interface IWorkContextStateProvider : IDependency {
        Func<WorkContext, T> Get<T>(string name);
    }

    public interface IWorkContextScope : IDisposable {
        WorkContext WorkContext { get; }
        TService Resolve<TService>();
        bool TryResolve<TService>(out TService service);
    }
}