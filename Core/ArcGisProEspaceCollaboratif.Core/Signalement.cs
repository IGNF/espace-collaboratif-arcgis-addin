using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe représentant un signalement
    /// </summary>
    public class Signalement
    {
        /// <summary>
        /// Identifiant du signalement
        /// </summary>
        public UInt64 Id;
        
        /// <summary>
        /// URL publique vers le signalement sur le site web de l'espace collaboratif.
        /// </summary>
        public String Lien;

        /// <summary>
        /// URL vers la partie privée du site web de l'Espace collaboratif.
        /// </summary>
        public String LienPrive;

        /// <summary>
        /// La date de création de la remarque EspaceCollaboratif.
        /// </summary>
        public DateTime DateCreation;

        /// <summary>
        /// La date de mise-à-jour du signalement.
        /// </summary>
        public DateTime DateMiseAJour;

        /// <summary>
        /// La date de validation du signalement.
        /// </summary>
        public DateTime DateValidation;

        /// <summary>
        /// La position du signalement (lon/lat)
        /// </summary>
        public Point Position = new Point() ;

        /// <summary>
        /// Le statut du signalement
        /// </summary>
        public Statut Statut;


        /// <summary>
        /// Le département où est situé le signalement (indicatif + nom)
        /// </summary>
        public Groupe Departement;

        /// <summary>
        /// La commune où est situé le signalement (nom)
        /// </summary>
        public String Commune;

        /// <summary>
        /// Le texte du message du signalement.
        /// </summary>
        public String Commentaire;

        /// <summary>
        /// L'auteur du signalement
        /// </summary>
        public Auteur Auteur;

        /// <summary>
        ///	Définit les droits d'action de l'utilisateur en cours sur le signalement.
        /// </summary>
        public String Autorisation;

        /// <summary>
        ///	
        /// </summary>
        public String Id_partition;

        /// <summary>
        /// Le groupe sous lequel l'auteur a créé le signalement. 
        /// </summary>
        public Groupe Groupe;

        /// <summary>
        /// les éventuelles réponses du signalement.
        /// </summary>
        public List<GeoReponse> Reponses = new List<GeoReponse>() ;

        /// <summary>
        /// les éventuelles croquis de la remarque EspaceCollaboratif.
        /// </summary>
        public List<Sketch> Sketch = new List<Sketch>();

        /// <summary>
        /// Les éventuels documents attachés au signalement
        /// </summary>
        public List<String> Documents = new List<String>();

        /// <summary>
        /// Les éventuels thèmes attachés au signalement
        /// </summary>
        public List<Theme> Themes = new List<Theme>();

        public String Hash;

        public String Source;

        /// <summary>
        /// brief Getter pour concaténer sur une seule ligne le nom de tous les thèmes contenus dans le signalement.
        /// return Un texte qui est la concaténation des noms de tous les thèmes contenus dans le signalement. 
        /// </summary>
        /// <returns></returns>
        public String ConcatenateThemes()
        {
            String result = "";

            for ( int i = 0; i < this.Themes.Count; i++ ){
                if (i != 0)
                {
                    result += ", ";
                }
                result += this.Themes[i].groupe.Nom;
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
        public String GetAuteurNom()
        {
            String result = "";
            if (!this.Auteur.Nom.Equals("")) { result = this.Auteur.Nom; }
            return result;
        }

        /// <summary>
        ///	\brief Getter de l'Id de l'auteur du signalement.
        /// \return L'Id de l'auteur ddu signalement.
        /// <returns></returns>
        public String GetAuteurId()
        {
            String result = "";
            if (!this.Auteur.Id.Equals("")) { result = this.Auteur.Id; }
            return result;
        }

        public double GetLongitude()
        {
            return this.Position.Longitude;
        }

        public double GetLatitude()
        {
            return this.Position.Latitude;
        }

        public String GetFirstDocument()
        {
            if (this.Documents.Count == 0)
            { return ""; }
            else
            { return this.Documents.First(); }                        
        }

        public String GetUrlDecodedComment()
        {
            return HttpUtility.UrlDecode(this.Commentaire);
        }

        public String ConcatenateReponseHTML()
        {
            String concatenate = "";

            if (this.Reponses.Count == 0)
            {
                concatenate = "<font color=\"red\">Pas de réponse actuellement pour la remarque n°" + this.Id + ".</font>";
            }
            else
            {
                for (int i = 0; i < this.Reponses.Count;  i++)
                {
                    concatenate += "<li><b><font color=\"green\">Réponse n°" + (this.Reponses.Count - i);

                    if (this.Reponses[i].Auteur.Nom.Length != 0)
                    {
                        concatenate += " par " + this.Utf8Encode(this.Reponses[i].Auteur.Nom);
                    }
                    if (this.Reponses[i].Date != null)
                    {
                        concatenate += " le " + this.Reponses[i].Date.ToString();
                    }
                    concatenate += ".</font></b><br>";
                    concatenate += "<b>" + HttpUtility.UrlDecode(this.Utf8Encode(this.Reponses[i].Titre())) + "</b><br>";
                    concatenate += "" + HttpUtility.UrlDecode(this.Reponses[i].Reponse) + "</li><br><br>";
                }
            }
            return concatenate;
        }

        public String ConcatenateReponse()
        {
            String concatenate = "";

            if (this.Reponses.Count == 0)
            {
                concatenate = "Pas de réponse actuellement pour la remarque n°" + this.Id + ".";
            }
            else
            {
                for (int i = 0; i < this.Reponses.Count; i++)
                {
                    concatenate += "Réponse n°" + (this.Reponses.Count - i);

                    if (this.Reponses[i].Auteur.Nom.Length != 0)
                    {
                        concatenate += " par " + this.Reponses[i].Auteur.Nom;
                    }
                    if (this.Reponses[i].Date != null)
                    {
                        concatenate += " le " + this.Reponses[i].Date.ToString();
                    }
                    concatenate += ".\n" + this.Reponses[i].Reponse + "\n";
                }
            }
            return concatenate;
        }

        /// <summary>
        /// Retourne les GeoReponse de la Remarque sous forme d'un XML.
        /// </summary>
        /// <returns></returns
        public System.Xml.Linq.XElement ReponsesEncodeToXML()
        {
            System.Xml.Linq.XElement reponsesXML = new System.Xml.Linq.XElement("GEOREM_GEOREP");
          
            foreach (ArcGisProEspaceCollaboratif.Core.GeoReponse uneGeoReponse in this.Reponses)
            {
                reponsesXML.Add(uneGeoReponse.EncodeToXML());
            }
            
            return reponsesXML;
        }

        public void SetPosition(ArcGisProEspaceCollaboratif.Core.Point position)
        {
            this.Position = position;
        }

        public void SetPosition(double longitude, double latitude)
        {
            this.Position = new ArcGisProEspaceCollaboratif.Core.Point( longitude, latitude);
        }

        public void ClearPosition()
        {
            this.Position = new ArcGisProEspaceCollaboratif.Core.Point();
        }

        public void SetCommentaire(String message)
        {
            this.Commentaire = message;
        }

        public void ClearCommentaire()
        {
            this.Commentaire = "";
        }

        public void AddDocument(String unDocument)
        {
            this.Documents.Add(unDocument);
        }

        public void AddDocument(List<String> listeDocument)
        {
            foreach (String document in listeDocument)
            {
                this.AddDocument(document);
            }
        }

        public void ClearDocuments()
        {
            this.Documents.Clear();
        }

        public void AddCroquis(Sketch unCroquis)
        {
            this.Sketch.Add(unCroquis);
        }

        public void AddCroquis(List<Sketch> listeCroquis)
        {
            foreach (Sketch croquis in listeCroquis)
            {
                this.AddCroquis(croquis);
            }
        }

        public void AddGeoReponse(GeoReponse uneReponse)
        {
            this.Reponses.Add(uneReponse);
        }

        public void ClearCroquis()
        {
            this.Sketch.Clear();
        }

        public void AddTheme(Theme unTheme)
        {
            this.Themes.Add(unTheme);
        }

        public void AddTheme(List<Theme> listeTheme)
        {
            foreach (Theme theme in listeTheme)
            {
                this.AddTheme(theme);
            }
        }

        public void ClearThemes()
        {
            this.Themes.Clear();
        }

        public  String Utf8Encode(String str)
        {
            return Encoding.UTF8.GetString(Encoding.Default.GetBytes(str));
        }  
    }
}
