using System.Collections.Generic;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe représentant les infos du <GEOGROUPE> de l'utilisateur
    /// </summary>
    public class GeoGroupe
    {
        // ID Geogroupe
        public string Id { get; set; }

        // Nom du Geogroupe
        public string Nom { get; set; }

        // Couches visibles sur les cartes de ce groupe (dans l'ordre dans lequel les superposer)
        public List<LayerGateway> Layers { get; set; }

        // Thèmes du groupe
        public List<Theme> Themes { get; set; }

        // Thèmes filtrés du groupe
        public List<string> FilteredThemes{ get; set; }

        public string CommentaireGeorem { get; set; }

    }
}
