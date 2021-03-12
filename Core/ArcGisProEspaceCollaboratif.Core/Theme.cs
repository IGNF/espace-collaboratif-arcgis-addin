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
        public List<ThemeAttributes> Attributes { get; set; }
        public bool Filtered { get; set; }
    }
}
