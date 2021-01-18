using System;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe représentant un auteur
    /// </summary>
    public class Auteur
    {
        public String Id; //identifiant de l'auteur
        public String Nom;//nom de l'auteur
       
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
         public Auteur(String id, String nom)
        {
            this.Id  = id;
            this.Nom = nom;
        }
    }
}