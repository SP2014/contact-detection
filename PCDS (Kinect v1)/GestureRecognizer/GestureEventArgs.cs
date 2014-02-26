
namespace GestureRecognizer
{
    using System;
    /// <summary>
    /// Gesture Event Args
    /// </summary>
    public class GestureEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public RecognitionResult Result { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GestureEventArgs" /> class.
        /// </summary>
        /// <param name="result">The result.</param>
        public GestureEventArgs(RecognitionResult result)
        {
            this.Result = result;
        }
    }
}