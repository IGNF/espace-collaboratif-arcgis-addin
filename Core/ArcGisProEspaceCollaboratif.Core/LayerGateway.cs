namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe représentant les caractéristiques d'une couche appartenant au <GEOGROUPE>
    /// </summary>
    public class LayerGateway
    {
        // <TYPE> GeoPortail </TYPE>
        public string type;

        // <NOM> ORTHOIMAGERY.ORTHOPHOTOS </NOM>
        public string nom;

        // <DESCRIPTION> Photographies aériennes </DESCRIPTION>
        public string description;
        
        // <MINZOOM> 0 </MINZOOM>
        public int minzoom;

        // <MAXZOOM> 20 </MAXZOOM>
        public int maxzoom;
        
        // <EXTENT> -180, -86, 180, 84 </EXTENT>
        public string extent;
        
        // <ROLE> Droit utilisateur sur la couche </ROLE>
        public string role;
        
        // <VISIBILITY> 1 </VISIBILITY>
        public int visibility;

        // <OPACITY> 1 </OPACITY>
        public int opacity;

        // <TILEZOOM>
        public string tilezoom;

        // <URL>
        public string url;

        public LayerGateway()
        {
            type = string.Empty;
            nom = string.Empty;
            description = string.Empty;
            minzoom = 0;
            maxzoom = 20;
            extent = string.Empty;
            role = string.Empty;
            visibility = 1;
            opacity = 1;
            tilezoom = string.Empty;
            url = string.Empty;
        }
    }
}
