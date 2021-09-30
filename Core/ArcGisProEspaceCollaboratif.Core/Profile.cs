using System.Collections.Generic;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe représentant le profil de l'utilisateur
    /// </summary>
    public class Profile
    {
        /// <summary>
        /// Nom de l'auteur
        /// </summary>
        public Author Author { get; set; }
                
        /// <summary>
        /// ID & Nom du Geogroupe
        /// </summary>
        public Group Group { get; set; }

        /// <summary>
        /// Titre du Geogroupe
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Statut (privilèges) du profil
        /// </summary>
        public string Statut { get; set; }

        /// <summary>
        /// Lien vers le logo du profil
        /// </summary>
        public string Logo { get; set; }

        /// <summary>
        /// Filtre du profil
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// La zone géographique de travail
        /// </summary>
        public ZoneGeographique Zone { get; set; }

        /// <summary>
        /// Indique si le profil a acces aux groupes prives
        /// </summary>
        public bool Private { get; set; }

        /// <summary>
        /// Les éventuels thèmes attachés au profil
        /// </summary>
        public List<Theme> Themes { get; set; } = new List<Theme>();

        /// <summary>
        /// Les thèmes filtrés attachés au profil
        /// </summary>
        public List<string> FilteredThemes { get; set; } = new List<string>();

        // <summary>
        /// Les thèmes globaux attachés au profil
        /// </summary>
        public List<Theme> GlobalThemes { get; set; } = new List<Theme>();
        public List<string> GlobalThemeNames { get; set; } = new List<string>();

        /// <summary>
        /// Identifiant geoprofil
        /// </summary>
        public string Id_Geoprofil { get; set; }

        /// <summary>
        /// Les différents groupes de l'utilisateur
        /// </summary>
        public List<GeoGroup> Geogroupes { get; set; }

        /// La liste des couches Geoportail visible avec la clé geoportail utilisateur
        public List<LayerGeoportail> LayersKeyGeoportail { get; set; }

        public (string, string, string) IdNameGroupKeyGeoPortail { get; set; }
    }
}
