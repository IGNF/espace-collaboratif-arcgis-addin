using System;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe représentant un attribut (d'un croquis)
    /// sous forme nom/valeur
    /// </summary>
    public class SketchAttributes
    {
        #region Parameters
        /// <summary>
        /// Nom de l'attribut
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Valeur de l'attribut
        /// </summary>
        public string Value { get; set; } = "";
        #endregion
    }
}