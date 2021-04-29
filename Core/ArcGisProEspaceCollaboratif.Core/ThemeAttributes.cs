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
        public string ThemeName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string TagDisplay { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UserSelectedValue { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string,string> Values { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Required { get; set; } = false;
    }
}
