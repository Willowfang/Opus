namespace Opus.Common.Services.Navigation
{
    /// <summary>
    /// A piece of instructions for a scheme.
    /// </summary>
    public class Instruction
    {
        /// <summary>
        /// Serial of the instruction piece .
        /// </summary>
        public string Serial { get; }

        /// <summary>
        /// Tekstual content of the instruction.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Create a new piece of instructions for a scheme.
        /// </summary>
        /// <param name="serial">Serial of this piece (e.g. "1.", "III").</param>
        /// <param name="content">Textual content of this piece.</param>
        public Instruction(string serial, string content)
        {
            Serial = serial;
            Content = content;
        }
    }
    /// <summary>
    /// A service for retrieving instructions for a scheme.
    /// </summary>
    public interface ISchemeInstructions
    {
        /// <summary>
        /// Set current scheme for instructions management.
        /// </summary>
        /// <param name="schemeName"></param>
        public void SetScheme(string schemeName);
        /// <summary>
        /// Get the localized title for a particular scheme.
        /// </summary>
        /// <returns>Title string</returns>
        public string Title();

        /// <summary>
        /// Get instructions for a particular scheme as a sequence.
        /// </summary>
        /// <returns>A sequence of instructions.</returns>
        public Instruction[] Instructions();
    }
}
