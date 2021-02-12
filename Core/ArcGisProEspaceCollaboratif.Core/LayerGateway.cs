namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe représentant les caractéristiques d'une couche appartenant au <GEOGROUPE>
    /// </summary>
    public class LayerGateway
    {
        // <TYPE> GeoPortail </TYPE>
        public string Type { get; set; }

        // <NOM> ORTHOIMAGERY.ORTHOPHOTOS </NOM>
        public string Nom { get; set; }

        // <DESCRIPTION> Photographies aériennes </DESCRIPTION>
        public string Description { get; set; }

        // <MINZOOM> 0 </MINZOOM>
        public int Minzoom { get; set; }

        // <MAXZOOM> 20 </MAXZOOM>
        public int Maxzoom { get; set; }

        // <EXTENT> -180, -86, 180, 84 </EXTENT>
        public string Extent { get; set; }

        // <ROLE> Droit utilisateur sur la couche </ROLE>
        public string Role { get; set; }

        // <VISIBILITY> 1 </VISIBILITY>
        public int Visibility { get; set; }

        // <OPACITY> 1 </OPACITY>
        public int Opacity { get; set; }

        // <TILEZOOM>
        public int Tilezoom { get; set; }

        // <URL>
        public string Url { get; set; }

    }
}
