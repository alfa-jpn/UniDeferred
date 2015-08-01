using System;

namespace UniDeferred {
    public interface IPromise {
        /// <summary>
        /// Error Message.
        /// </summary>
        string Error { get; }

        /// <summary>
        /// State.
        /// </summary>
        StateType State { get; }

        /// <summary>
        /// Progress value (0-1)
        /// </summary>
        float ProgressValue { get; }

        /// <summary>
        /// Get result.
        /// </summary>
        /// <typeparam name="Type">ResultType</typeparam>
        /// <returns>Result</returns>
        Type GetResult<Type>();

        /// <summary>
        /// Set action of done.
        /// </summary>
        /// <param name="action">Action.</param>
        /// <returns>Promise</returns>
        IPromise Done(Action<IPromise> action);

        /// <summary>
        /// Set action of fail.
        /// </summary>
        /// <param name="action">Action.</param>
        /// <returns>Promise</returns>
        IPromise Fail(Action<IPromise> action);

        /// <summary>
        /// Set action of always.
        /// </summary>
        /// <param name="action">Action.</param>
        /// <returns>Promise</returns>
        IPromise Always(Action<IPromise> action);

        /// <summary>
        /// Set action of progress.
        /// </summary>
        /// <param name="action">Action.</param>
        /// <returns>Promise</returns>
        IPromise Progress(Action<IPromise> action);

        /// <summary>
        /// Set Next action.
        /// </summary>
        /// <typeparam name="Type">ResultType</typeparam>
        /// <param name="action">Next action</param>
        /// <returns>Promise</returns>
        IPromise Next<Type>(Action<IDeferred> action);

        /// <summary>
        /// Set Next action.
        /// </summary>
        /// <typeparam name="Type">ResultType</typeparam>
        /// <param name="action">Next action</param>
        /// <returns>Promise</returns>
        IPromise Next<Type>(DeferredCoroutine action);

        /// <summary>
        /// Set Next action.
        /// </summary>
        /// <typeparam name="Type">ResultType</typeparam>
        /// <param name="action">Next action</param>
        /// <returns>Promise</returns>
        IPromise Next(UnityEngine.Coroutine coroutine);
    }
}
