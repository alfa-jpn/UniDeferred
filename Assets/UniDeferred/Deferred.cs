using System;
using System.Collections;

namespace UniDeferred {
    /// <summary>
    /// Delegate of Deferred IEnumerator.
    /// </summary>
    public delegate IEnumerator DeferredCoroutine(IDeferred deferred);

    public class Deferred {
        /// <summary>
        /// Defer.
        /// </summary>
        /// <param name="action">action</param>
        /// <returns>IPromise</returns>
        public static IPromise Defer<T>(Action<IDeferred> action) {
            return DeferredExecutor.Defer<T>(action).Run();
        }

        /// <summary>
        /// Defer.
        /// </summary>
        /// <param name="action">action</param>
        /// <returns>IPromise</returns>
        public static IPromise Defer<T>(DeferredCoroutine action) {
            return DeferredExecutor.Defer<T>(action).Run();
        }

        /// <summary>
        /// Defer.
        /// </summary>
        /// <param name="coroutine">action</param>
        /// <returns>IPromise</returns>
        public static IPromise Defer(UnityEngine.Coroutine coroutine) {
            return DeferredExecutor.Defer(coroutine).Run();
        }

        /// <summary>
        /// Execute pararel promises.
        /// </summary>
        /// <param name="promises">Promises.</param>
        /// <returns>IPromise</returns>
        public static IPromise When(params IPromise[] promises) {
            return Defer<IPromise[]>(d => {
                bool fail = false;
                int left = promises.Length;
                foreach (IPromise promise in promises) {
                    promise.Fail(p => {
                        fail = true;
                    }).Always(p => {
                        if (--left == 0) {
                            if (!fail) {
                                d.Resolve(promises);
                            } else {
                                d.Reject();
                            }
                        }
                    });
                }
            });
        }
    }

    public class Deferred<T> : IDeferred, IPromise {
        /// <summary>
        /// Error Message.
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        /// State.
        /// </summary>
        public StateType State { get; private set; }

        /// <summary>
        /// Progress value (0-1)
        /// </summary>
        public float ProgressValue { get; private set; }

        /// <summary>
        /// Promise.
        /// </summary>
        public IPromise Promise { get { return this; } }

        /// <summary>
        /// Result.
        /// </summary>
        private T _result;

        private Action<IPromise> _actionDone;
        private Action<IPromise> _actionFail;
        private Action<IPromise> _actionAlways;
        private Action<IPromise> _actionProgress;

        private DeferredExecutor _nextAction;

        public Deferred() {
            State = StateType.Pending;
        }

        /// <summary>
        /// Get Result.
        /// </summary>
        /// <typeparam name="Type">ResultType</typeparam>
        /// <returns>Result</returns>
        public Type GetResult<Type>() {
            if (typeof(Type) != typeof(T)) {
                throw new Exception(string.Format("Missing Type. ({0}!={1})", typeof(T).ToString(), typeof(Type).ToString()));
            }
            return (Type)(object)_result;
        }

        /// <summary>
        /// Set Action of Done.
        /// </summary>
        /// <param name="action">Action.</param>
        /// <returns>Promise</returns>
        public IPromise Done(Action<IPromise> action) {
            if (State == StateType.Resolved) {
                action(this);
            } else {
                _actionDone = action;
            }
            return this;
        }

        /// <summary>
        /// Set Action of Fail.
        /// </summary>
        /// <param name="action">Action.</param>
        /// <returns>Promise</returns>
        public IPromise Fail(Action<IPromise> action) {
            if (State == StateType.Rejected) {
                action(this);
            } else {
                _actionFail = action;
            }
            return this;
        }

        /// <summary>
        /// Set Action of Always.
        /// </summary>
        /// <param name="action">Action.</param>
        /// <returns>Promise</returns>
        public IPromise Always(Action<IPromise> action) {
            if (State != StateType.Pending) {
                action(this);
            } else {
                _actionAlways = action;
            }
            return this;
        }

        /// <summary>
        /// Set Action of Progress.
        /// </summary>
        /// <param name="action">Action.</param>
        /// <returns>Promise</returns>
        public IPromise Progress(Action<IPromise> action) {
            _actionProgress = action;
            return this;
        }

        /// <summary>
        /// Set Next action.
        /// </summary>
        /// <typeparam name="Type">ResultType</typeparam>
        /// <param name="action">Next action</param>
        /// <returns>Promise</returns>
        public IPromise Next<Type>(Action<IDeferred> action) {
            return SetOrRunExecutor(DeferredExecutor.Defer<Type>(action));
        }

        /// <summary>
        /// Set Next action.
        /// </summary>
        /// <typeparam name="Type">ResultType</typeparam>
        /// <param name="action">Next action</param>
        /// <returns>Promise</returns>
        public IPromise Next<Type>(DeferredCoroutine action) {
            return SetOrRunExecutor(DeferredExecutor.Defer<Type>(action));
        }

        /// <summary>
        /// Set Next action.
        /// </summary>
        /// <typeparam name="Type">ResultType</typeparam>
        /// <param name="action">Next action</param>
        /// <returns>Promise</returns>
        public IPromise Next(UnityEngine.Coroutine coroutine) {
            return SetOrRunExecutor(DeferredExecutor.Defer(coroutine));
        }

        /// <summary>
        /// Resolve.
        /// </summary>
        /// <param name="result">Result Value.</param>
        public void Resolve(object result = null) {
            if (result != null) {
                if (result.GetType() != typeof(T)) {
                    throw new Exception(string.Format("Missing Type. ({0}!={1})", result.GetType().ToString(), typeof(T).ToString()));
                } else {
                    _result = (T)result;
                }
            }
            State = StateType.Resolved;
            if (_actionDone != null) {
                _actionDone(this);
            }

            if (_actionAlways != null) {
                _actionAlways(this);
            }

            if (_nextAction != null) {
                _nextAction.Run();
            }
        }

        /// <summary>
        /// Resolve.
        /// </summary>
        /// <param name="result">Result Value.</param>
        public void Reject(string error = "") {
            Error = error;
            State = StateType.Rejected;
            if (_actionFail != null) {
                _actionFail(this);
            }

            if (_actionAlways != null) {
                _actionAlways(this);
            }
        }

        /// <summary>
        /// Progress.
        /// </summary>
        /// <param name="value">Progress Value(0-1)</param>
        public void Notify(float value) {
            ProgressValue = UnityEngine.Mathf.Clamp01(value);
            if (_actionProgress != null) {
                _actionProgress(this);
            }
        }

        /// <summary>
        /// Set or Run DeferredExecutor.
        /// </summary>
        /// <param name="executor">Deferred Executor, if State is Resolve, Run. Other set _nextAction.</param>
        /// <returns></returns>
        private IPromise SetOrRunExecutor(DeferredExecutor executor) {
            if (State == StateType.Resolved) {
                executor.Run();
            } else {
                _nextAction = executor;
            }
            return executor.Promise;
        }
    }
}
