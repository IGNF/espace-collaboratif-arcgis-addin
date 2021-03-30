using System;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe représentant un auteur
    /// </summary>
    public class Auteur
    {
        public string Id; //identifiant de l'auteur
        public string Nom;//nom de l'auteur
       
        /// <summary>
        /// constructeur
        /// </summary>
         public Auteur()
        {
            this.Id = "";
            this.Nom = "";
        }

        /// <summary>
        /// constructeur initialisant l'id et le nom
        /// </summary>
        /// <param name="id"></param>
        /// <param name="nom"></param>
         public Auteur(string id, string nom)
        {
            this.Id  = id;
            this.Nom = nom;
        }
    }
}