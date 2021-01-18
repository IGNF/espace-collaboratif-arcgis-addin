using System;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Un groupe EspaceCollaboratif
    /// </summary>
    public class Groupe
    {
        /// <summary>
        /// L'identifiant du groupe
        /// </summary>
        public String Id;
        /// <summary>
        /// Le nom du groupe
        /// </summary>
        public String Nom;

        /// <summary>
        /// constructeur
        /// </summary>
        public Groupe()
        {
            this.Id = "";
            this.Nom = "";
        }

        /// <summary>
        /// Constructeur 
        /// </summary>
        /// <param name="id">identifiant du groupe</param>
        /// <param name="nom">nom du groupe</param>
        public Groupe(String id, String nom)
        {
            this.Id  = id;
            this.Nom = nom;
        }
       


    }
}