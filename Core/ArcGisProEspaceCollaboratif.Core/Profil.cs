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
        public Auteur Auteur;
                
        /// <summary>
        /// ID & Nom du Geogroupe
        /// </summary>
        public Groupe groupe;

        /// <summary>
        /// Titre du Geogroupe
        /// </summary>
        public string Titre;
        
        /// <summary>
        /// Statut (privilèges) du profil
        /// </summary>
        public string Statut;

        /// <summary>
        /// Lien vers le logo du profil
        /// </summary>
        public string Logo;

        /// <summary>
        /// Filtre du profil
        /// </summary>
        public string Filter;

        /// <summary>
        /// La zone géographique de travail
        /// </summary>
        public ZoneGeographique Zone;

        /// <summary>
        /// Indique si le profil a acces aux groupes prives
        /// </summary>
        public bool Prive;

        /// <summary>
        /// Les éventuels thèmes attachés au profil
        /// </summary>
        public List<Theme> Themes;

        /// <summary>
        /// Les thèmes filtrés attachés au profil
        /// </summary>
        public List<string> filteredThemes;

        /// <summary>
        /// Identifiant geoprofil
        /// </summary>
        public string Id_Geoprofil;

        /// <summary>
        /// Les différents groupes de l'utilisateur
        /// </summary>
        public List<GeoGroupe> geogroupes;

        /// La liste des couches Geoportail visible avec la clé geoportail utilisateur
        public Dictionary<string,string> layersCleGeoportail;
    }
}
