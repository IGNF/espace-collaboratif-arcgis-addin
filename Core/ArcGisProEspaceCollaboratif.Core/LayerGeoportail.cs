using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// La classe décrivant une partie des caractéristiques d'une couche issue du Geoportail
    /// à partir de la requete https://wxs.ign.fr/choisirgeoportail/geoportail/wmts?service=WMTS&request=GetCapabilities
    /// </summary>
    public class LayerGeoportail
    {
        /// <summary>
        /// Balise exemple <Name>CADASTRALPARCELS.PARCELLAIRE_EXPRESS</Name>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Balise exemple <Title>PCI vecteur</Title>
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Balise exemple <Abstract>Plan cadastral informatisé vecteur de la DGFIP.</Abstract>
        /// </summary>
        public string Abstract { get; set; }
    }
}
