using System;
using System.Collections.Generic;

namespace EspaceCollaboratif.Core
{
    /// <summary>
    /// Classe représentqnt le profil de l'utilisateur
    /// </summary>
    public class Profil
    {
        /// <summary>
        /// Nom de l'auteur
        /// </summary>
        public Auteur Auteur;
                
        /// <summary>
        /// Nom du Geogroupe
        /// </summary>
        public Groupe Geogroupe;

        /// <summary>
        /// Titre du Geogroupe
        /// </summary>
        public String Titre;
        
        /// <summary>
        /// Statut (privilèges) du profil
        /// </summary>
        public String Statut;

        /// <summary>
        /// Lien vers le logo du profil
        /// </summary>
        public String Logo;

        /// <summary>
        /// Filtre du profil
        /// </summary>
        public String Filtre;

        /// <summary>
        /// La zone géographique de travail du profil
        /// </summary>
        public ZoneGeographique Zone;

        /// <summary>
        /// Indique si le profil a acces aux groupes prives
        /// </summary>
        public bool Prive;

        /// <summary>
        /// Les éventuels thèmes attachés au profil
        /// </summary>
        public List<Theme> Themes = new List<Theme>();


        /// <summary>
        /// identifiant geoprofil
        /// </summary>
        public String Id_Geoprofil;

       




         

    }
}