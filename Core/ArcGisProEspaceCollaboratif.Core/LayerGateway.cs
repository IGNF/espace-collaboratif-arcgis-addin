namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe représentant les caractéristiques d'une couche appartenant au <GEOGROUPE>
    /// Les commentaires au-dessus des variables sonr des balises données en exemple
    /// l'url : https://qlf-collaboratif.ign.fr/collaboratif-develop/api/georem/geoaut_get.xml
    /// </summary>
    public class LayerGateway
    {
        /// <summary>
        /// Balise exemple <TYPE>WXSIGN</TYPE>
        /// ou <TYPE>WFS</TYPE>
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Balise exemple <NOM>CADASTRALPARCELS.PARCELLAIRE_EXPRESS</NOM>
        /// ou <NOM>piste_cyclable</NOM>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Balise exemple <DESCRIPTION>Plan cadastral informatisé vecteur de la DGFIP.</DESCRIPTION>
        /// ou <DESCRIPTION/>
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Balise exemple <MINZOOM>0</MINZOOM>
        /// </summary>
        public int Minzoom { get; set; }

        /// <summary>
        /// Balise exemple <MAXZOOM>20</MAXZOOM>
        /// </summary>
        public int Maxzoom { get; set; }

        /// <summary>
        /// Balise exemple <EXTENT>-70,-70,70,70</EXTENT>
        /// </summary>
        public string Extent { get; set; }

        /// <summary>
        /// Balise exemple <ROLE>edit</ROLE>
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Balise exemple <VISIBILITY>1</VISIBILITY>
        /// </summary>
        public int Visibility { get; set; }

        /// <summary>
        /// Balise exemple <OPACITY>1</OPACITY>
        /// </summary>
        public double Opacity { get; set; }

        /// <summary>
        /// Balise exemple <TILEZOOM>12</TILEZOOM>
        /// </summary>
        public int Tilezoom { get; set; }

        /// <summary>
        /// Balise exemple <URL>https://espacecollaboratif.ign.fr/gcms/wfs?service=wfs&databasename=demo_guichet</URL>
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// WMTS : le nom du service correspondant à la balise LAYER de l'espace collaboratif
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// WMTS : le format des images servis par le geoservice
        /// </summary>
        public string ImageFormat { get; set; }
    }
}
