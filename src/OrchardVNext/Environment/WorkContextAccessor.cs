using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OrchardVNext.Environment {
    public class WorkContextAccessor : IWorkContextAccessor {
        private readonly IServiceProvider _serviceProvider;

        // a different symbolic key is used for each tenant.
        // this guarantees the correct accessor is being resolved.
        readonly object _workContextKey = new object();

        [ThreadStatic]
        static ConcurrentDictionary<object, WorkContext> _threadStaticContexts;


        public WorkContextAccessor(IServiceProvider serviceProvider) {
            _serviceProvider = serviceProvider;
        }

        public WorkContext GetContext() {
            WorkContext workContext;
            return EnsureThreadStaticContexts().TryGetValue(_workContextKey, out workContext) ? workContext : null;
        }

        public IWorkContextScope CreateWorkContextScope() {
            return new ThreadStaticScopeImplementation(
                _serviceProvider,
                EnsureThreadStaticContexts(),
                _workContextKey);
        }

        static ConcurrentDictionary<object, WorkContext> EnsureThreadStaticContexts() {
            return _threadStaticContexts ?? (_threadStaticContexts = new ConcurrentDictionary<object, WorkContext>());
        }

        class ThreadStaticScopeImplementation : IWorkContextScope {
            readonly WorkContext _workContext;
            readonly Action _disposer;

            public ThreadStaticScopeImplementation(IServiceProvider serviceProvider, ConcurrentDictionary<object, WorkContext> contexts, object workContextKey) {
                _workContext = (WorkContext)serviceProvider.GetService(typeof(WorkContext));
                contexts.AddOrUpdate(workContextKey, _workContext, (a, b) => _workContext);

                _disposer = () => {
                    WorkContext removedContext;
                    contexts.TryRemove(workContextKey, out removedContext);
                };
            }

            void IDisposable.Dispose() {
                _disposer();
            }

            public WorkContext WorkContext {
                get { return _workContext; }
            }

            public TService Resolve<TService>() {
                return WorkContext.Resolve<TService>();
            }

            public bool TryResolve<TService>(out TService service) {
                return WorkContext.TryResolve(out service);
            }
        }
    }
}