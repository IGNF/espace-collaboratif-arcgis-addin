using System;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe pour définir un objet réponse de EspaceCollaboratif.
    /// </summary>
    public class GeoResponse
    {

        /// <summary>
        /// Groupe contenant l'Id et le titre de l'object GeoReponse
        /// </summary>
        public Group Group { get; set; }

        /// <summary>
        /// L'auteur de la réponse
        /// </summary>
        public Author Author { get; set; }
        
        /// <summary>
        /// La réponse incluse dans l'object GeoReponse
        /// </summary>
        public string Reponse;

        /// <summary>
        /// La date de la réponse
        /// </summary>
        public DateTime Date;

        /// <summary>
        /// Le statut de la GeoReponse
        /// </summary>
        public Status Statut;
        
        /// <summary>
        /// Retourne l'id de la GeoReponse 
        /// </summary>
        /// <returns></returns>
        public string Id() { return this.Group.Id; }

        /// <summary>
        /// Retourne le titre de la GeoReponse 
        /// </summary>
        /// <returns></returns>
        public string Titre() { return 
            this.Group.Name; 
        }



        /// <summary>
        /// Retourne la GeoReponse sous forme de XML.
        /// </summary>
        /// <returns></returns
        public System.Xml.Linq.XElement EncodeToXML()
        {
            return new System.Xml.Linq.XElement(
                        new System.Xml.Linq.XElement("GEOREP",
                            new System.Xml.Linq.XElement("ID_GEOREP", this.Id()),
                            new System.Xml.Linq.XElement("ID_AUTEUR", this.Author.Id),
                            new System.Xml.Linq.XElement("AUTEUR", this.Author.Name),
                            new System.Xml.Linq.XElement("TITRE", this.Titre()),
                            new System.Xml.Linq.XElement("DATE", this.Date),
                            new System.Xml.Linq.XElement("REPONSE", this.Reponse),
                            new System.Xml.Linq.XElement("STATUT", this.Statut)

                        )
                    );
            
        }
        
    }
}