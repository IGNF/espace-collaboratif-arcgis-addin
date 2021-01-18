using System;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe pour définir un objet réponse de EspaceCollaboratif.
    /// </summary>
    public class GeoReponse
    {

        /// <summary>
        /// Groupe contenant l'Id et le titre de l'object GeoReponse
        /// </summary>
        public Groupe Groupe;

        /// <summary>
        /// L'auteur de la réponse
        /// </summary>
        public Auteur Auteur;
        
        /// <summary>
        /// La réponse incluse dans l'object GeoReponse
        /// </summary>
        public String Reponse;

        /// <summary>
        /// La date de la réponse
        /// </summary>
        public DateTime Date;

        /// <summary>
        /// Le statut de la GeoReponse
        /// </summary>
        public Statut Statut;
        
        /// <summary>
        /// Retourne l'id de la GeoReponse 
        /// </summary>
        /// <returns></returns>
        public string Id() { return this.Groupe.Id; }

        /// <summary>
        /// Retourne le titre de la GeoReponse 
        /// </summary>
        /// <returns></returns>
        public string Titre() { return 
            this.Groupe.Nom; 
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
                            new System.Xml.Linq.XElement("ID_AUTEUR", this.Auteur.Id),
                            new System.Xml.Linq.XElement("AUTEUR", this.Auteur.Nom),
                            new System.Xml.Linq.XElement("TITRE", this.Titre()),
                            new System.Xml.Linq.XElement("DATE", this.Date),
                            new System.Xml.Linq.XElement("REPONSE", this.Reponse),
                            new System.Xml.Linq.XElement("STATUT", this.Statut)

                        )
                    );
            
        }
        
    }
}