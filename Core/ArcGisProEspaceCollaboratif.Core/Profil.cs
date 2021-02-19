using System.Collections.Generic;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe représentant le profil de l'utilisateur
    /// </summary>
    public class Profil
    {
        /// <summary>
        /// Nom de l'auteur
        /// </summary>
        public Auteur Auteur { get; set; }
                
        /// <summary>
        /// ID & Nom du Geogroupe
        /// </summary>
        public Groupe Groupe { get; set; }

        /// <summary>
        /// Titre du Geogroupe
        /// </summary>
        public string Titre { get; set; }
        
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
        public bool Prive { get; set; }

        /// <summary>
        /// Les éventuels thèmes attachés au profil
        /// </summary>
        public List<Theme> Themes { get; set; }

        /// <summary>
        /// Les thèmes filtrés attachés au profil
        /// </summary>
        public List<string> FilteredThemes { get; set; }

        /// <summary>
        /// Identifiant geoprofil
        /// </summary>
        public string Id_Geoprofil { get; set; }

        /// <summary>
        /// Les différents groupes de l'utilisateur
        /// </summary>
        public List<GeoGroupe> Geogroupes { get; set; }

        /// La liste des couches Geoportail visible avec la clé geoportail utilisateur
        public Dictionary<string,string> LayersCleGeoportail { get; set; }

        public (string, string, string) IdNomGroupeCleGeoPortail { get; set; }
    }
}
