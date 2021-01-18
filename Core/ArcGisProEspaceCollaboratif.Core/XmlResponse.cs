using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using System.IO;
using System.Globalization;
using log4net;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe pour le parsing des réponses xml et l'extraction des informations nécessaires
    /// </summary>
    /// 
    public class XmlResponse
    {
        //la réponse du serveur (au format xml)
        private readonly String response;
      
        private XPathDocument docxpath;
   
        private XPathNavigator nav;

        private readonly CultureInfo invC = CultureInfo.InvariantCulture;


        private readonly EspaceCollaboratifLogger riplogger = EspaceCollaboratifLogger.Instance;
        private ILog logger = LogManager.GetLogger(typeof(XmlResponse));


        /// <summary>
        /// Constructeur. 
        /// initialisation du XPathDocument
        /// </summary>
        /// <param name="response">la réponse reçue du serveur au format xml </param>
        public XmlResponse(String response)
        {
            this.response = response;
            docxpath = new XPathDocument(StringToStream(response));
            nav = docxpath.CreateNavigator();
        }

   

        /// <summary>
        /// Contrôle la validité de la réponse. 
        /// Si le code erreur="OK", la réponse est valide
        /// </summary>
        /// <returns>Dictionary<String,String>  à 2 clés: message et code"</returns>
        public Dictionary<String,String> CheckResponseValidity(){
           
            Dictionary<String,String> errMessage = new Dictionary<string,string>();

            try
            {
                XPathExpression expr =nav.Compile("/geors/REPONSE/ERREUR");
                XPathNodeIterator iterator = nav.Select(expr);
                iterator.MoveNext(); 
                errMessage["message"] = iterator.Current.InnerXml;
                errMessage["code"]=  iterator.Current.GetAttribute("code","");     
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return errMessage ;
        }


        /// <summary>
        /// Extraction des Aleas
        /// </summary>
        /// <returns>une liste contenant les 2 aleas</returns>
        public List<String> GetAleas()
        {
            List<String> aleas = new List<string>();

            try
            {
                XPathExpression expr = nav.Compile("/geors/REPONSE/ALEA1");
                XPathNodeIterator iterator = nav.Select(expr);
                if (iterator.MoveNext())
                {
                    aleas.Add(iterator.Current.InnerXml);
                }
                else throw new Exception("Problème de connexion");
               

                expr = nav.Compile("/geors/REPONSE/ALEA2");
                iterator = nav.Select(expr);
                if (iterator.MoveNext())
                {
                    aleas.Add(iterator.Current.InnerXml);
                }
                else throw new Exception("Problème de connexion");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + "\n" + ex.StackTrace);
                
            }

            return aleas;
        }



        /// <summary>
        /// Extraction des paramètres de connexion
        /// </summary>
        /// <returns>un dictionnaire contenant les paramètres de connexion (ID_AUTEUR, JETON, SITE)</returns>
        public Dictionary<String, String> GetConnectValues() {

            Dictionary<String, String> connectValues = new Dictionary<String, String>();

            try
            {
                XPathExpression expr = nav.Compile("/geors/REPONSE/ID_AUTEUR");
                XPathNodeIterator iterator = nav.Select(expr);
                if (iterator.MoveNext())
                    connectValues.Add("ID_AUTEUR", iterator.Current.InnerXml);
                else
                    throw new Exception("ID_AUTEUR inexistant dans la réponse xml");

                expr = nav.Compile("/geors/REPONSE/JETON");
                iterator = nav.Select(expr);
                if (iterator.MoveNext())
                    connectValues.Add("JETON", iterator.Current.InnerXml);
                else
                    throw new Exception("JETON inexistant dans la réponse xml");
                

                expr = nav.Compile("/geors/REPONSE/SITE");
                iterator = nav.Select(expr);
                if (iterator.MoveNext())         
                   connectValues.Add("SITE", iterator.Current.InnerXml);
                else
                    throw new Exception("SITE inexistant dans la réponse xml");
               
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + "\n" + ex.StackTrace);

                throw new Exception("Problème de connexion au serveur");
                               
            }
            return connectValues;
        }


        /// <summary>
        /// Extraction du nouveau jeton
        /// </summary>
        /// <returns>le jeton</returns>
        public String GetCurrentJeton(){
            String jeton = "";

            try
            {           
                XPathExpression expr = nav.Compile("/geors/REPONSE/JETON");
                XPathNodeIterator iterator = nav.Select(expr);
                if (iterator.MoveNext())
                    jeton = iterator.Current.InnerXml;
                else
                    throw new Exception();
            }
            catch (Exception e)
            {
                logger.Error("getCurrentJeton:" + e.Message + "\n" + e.StackTrace);
                throw new Exception("Jeton non valide");
             
            }

            return jeton;
        }


        /// <summary>
        /// Extraction du profil à partir de la réponse xml
        /// </summary>
        /// <returns>Profil profil de l'utilisateur</returns>
        public Profil ExtractProfil()
        {
            Profil profil = new Profil();

            String profilXpath ="/geors/PROFIL/";

            try
            {

                XPathExpression expr = nav.Compile(profilXpath+"ID_GEOPROFIL");
                XPathNodeIterator iterator = nav.Select(expr);
                if (iterator.MoveNext())
                    profil.Id_Geoprofil = iterator.Current.InnerXml;

                expr = nav.Compile(profilXpath + "TITRE");
                iterator = nav.Select(expr);
                if (iterator.MoveNext())           
                    profil.Titre = EncodeToUTF8(iterator.Current.InnerXml);


                expr = nav.Compile(profilXpath + "ZONE");
                iterator = nav.Select(expr);
                if (iterator.MoveNext())
                    profil.Zone = (ZoneGeographique)Enum.Parse(typeof(ZoneGeographique), iterator.Current.InnerXml);


                Groupe gr = new Groupe();
                expr = nav.Compile(profilXpath + "ID_GEOGROUPE");
                iterator = nav.Select(expr);
                if (iterator.MoveNext()) 
                     gr.Id = iterator.Current.InnerXml;

                expr = nav.Compile(profilXpath + "GROUPE");
                iterator = nav.Select(expr);
                if (iterator.MoveNext())      
                    gr.Nom = EncodeToUTF8(iterator.Current.InnerXml);

                profil.Geogroupe = gr;

                expr = nav.Compile(profilXpath + "LOGO");
                iterator = nav.Select(expr);
                if (iterator.MoveNext())     
                    profil.Logo= iterator.Current.InnerXml;

                expr = nav.Compile(profilXpath + "FILTRE");
                iterator = nav.Select(expr);
                if (iterator.MoveNext())         
                    profil.Filtre = iterator.Current.InnerXml;

                expr = nav.Compile(profilXpath + "PRIVE");
                iterator = nav.Select(expr);
                if (iterator.MoveNext())      
                    profil.Prive = iterator.Current.InnerXml.Equals("1");


                //va chercher les thèmes associés au profil
                List<Theme> themes =this.GetThemes();
                profil.Themes = themes;


                profilXpath = "/geors/AUTEUR/";
                expr = nav.Compile(profilXpath + "NOM");
                iterator = nav.Select(expr);
                profil.Auteur = new Auteur();
                if (iterator.MoveNext())
                    profil.Auteur.Nom = EncodeToUTF8(iterator.Current.InnerXml);


            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + "\n" + ex.StackTrace);
                throw ex;

            }
            return profil;
        }



        /// <summary>
        /// Extraction des thèmes associés au profil
        /// </summary>
        /// <returns></returns>
        public List<Theme> GetThemes()
        {
            List<Theme> themes = new List<Theme>();
             Profil profil = new Profil();

            String profilXpath ="/geors/THEMES/THEME";

            try
            {
                XPathExpression expr = nav.Compile(profilXpath);
                XPathNodeIterator iterator = nav.Select(expr);
             
                foreach (XPathNavigator val in iterator)
                {
                    Theme theme = new Theme
                    {
                        Groupe = new Groupe()
                    };

                    theme.Groupe.Nom = EncodeToUTF8(val.SelectSingleNode("NOM").Value);
                    theme.Groupe.Id = val.SelectSingleNode("ID_GEOGROUPE").Value;     
                    themes.Add(theme);    
                }

          

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + "\n" + ex.StackTrace);
                throw new Exception("Récupération du profil");

            }
            return themes;

            
        }





        /// <summary>
        /// Extraction des remarques de la réponse xml
        /// </summary>
        /// <param name="remarques">la liste de remarques</param>
        /// <returns>la liste de remarques (dans l'ordre inverse d'identifiants)</returns>
        public List<Remarque> ExtractRemarques(List<Remarque> remarques)
        {
            
            SortedDictionary<UInt64, Remarque> dicRem = new SortedDictionary<ulong, Remarque>();
         
            dicRem= this.ExtractRemarques(dicRem);

            remarques = new List<Remarque>(dicRem.Values);

            remarques.Reverse();

            return remarques;

        }


        /// <summary>
        /// Extraction des remarques de la réponse xml
        /// </summary>
        /// <param name="remarques">SortedDictionary key:indentifiant de la remarque, value: la remarque</param>
        /// <returns>le dictionnaire de remarques</returns>
        public SortedDictionary<UInt64, Remarque> ExtractRemarques(SortedDictionary<UInt64, Remarque> remarques)
        {

            Remarque rem = null;
            String remXpath = "/geors/GEOREM";

            try
            {
                XPathExpression expr = nav.Compile(remXpath);
                XPathNodeIterator iterator = nav.Select(expr);

                foreach (XPathNavigator val in iterator)
                {
                    List<Theme> themes = new List<Theme>();
                    rem = new Remarque();
                    val.MoveToFollowing("ID_GEOREM", "");
                    rem.Id = Convert.ToUInt64(val.InnerXml);

                    val.MoveToFollowing("AUTORISATION", "");
                    rem.Autorisation = val.InnerXml;

                    val.MoveToParent();
                    XPathNodeIterator it = val.Select("THEME");
                    foreach (XPathNavigator v in it)
                    {
                        String nomGroupe = v.SelectSingleNode("NOM").Value;
                        String idGroupe = v.SelectSingleNode("ID_GEOGROUPE").Value;
               
                        nomGroupe = EncodeToUTF8( nomGroupe);


                        Theme theme = new Theme
                        {
                            Groupe = new Groupe(idGroupe, nomGroupe)
                        };
                        themes.Add(theme);
                    }

                    rem.Themes = themes;

                    val.MoveToFollowing("LIEN", "");
                    rem.Lien =  val.InnerXml.Replace("&amp;" , "&");
                    val.MoveToFollowing("LIEN_PRIVE", "");
                    rem.LienPrive = val.InnerXml.Replace("&amp;" , "&");

                    DateTime dateValue = new DateTime();
                    val.MoveToFollowing("DATE", "");
                    String d = val.InnerXml;
                    rem.DateCreation = (d != null) ? Convert.ToDateTime(d) : Convert.ToDateTime("");

                    val.MoveToFollowing("MAJ", "");
                    d = val.InnerXml;
                    rem.DateMiseAJour = (d != null) ? Convert.ToDateTime(d) : Convert.ToDateTime("");

                    val.MoveToFollowing("DATE_VALID", "");
                    d = val.InnerXml;
                    if (DateTime.TryParse(d, out dateValue))
                    {
                        rem.DateValidation = Convert.ToDateTime(d);
                    }

                    val.MoveToFollowing("LON", "");
                    double lon = Double.Parse(val.InnerXml, invC);
                    val.MoveToFollowing("LAT", "");
                    double lat = Double.Parse(val.InnerXml, invC);
                    rem.Position = new Point(lon, lat);

                    val.MoveToFollowing("STATUT", "");
                    try
                    {
                        rem.Statut = (Statut)Enum.Parse(typeof(Statut), val.InnerXml, true);
                    }
                    catch (Exception e)
                    {
                        //do nothing...
                        logger.Error("Erreur rem.Statut non valide :"+val.InnerXml + " pour remId="+ rem.Id +" "+e.Message + "\n" + e.StackTrace);
                    }

                    rem.Departement = new Groupe();
                    val.MoveToFollowing("ID_DEP", "");
                    rem.Departement.Id = val.InnerXml;
                    val.MoveToFollowing("DEPARTEMENT", "");
                    rem.Departement.Nom = EncodeToUTF8( val.InnerXml);

                    val.MoveToFollowing("COMMUNE", "");
                    rem.Commune = EncodeToUTF8(val.InnerXml);
                    val.MoveToFollowing("COMMENTAIRE", "");
                    rem.Commentaire = EncodeToUTF8(val.InnerXml);

                    Auteur auteur = new Auteur();
                    val.MoveToFollowing("ID_AUTEUR", "");
                    auteur.Id = val.InnerXml;
                    val.MoveToFollowing("AUTEUR", "");
                    auteur.Nom = val.InnerXml;
                    rem.Auteur = auteur;

                    Groupe gr = new Groupe();
                    val.MoveToFollowing("ID_GEOGROUPE", "");
                    gr.Id = val.InnerXml;
                    val.MoveToFollowing("GROUPE", "");
                    gr.Nom = val.InnerXml;
                    rem.Groupe = gr;

                    val.MoveToFollowing("ID_PARTITION", "");
                    rem.Id_partition = val.InnerXml;

                    //croquis  
                    it = GetCroquisForRem(rem, val);

                    //documents  (DOC)
                     GetDoc(rem, val);

                    //réponses (GEOREP)
                     GetGeoRep(rem, val);
    
                     rem.Source = val.SelectSingleNode(val.Compile(remXpath + "/SOURCE")).Value;

                     if (remarques.ContainsKey(rem.Id))
                     {                 
                         return remarques;
                     }
                     remarques.Add(rem.Id,rem);
                }          
            }

            catch (Exception e)
            {
                logger.Error(e.Message + "\n" + e.StackTrace);
                throw new Exception("Une erreur est survenue dans l'importation des remarques");
            }

            return remarques;
        }




        /// <summary>
        ///  Extrait les croquis d'une remarque et les ajoute dans l'objet Remarque 
        ///  passé en paramètre
        /// </summary>
        /// <param name="rem">un objet Remarque</param>
        /// <param name="val">xpathnavigator</param>
        /// <returns>XPathNodeIterator</returns>
        private XPathNodeIterator GetCroquisForRem(Remarque rem, XPathNavigator val )
        {
            val.MoveToParent();
            XPathNodeIterator it = val.Select("CROQUIS/objet");

            List<Point> points = new List<Point>();
            foreach (XPathNavigator v in it)
            {
                Croquis croquis = new Croquis();
                Croquis.CroquisType type =
                    (Croquis.CroquisType)Enum.Parse(typeof(Croquis.CroquisType), v.GetAttribute("type", ""), true);

                String nomCr = EncodeToUTF8(v.SelectSingleNode("nom").Value);

                //attributs
                XPathNodeIterator itAttribut = v.Select("attributs/attribut");
                foreach (XPathNavigator att in itAttribut)
                {
                    Attribut attribut = new Attribut
                    {
                        Nom = EncodeToUTF8(att.GetAttribute("name", "")),
                        Valeur = EncodeToUTF8(att.InnerXml)
                    };
                    croquis.AddAttribut(attribut);
                }
                  
                v.MoveToFollowing("geometrie", "");

                v.MoveToChild(XPathNodeType.Element);

              
                while (!v.LocalName.Equals("coordinates"))
                {
                   
                    v.MoveToChild(XPathNodeType.Element);
                    if (!v.HasChildren)
                    {

                        v.MoveToFollowing(XPathNodeType.Element);
                    }
                }

                String sCoord = v.InnerXml;
                String s = " ";

                String[] tCoord = sCoord.Split(s.ToCharArray(0, 1));


                Point pt = new Point();
                for (int i = 0; i < tCoord.Length; i++)
                {                 
                    pt = new Point();
                    String[] latlon = tCoord[i].Split(',');
                    if (latlon.Length==4)
                    {
                        pt.Longitude = double.Parse(latlon[0] + "."+latlon[1], invC);
                        pt.Latitude = double.Parse(latlon[2] + "." + latlon[3], invC);                     
                    }
                    else if (latlon.Length==2) {
                        pt.Longitude = double.Parse(latlon[0], invC);
                        pt.Latitude = double.Parse(latlon[1], invC);
                    }
                    if (!double.IsNaN(pt.Latitude) && !double.IsNaN(pt.Longitude))
                    {
                        croquis.AddPoint(pt);
                    }
                    else
                    {
                        this.logger.Debug("none sCoord");
                    }
                }

                croquis.Type = type;
                croquis.Nom = nomCr;
                rem.AddCroquis(croquis);
            }
            return it;
        }


        /// <summary>
        /// Extraction des documents attachés à une remarque
        /// </summary>
        /// <param name="rem">la remarque</param>
        /// <param name="val">XPathNavigator (le xml contenant la remarque)</param>
        private void GetDoc(Remarque rem, XPathNavigator val)
        {
            XPathNodeIterator it = val.Select("DOC");
              
            foreach (XPathNavigator v in it)
            {
                rem.AddDocument(v.InnerXml);
            }
            
        }


        /// <summary>
        /// Extraction des réponses d'une remarque
        /// </summary>
        /// <param name="rem">la remarque</param>
        /// <param name="val">XPathNavigator (le xml contenant la remarque)</param>
        private void GetGeoRep(Remarque rem, XPathNavigator val)
        {
            XPathNodeIterator it = val.Select("GEOREP");

            foreach (XPathNavigator v in it)
            {
                GeoReponse georep = new GeoReponse();

                Groupe gr = new Groupe
                {
                    Id = v.SelectSingleNode("ID_GEOREP").Value,
                    Nom = v.SelectSingleNode("TITRE").Value
                };
                georep.Groupe = gr;

                georep.Auteur = new Auteur
                {
                    Id = v.SelectSingleNode("ID_AUTEUR").Value,

                    Nom = EncodeToUTF8(v.SelectSingleNode("AUTEUR").Value)
                };

                georep.Statut =(Statut) Enum.Parse(typeof(Statut),v.SelectSingleNode("STATUT").Value,true);
        
                georep.Date = Convert.ToDateTime(v.SelectSingleNode("DATE").Value);

                georep.Reponse= EncodeToUTF8(v.SelectSingleNode("REPONSE").Value);
               
                rem.AddGeoReponse(georep);
            } 
        }


        /// <summary>
        /// Extraction des thèmes liés à une remarque
        /// </summary>
        /// <param name="valRem">XPathNavigator (le xml contenant la remarque)</param>
        /// <returns>la liste de thèmes</returns>
        private List<Theme> GetGeomRemThemes(XPathNavigator valRem)
        {
            List<Theme> themes = new List<Theme>();

           String remXpath ="/geors/GEOREM/THEME";

            try
            {    
                XPathNodeIterator iterator = valRem.SelectDescendants(XPathNodeType.Element, false);
             
                foreach (XPathNavigator val in iterator)
                {
                    Theme theme = new Theme
                    {
                        Groupe = new Groupe()
                    };

                    theme.Groupe.Nom = val.SelectSingleNode(valRem.Compile(remXpath + "/NOM")).Value;
                    theme.Groupe.Id = val.SelectSingleNode(valRem.Compile(remXpath + "/ID_GEOGROUPE")).Value;
                    themes.Add(theme);    
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + "\n" + ex.StackTrace);
                throw new Exception("Erreur dans l'extraction des thèmes de la remarque");

            }
            return themes;           
        }


        /// <summary>
        /// Retourne le nombre total de réponses
        /// </summary>
        /// <returns>nombre total de réponses</returns>
        public int GetTotalResponse()
        {
            int total = 0;
            String xpath = "/geors/PAGE/TOTAL";

            try
            {
                XPathExpression expr = nav.Compile(xpath);
                XPathNodeIterator iterator = nav.Select(expr);
                iterator.MoveNext();

                total =int.Parse( iterator.Current.InnerXml);           
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + "\n" + ex.StackTrace);   
            }
            return total;
        }


        /// <summary>
        /// retourne la version du service EspaceCollaboratif
        /// </summary>
        /// <returns>version du service EspaceCollaboratif</returns>
        public String GetVersion()
        {
            String v = "";
            String xpath = "/geors";

            try
            {
                XPathExpression expr = nav.Compile(xpath);
                XPathNodeIterator iterator = nav.Select(expr);
                iterator.MoveNext();
                v=iterator.Current.GetAttribute("version", "");   

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + "\n" + ex.StackTrace);
            }

            return v;
        }


        /// <summary>
        /// Retourne la date/heure de la réponse
        /// </summary>
        /// <returns></returns>
        public String GetDate()
        {
            String sdate = "";
            String xpath = "/geors/PAGE/DATE";

            try
            {
                XPathExpression expr = nav.Compile(xpath);
                XPathNodeIterator iterator = nav.Select(expr);
                iterator.MoveNext();
                sdate = iterator.Current.InnerXml;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + "\n" + ex.StackTrace);
    
            }

            return sdate;
        }

        /// <summary>
        /// Transformation d'un String en Stream
        /// </summary>
        /// <param name="str">la chaîne de caractère à transformer</param>
        /// <returns>le Stream</returns>
        private Stream StringToStream(string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }



        /// <summary>
        /// Encode une chaîne de caractères en UTF8
        /// </summary>
        /// <param name="str">la chaîne de caractère</param>
        /// <returns>la chaîne de caractère en UTF8</returns>
        protected static String EncodeToUTF8(String str)
        {            
            byte[] bytes = Encoding.Default.GetBytes(str);
            str = Encoding.UTF8.GetString(bytes);
            return str;
        }
    }
}