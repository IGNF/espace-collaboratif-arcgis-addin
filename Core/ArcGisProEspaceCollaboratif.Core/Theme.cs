using System.Collections.Generic;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe représentant un thème
    /// </summary>
    public class Theme
    {
        /// <summary>
        /// Groupe du thème
        /// </summary>
        public Groupe Group { get; set; }

        /// <summary>
        /// La listes des attributs du thème
        /// </summary>
        public List<ThemeAttributes> Attributes { get; set; }

        /// <summary>
        /// Le thème est-il filtré ?
        /// </summary>
        public bool Filtered { get; set; }
    }
}
