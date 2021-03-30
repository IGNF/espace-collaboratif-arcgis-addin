using System;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe représentant un attribut (d'un croquis)
    /// sous forme nom/valeur
    /// </summary>
    public class SketchAttributes
    {
        public string Nom ;   //nom de l'attribut
        public string Valeur ;//valeur de l'attribut

        public SketchAttributes()
        {
            this.Nom = "";
            this.Valeur = "";
        }

        public SketchAttributes(string nom, string valeur)
        {
            this.Nom = nom;
            this.Valeur = valeur;
        }
    }
}