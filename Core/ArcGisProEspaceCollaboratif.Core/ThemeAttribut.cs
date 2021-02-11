using System.Collections.Generic;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe représentant un attribut d'un thème
    /// </summary>
    public class ThemeAttribut
    {
        public string theme;
        public string nom;
        public string valeur;
        public string defaultVal;
        public List<string> valeurs;
        public string type;
        public string obligatoire;

        public string Theme { get; set; }
        public string Nom { get; set; }
        public string Valeur { get; set; }
        public string DefaultVal { get; set; }
        public List<string> Valeurs { get; set; }
        public string Type { get; set; }
        public string Obligatoire { get; set; }
    }
}
