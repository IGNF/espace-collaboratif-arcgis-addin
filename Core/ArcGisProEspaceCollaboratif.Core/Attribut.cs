using System;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe représentant un attribut (d'un croquis)
    /// sous forme nom/valeur
    /// </summary>
    public class Attribut
    {
        public String Nom ;   //nom de l'attribut
        public String Valeur ;//valeur de l'attribut

        public Attribut()
        {
            this.Nom = "";
            this.Valeur = "";
        }

        public Attribut(String nom, String valeur)
        {
            this.Nom = nom;
            this.Valeur = valeur;
        }
    }
}