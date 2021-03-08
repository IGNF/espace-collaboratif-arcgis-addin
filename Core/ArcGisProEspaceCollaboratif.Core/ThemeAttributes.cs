using System.Collections.Generic;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe représentant un attribut d'un thème
    /// </summary>
    public class ThemeAttributes
    {
        /// <summary>
        /// 
        /// </summary>
        public string Theme { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Nom { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Valeur { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string DefaultVal { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> Valeurs { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Obligatoire { get; set; } = false;
    }
}
