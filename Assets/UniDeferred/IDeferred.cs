namespace UniDeferred {
    public interface IDeferred {
        /// <summary>
        /// Resolve.
        /// </summary>
        /// <param name="result">Result Value.</param>
        void Resolve(object result = null);

        /// <summary>
        /// Reject.
        /// </summary>
        /// <param name="error">Error Message.</param>
        void Reject(string error = "");

        /// <summary>
        /// Notify.
        /// </summary>
        /// <param name="value">Progress value.</param>
        void Notify(float value);
    }
}
