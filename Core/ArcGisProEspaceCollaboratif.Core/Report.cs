using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using static ArcGisProEspaceCollaboratif.Core.Status;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe représentant un signalement
    /// </summary>
    public class Report
    {
        #region Parameters

        /// <summary>
        /// Identifiant du signalement
        /// </summary>
        public UInt64 Id;
        
        /// <summary>
        /// URL publique vers le signalement sur le site web de l'espace collaboratif.
        /// </summary>
        public string Lien;

        /// <summary>
        /// URL vers la partie privée du site web de l'Espace collaboratif.
        /// </summary>
        public string LienPrive;

        /// <summary>
        /// La date de création du signalement EspaceCollaboratif.
        /// </summary>
        public DateTime DateCreation { get; set; }

        /// <summary>
        /// La date de mise à jour du signalement.
        /// </summary>
        public DateTime DateUpdate { get; set; }

        /// <summary>
        /// La date de validation du signalement.
        /// </summary>
        public DateTime DateValidation { get; set; }

        /// <summary>
        /// La position du signalement (lon/lat)
        /// </summary>
        public Point Position = new Point() ;

        /// <summary>
        /// Le statut du signalement
        /// </summary>
        public EnumStatus Status { get; set; }

        /// <summary>
        /// Le département où est situé le signalement (indicatif + nom)
        /// </summary>
        public Group Departement;

        /// <summary>
        /// La commune où est situé le signalement (nom)
        /// </summary>
        public string Commune;

        /// <summary>
        /// Le texte du commentaire lié au signalement.
        /// </summary>
        public string Commentary { get; set; }

        /// <summary>
        /// L'auteur du signalement
        /// </summary>
        public Author Author { get; set; }

        /// <summary>
        ///	Définit les droits d'action de l'utilisateur en cours sur le signalement.
        /// </summary>
        public string Authorisation { get; set; }

        /// <summary>
        ///	
        /// </summary>
        public string Id_partition;

        /// <summary>
        /// Le groupe sous lequel l'auteur a créé le signalement. 
        /// </summary>
        public Group Group { get; set; }

        /// <summary>
        /// les éventuelles réponses du signalement.
        /// </summary>
        public List<GeoResponse> Responses = new List<GeoResponse>() ;

        /// <summary>
        /// les éventuelles croquis du signalement EspaceCollaboratif.
        /// </summary>
        public List<Sketch> Sketch = new List<Sketch>();

        /// <summary>
        /// Les éventuels documents attachés au signalement
        /// </summary>
        public List<string> Documents = new List<string>();

        /// <summary>
        /// Les éventuels thèmes attachés au signalement
        /// </summary>
        public List<Theme> Themes = new List<Theme>();

        /// <summary>
        /// 
        /// </summary>
        public string Hash;

        /// <summary>
        /// 
        /// </summary>
        public string Source;

        #endregion

        /// <summary>
        /// brief Getter pour concaténer sur une seule ligne le nom de tous les thèmes contenus dans le signalement.
        /// return Un texte qui est la concaténation des noms de tous les thèmes contenus dans le signalement. 
        /// </summary>
        /// <returns></returns>
        public string ConcatenateThemes()
        {
            string result = "";

            for ( int i = 0; i < this.Themes.Count; i++ ){
                if (i != 0)
                {
                    result += ", ";
                }
                result += this.Themes[i].Group.Name;
            }
            return result;
        }

        /// <summary>
        /// brief Test pour savoir s'il n'y a pas de croquis contenu dans le signalement (Vrai s'il n'y en a pas).
        /// return True s'il n'y a pas de croquis associé au signalement. False dans le cas contraire.
        /// <returns></returns>
        public bool IsCroquisEmpty()
        {
            return this.Sketch.Count == 0;
        }

        /// <summary>
        ///	\brief Getter du nom de l'auteur du signalement.
        /// \return Le nom de l'auteur du signalement.
        /// <returns></returns>
        public string GetAuteurNom()
        {
            string result = "";
            if (!this.Author.Name.Equals("")) { result = this.Author.Name; }
            return result;
        }

        /// <summary>
        ///	\brief Getter de l'Id de l'auteur du signalement.
        /// \return L'Id de l'auteur ddu signalement.
        /// <returns></returns>
        public string GetAuteurId()
        {
            string result = "";
            if (!this.Author.Id.Equals("")) { result = this.Author.Id; }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double GetLongitude()
        {
            return this.Position.Longitude;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double GetLatitude()
        {
            return this.Position.Latitude;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetFirstDocument()
        {
            if (this.Documents.Count == 0)
            { return ""; }
            else
            { return this.Documents.First(); }                        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetUrlDecodedComment()
        {
            return HttpUtility.UrlDecode(this.Commentary);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ConcatenateReponseHTML()
        {
            string concatenate = "";

            if (this.Responses.Count == 0)
            {
                //concatenate = "<font color=\"red\">Pas de réponse actuellement pour le signalement n°" + this.Id + ".</font>";
                concatenate = "<font color=\"red\">Ce signalement n'a pas reçu de réponses.</font>";
            }
            else
            {
                for (int i = 0; i < this.Responses.Count;  i++)
                {
                    concatenate += "<li><b><font color=\"green\">Réponse n°" + (this.Responses.Count - i);

                    if (this.Responses[i].Author.Name.Length != 0)
                    {
                        concatenate += " par " + this.EncodeToUTF8(this.Responses[i].Author.Name);
                    }
                    if (!string.IsNullOrEmpty(this.Responses[i].Date.ToString()))
                    {
                        concatenate += " le " + this.Responses[i].Date.ToString();
                    }
                    concatenate += ".</font></b><br>";
                    concatenate += "<b>" + HttpUtility.UrlDecode(this.EncodeToUTF8(this.Responses[i].Titre())) + "</b><br>";
                    concatenate += "" + HttpUtility.UrlDecode(this.Responses[i].Response) + "</li><br><br>";
                }
            }
            return concatenate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ConcatenateResponse()
        {
            string concatenate = "";

            if (this.Responses.Count == 0)
            {
                //concatenate = "Pas de réponse actuellement pour le signalement n°" + this.Id + ".";
                concatenate = "Ce signalement n'a pas reçu de réponses.";
            }
            else
            {
                for (int i = 0; i < this.Responses.Count; i++)
                {
                    concatenate += "Réponse n°" + (this.Responses.Count - i);

                    if (this.Responses[i].Author.Name.Length != 0)
                    {
                        concatenate += " par " + this.Responses[i].Author.Name;
                    }
                    if (!string.IsNullOrEmpty(this.Responses[i].Date.ToString()))
                    {
                        concatenate += " le " + this.Responses[i].Date.ToString();
                    }
                    concatenate += ".\n" + this.Responses[i].Response + "\n";
                }
            }
            return concatenate;
        }

        /// <summary>
        /// Retourne les GeoReponse du signalement sous forme d'un XML.
        /// </summary>
        /// <returns></returns
        public System.Xml.Linq.XElement ReponsesEncodeToXML()
        {
            System.Xml.Linq.XElement reponsesXML = new System.Xml.Linq.XElement("GEOREM_GEOREP");
          
            foreach (ArcGisProEspaceCollaboratif.Core.GeoResponse uneGeoReponse in this.Responses)
            {
                reponsesXML.Add(uneGeoReponse.EncodeToXML());
            }
            
            return reponsesXML;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        public void SetPosition(ArcGisProEspaceCollaboratif.Core.Point position)
        {
            this.Position = position;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        public void SetPosition(double longitude, double latitude)
        {
            this.Position = new ArcGisProEspaceCollaboratif.Core.Point( longitude, latitude);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearPosition()
        {
            this.Position = new ArcGisProEspaceCollaboratif.Core.Point();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unDocument"></param>
        public void AddDocument(string unDocument)
        {
            this.Documents.Add(unDocument);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listeDocument"></param>
        public void AddDocument(List<string> listeDocument)
        {
            foreach (string document in listeDocument)
            {
                this.AddDocument(document);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearDocuments()
        {
            this.Documents.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unCroquis"></param>
        public void AddCroquis(Sketch unCroquis)
        {
            this.Sketch.Add(unCroquis);
        }
 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="listeCroquis"></param>
        public void AddCroquis(List<Sketch> listeCroquis)
        {
            foreach (Sketch croquis in listeCroquis)
            {
                this.AddCroquis(croquis);
            }
        }
 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uneReponse"></param>
        public void AddGeoReponse(GeoResponse uneReponse)
        {
            this.Responses.Add(uneReponse);
        }
 
        /// <summary>
        /// 
        /// </summary>
        public void ClearCroquis()
        {
            this.Sketch.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unTheme"></param>
        public void AddTheme(Theme unTheme)
        {
            this.Themes.Add(unTheme);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listeTheme"></param>
        public void AddTheme(List<Theme> listeTheme)
        {
            foreach (Theme theme in listeTheme)
            {
                this.AddTheme(theme);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearThemes()
        {
            this.Themes.Clear();
        }

        /// <summary>
        /// Encode une chaîne de caractères en UTF8
        /// </summary>
        /// <param name="str">la chaîne de caractère</param>
        /// <returns>la chaîne de caractère en UTF8</returns>
        public string EncodeToUTF8(string str)
        {
            byte[] bytes = Encoding.Default.GetBytes(str);
            str = Encoding.UTF8.GetString(bytes);
            return str;
        }
    }
}
