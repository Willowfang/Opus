using Opus.Services.UI;
using Opus.Values;
using System.Collections.Generic;
using System.Resources;

namespace Opus.Services.Implementation.UI
{
    /// <summary>
    /// Default implementation of <see cref="ISchemeInstructions"/>.
    /// <para>
    /// Retrieves the correct localized information for a scheme from <see cref="Opus.Resources"/>.
    /// </para>
    /// </summary>
    public class SchemeInstructions : ISchemeInstructions
    {
        private ResourceManager? manager;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="schemeName"></param>
        public void SetScheme(string schemeName)
        {
            if (schemeName == SchemeNames.WORKCOPY) manager = Resources.Instructions.WorkCopy.ResourceManager;
            else if (schemeName == SchemeNames.MERGE) manager = Resources.Instructions.Merge.ResourceManager;
            else if (schemeName == SchemeNames.COMPOSE) manager = Resources.Instructions.Compose.ResourceManager;
            else manager = null;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns>Instructions for a given scheme.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Instruction[] Instructions()
        {
            List<Instruction> instructions = new List<Instruction>();
            if (manager == null) return instructions.ToArray();

            string? unsplit = manager.GetString("Steps");
            if (unsplit == null) return instructions.ToArray();

            string[] split = unsplit.Split(';');

            for (int i = 0; i < split.Length; i++)
            {
                string content = split[i].Trim() + ".";
                instructions.Add(new Instruction((i + 1).ToString() + ".", content));
            }

            return instructions.ToArray();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public string Title()
        {
            if (manager == null) return "";

            return manager.GetString("InstructionTitle") ?? "";
        }
    }
}
