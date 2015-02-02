using Microsoft.AspNet.Http;

namespace OrchardVNext {
    /// <summary>
    /// A work context for work context scope
    /// </summary>
    public abstract class WorkContext {
        /// <summary>
        /// Resolves a registered dependency type
        /// </summary>
        /// <typeparam name="T">The type of the dependency</typeparam>
        /// <returns>An instance of the dependency if it could be resolved</returns>
        public abstract T Resolve<T>();

        /// <summary>
        /// Tries to resolve a registered dependency type
        /// </summary>
        /// <typeparam name="T">The type of the dependency</typeparam>
        /// <param name="service">An instance of the dependency if it could be resolved</param>
        /// <returns>True if the dependency could be resolved, false otherwise</returns>
        public abstract bool TryResolve<T>(out T service);

        public abstract T GetState<T>(string name);
        public abstract void SetState<T>(string name, T value);

        /// <summary>
        /// The http context corresponding to the work context
        /// </summary>
        public HttpContext HttpContext {
            get { return GetState<HttpContext>("HttpContext"); }
            set { SetState("HttpContext", value); }
        }
    }
}