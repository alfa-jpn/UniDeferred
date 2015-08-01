using System.Collections;
using UnityEngine;

namespace UniDeferred {
    public class DeferredExecutor {
        /// <summary>
        /// Coroutine Executor.
        /// </summary>
        protected static MonoBehaviour Executor {
            get {
                if (_executor == null) {
                    _executor = new GameObject("UniDeferred::Async::Executor").AddComponent<MonoBehaviour>();
                    _executor.gameObject.hideFlags = HideFlags.HideAndDontSave;
                }
                return _executor;
            }
        }
        private static MonoBehaviour _executor;

        /// <summary>
        /// Promise
        /// </summary>
        public IPromise Promise { get; private set; }

        /// <summary>
        /// Action
        /// </summary>
        private System.Action _action;

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="promise">Promise of action.</param>
        /// <param name="action">Deffered action.</param>
        private DeferredExecutor(IPromise promise, System.Action action) {
            Promise = promise;
            _action = action;
        }

        /// <summary>
        /// Run action.
        /// </summary>
        /// <returns>Promise</returns>
        public IPromise Run() {
            _action();
            return Promise;
        }

        /// <summary>
        /// Defer action.
        /// </summary>
        /// <typeparam name="T">Type of result value.</typeparam>
        /// <param name="action">action</param>
        /// <returns>DeferredExecutor</returns>
        public static DeferredExecutor Defer<T>(System.Action<IDeferred> action) {
            Deferred<T> d = new Deferred<T>();
            return new DeferredExecutor(d.Promise, () =>
                action(d)
            );
        }

        /// <summary>
        /// Defer deferredDelegate.
        /// </summary>
        /// <typeparam name="T">Type of result value.</typeparam>
        /// <param name="action">action</param>
        /// <returns>DeferredExecutor</returns>
        public static DeferredExecutor Defer<T>(DeferredCoroutine action) {
            Deferred<T> d = new Deferred<T>();
            return new DeferredExecutor(d.Promise, () =>
                Executor.StartCoroutine(action(d))
            );
        }

        /// <summary>
        /// Defer coroutine.
        /// </summary>
        /// <param name="coroutine">action</param>
        /// <returns>DeferredExecutor</returns>
        public static DeferredExecutor Defer(Coroutine coroutine) {
            Deferred<None> d = new Deferred<None>();
            return new DeferredExecutor(d.Promise, () =>
                Executor.StartCoroutine(ExecuteCoroutineAndResolve(coroutine, d))
            );
        }

        /// <summary>
        /// Execute coroutine and resolve deferred.
        /// </summary>
        /// <param name="coroutine">Coroutine</param>
        /// <param name="deferred">Deferred</param>
        /// <returns></returns>
        private static IEnumerator ExecuteCoroutineAndResolve(Coroutine coroutine, IDeferred deferred) {
            yield return coroutine;
            deferred.Resolve();
        }
    }
}
