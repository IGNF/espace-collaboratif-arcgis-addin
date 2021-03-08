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
        public Groupe Groupe { get; set; }
        public List<ThemeAttributes> Attributs { get; set; }
        public bool Filtered { get; set; }
    }
}
