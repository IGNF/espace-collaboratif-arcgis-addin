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
        public Groupe groupe;
        public List<ThemeAttribut> attributs;
        public bool filtered;

        public Theme()
        {
            groupe = new Groupe();
        }

        public Groupe Groupe { get; set; }
        public List<ThemeAttribut> Attributs { get; set; }
        public bool Filtered { get; set; }
    }
}
