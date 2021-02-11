using System.Collections.Generic;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe représentant les infos du <GEOGROUPE> de l'utilisateur
    /// </summary>
    public class GeoGroupe
    {
        // ID Geogroupe
        public string id;

        // Nom du Geogroupe
        public string nom;

        // Couches visibles sur les cartes de ce groupe (dans l'ordre dans lequel les superposer)
        public List<LayerGateway> layers;

        // Thèmes du groupe
        public List<Theme> themes;

        // Thèmes filtrés du groupe
        public List<Theme> filteredThemes;

        public string georemComment;

        public GeoGroupe()
        {
            id = string.Empty;
            nom = string.Empty;
            layers = new List<LayerGateway>();
            themes = new List<Theme>();
            filteredThemes = new List<Theme>();
            georemComment = string.Empty;
        }
    }
}
