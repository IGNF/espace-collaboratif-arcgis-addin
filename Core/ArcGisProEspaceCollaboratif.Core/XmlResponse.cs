using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using System.IO;
using System.Globalization;
using log4net;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe pour le parsing des réponses xml et l'extraction des informations nécessaires
    /// </summary>
    /// 
    public class XmlResponse
    {
        //la réponse du serveur (au format xml)
        private readonly string response;

        private XPathDocument docxpath;

        private XPathNavigator navigator;

        private readonly CultureInfo invC = CultureInfo.InvariantCulture;


        private readonly Logger riplogger = Logger.Instance;
        private ILog logger = LogManager.GetLogger(typeof(XmlResponse));


        /// <summary>
        /// Constructeur. 
        /// initialisation du XPathDocument
        /// </summary>
        /// <param name="response">la réponse reçue du serveur au format xml </param>
        public XmlResponse(string response)
        {
            this.response = response;
            docxpath = new XPathDocument(StringToStream(response));
            navigator = docxpath.CreateNavigator();
        }



        /// <summary>
        /// Contrôle la validité de la réponse. 
        /// Si le code erreur="OK", la réponse est valide
        /// </summary>
        /// <returns>Dictionary<string,string>  à 2 clés: message et code"</returns>
        public Dictionary<string, string> CheckResponseValidity()
        {
            Dictionary<string, string> errMessage = new Dictionary<string, string>();
            try
            {
                XPathExpression expr = navigator.Compile("/geors/REPONSE/ERREUR");
                XPathNodeIterator iterator = navigator.Select(expr);
                iterator.MoveNext();
                errMessage["message"] = EncodeToUTF8(iterator.Current.InnerXml);
                errMessage["code"] = EncodeToUTF8(iterator.Current.GetAttribute("code", ""));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return errMessage;
        }

        /// <summary>
        /// Extraction des Aleas
        /// </summary>
        /// <returns>une liste contenant les 2 aleas</returns>
        public List<string> GetAleas()
        {
            List<string> aleas = new List<string>();

            try
            {
                XPathExpression expr = navigator.Compile("/geors/REPONSE/ALEA1");
                XPathNodeIterator iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                {
                    aleas.Add(iterator.Current.InnerXml);
                }
                else throw new Exception("Problème de connexion");


                expr = navigator.Compile("/geors/REPONSE/ALEA2");
                iterator = navigator.Select(expr);
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
        public Dictionary<string, string> GetConnectValues() {

            Dictionary<string, string> connectValues = new Dictionary<string, string>();

            try
            {
                XPathExpression expr = navigator.Compile("/geors/REPONSE/ID_AUTEUR");
                XPathNodeIterator iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                    connectValues.Add("ID_AUTEUR", iterator.Current.InnerXml);
                else
                    throw new Exception("ID_AUTEUR inexistant dans la réponse xml");

                expr = navigator.Compile("/geors/REPONSE/JETON");
                iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                    connectValues.Add("JETON", iterator.Current.InnerXml);
                else
                    throw new Exception("JETON inexistant dans la réponse xml");


                expr = navigator.Compile("/geors/REPONSE/SITE");
                iterator = navigator.Select(expr);
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
        public string GetCurrentJeton() {
            string jeton = "";

            try
            {
                XPathExpression expr = navigator.Compile("/geors/REPONSE/JETON");
                XPathNodeIterator iterator = navigator.Select(expr);
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
        /// 
        /// </summary>
        /// <returns></returns>
        public List<LayerGeoportail> ExtractLayersFromCleGeoportailUser()
        {
            List<LayerGeoportail> layers = new List<LayerGeoportail>();
            try
            {
                navigator.MoveToRoot();
                XPathNodeIterator itLayer = navigator.SelectDescendants("Layer", "http://www.opengis.net/context", false);
                foreach (XPathNavigator layer in itLayer)
                {
                    XPathNodeIterator iteratorElement = layer.SelectDescendants(XPathNodeType.Element, false);
                    LayerGeoportail tmpLayer = new LayerGeoportail();
                    foreach (XPathNavigator element in iteratorElement)
                    {
                        if(element.Name == "Name")
                        {
                            if (string.IsNullOrEmpty(tmpLayer.Name))
                            {
                                tmpLayer.Name = EncodeToUTF8(element.InnerXml);
                            }
                        }
                        if (element.Name == "Title")
                        {
                            if (string.IsNullOrEmpty(tmpLayer.Title))
                            {
                                tmpLayer.Title = EncodeToUTF8(element.InnerXml);
                            }
                        }
                        if (element.Name == "Abstract")
                        {
                            if (string.IsNullOrEmpty(tmpLayer.Abstract))
                            {
                                tmpLayer.Abstract = EncodeToUTF8(element.InnerXml);
                            }
                        }
                    }
                    layers.Add(tmpLayer);
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("{0}\n{1}", ex.Message, ex.StackTrace));
                throw ex;
            }
            return layers;
        }

        /// <summary>
        /// Extraction du profil à partir de la réponse xml
        /// </summary>
        /// <returns>Profil profil de l'utilisateur</returns>
        public Profil ExtractProfil()
        {
            Profil profil = new Profil();
            try
            {
                string profilXpath = "/geors/AUTEUR/";
                XPathExpression expr = navigator.Compile(profilXpath + "NOM");
                XPathNodeIterator iterator = navigator.Select(expr);
                profil.Author = new Auteur();
                if (iterator.MoveNext())
                {
                    profil.Author.Nom = EncodeToUTF8(iterator.Current.InnerXml);
                }

                profilXpath = "/geors/PROFIL/";
                expr = navigator.Compile(profilXpath + "ID_GEOPROFIL");
                iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                {
                    profil.Id_Geoprofil = iterator.Current.InnerXml;
                }

                expr = navigator.Compile(profilXpath + "TITRE");
                iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                {
                    profil.Title = EncodeToUTF8(iterator.Current.InnerXml);
                }

                Groupe gr = new Groupe();
                expr = navigator.Compile(profilXpath + "ID_GEOGROUPE");
                iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                {
                    gr.Id = iterator.Current.InnerXml;

                }
                expr = navigator.Compile(profilXpath + "GROUPE");
                iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                {
                    gr.Name = EncodeToUTF8(iterator.Current.InnerXml);
                }
                profil.Group = gr;

                expr = navigator.Compile(profilXpath + "LOGO");
                iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                {
                    profil.Logo = iterator.Current.InnerXml;
                }

                expr = navigator.Compile(profilXpath + "FILTRE");
                iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                {
                    profil.Filter = iterator.Current.InnerXml;
                }

                expr = navigator.Compile(profilXpath + "PRIVE");
                iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                {
                    profil.Private = iterator.Current.InnerXml.Equals("1");
                }

                // Les thèmes associés au profil
                List<Theme> themes = new List<Theme>();
                List<string> filteredThemes = new List<string>();
                GetThemes(ref themes, ref filteredThemes);
                profil.Themes = themes;
                profil.FilteredThemes = filteredThemes;

                // Les infos sur tous les geogroupes de l'utilisateur
                profil.Geogroupes = GetGeoGroupes();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + "\n" + ex.StackTrace);
                throw ex;
            }
            return profil;
        }

        public int TransformStringOrInt(string ToTransform, ref string newString, bool toInt)
        {
            int newInt = 0;
            if (string.IsNullOrEmpty(ToTransform))
            {
                newString = "";
            }
            else
            {
                if (toInt)
                {
                    newInt = Int32.Parse(ToTransform);
                }
                if (newString != null)
                {
                    newString = EncodeToUTF8(ToTransform);
                }
            }
            
            return newInt;
        }


        /// <summary>
        /// Récupération des infos sur les balises <GEOGROUPE>
        /// </summary>
        /// <returns></returns>
        public List<GeoGroupe> GetGeoGroupes()
        {
            List<GeoGroupe> listGeoGroupe = new List<GeoGroupe>();
            try
            {
                navigator.MoveToRoot();
                XPathExpression expr = navigator.Compile("/geors/GEOGROUPE");
                XPathNodeIterator iterator = navigator.Select(expr);
                foreach (XPathNavigator val in iterator)
                {
                    // Infos générales sur le GeoGroupe
                    GeoGroupe tmpGeoGroupe = new GeoGroupe
                    {
                        Id = EncodeToUTF8(val.SelectSingleNode("ID_GEOGROUPE").Value),
                        Name = EncodeToUTF8(val.SelectSingleNode("NOM").Value),
                        CommentaryGeorem = EncodeToUTF8(val.SelectSingleNode("COMMENTAIRE_GEOREM").Value),
                        Layers = new List<LayerGateway>()
                    };

                    // Récupération des attributs des thèmes du GeoGroupe
                    XPathNodeIterator attiterator = val.Select("THEMES/ATTRIBUT");
                    ConcurrentDictionary<string, List<ThemeAttributes>> themesAttributesDict = GetThemesAttributes(attiterator);

                    // Récupérer uniquement les thèmes du guichet passés à travers le filtre
                    XPathNodeIterator filtreIterator = val.Select("FILTER");
                    tmpGeoGroupe.FilteredThemes = GetFilteredThemes(filtreIterator, tmpGeoGroupe.Id);

                    XPathNodeIterator themeiterator = val.Select("THEMES/THEME");
                    List<Theme> themes = new List<Theme>();
                    foreach (XPathNavigator th in themeiterator)
                    {
                        Theme tmpTheme = new Theme()
                        {
                            Group = new Groupe()
                        };

                        string nom = EncodeToUTF8(th.SelectSingleNode("NOM").Value);
                        tmpTheme.Group.Name = nom;
                        if (tmpGeoGroupe.FilteredThemes.Contains(nom))
                        {
                            tmpTheme.Filtered = true;
                        }
                        if (themesAttributesDict.ContainsKey(nom))
                        {
                            List<ThemeAttributes> tmp = new List<ThemeAttributes>();
                            themesAttributesDict.TryGetValue(nom, out tmp);
                            tmpTheme.Attributes = tmp;
                        }
                        tmpTheme.Group.Id = val.SelectSingleNode("ID_GEOGROUPE").Value;
                        themes.Add(tmpTheme);
                    }
                    tmpGeoGroupe.Themes = themes;

                    // Récupération des layers
                    XPathNodeIterator layersIterator = val.Select("LAYERS/LAYER");
                    List<LayerGateway> layersGateway = new List<LayerGateway>();
                    foreach (XPathNavigator lay in layersIterator)
                    {
                        LayerGateway layerGateway = new LayerGateway
                        {
                            Type = EncodeToUTF8(lay.SelectSingleNode("TYPE").Value),
                            Name = EncodeToUTF8(lay.SelectSingleNode("NOM").Value),
                            Description = EncodeToUTF8(lay.SelectSingleNode("DESCRIPTION").Value),
                            Minzoom = Int32.Parse(lay.SelectSingleNode("MINZOOM").Value),
                            Maxzoom = Int32.Parse(lay.SelectSingleNode("MAXZOOM").Value),
                            Extent = EncodeToUTF8(lay.SelectSingleNode("EXTENT").Value),
                            Role = EncodeToUTF8(lay.SelectSingleNode("ROLE").Value)
                        };

                        string newString = null;
                        layerGateway.Visibility = TransformStringOrInt(lay.SelectSingleNode("VISIBILITY").Value, ref newString, true);
                        layerGateway.Opacity = Double.Parse(lay.SelectSingleNode("OPACITY").Value, System.Globalization.CultureInfo.InvariantCulture);

                        XPathNavigator tilezoom = lay.SelectSingleNode("TILEZOOM");
                        if (tilezoom != null)
                        {
                            layerGateway.Tilezoom = TransformStringOrInt(tilezoom.Value, ref newString, true);
                        }
                        XPathNavigator url = lay.SelectSingleNode("URL");
                        if (url != null)
                        {
                            layerGateway.Url = EncodeToUTF8(url.Value);
                        }

                        layersGateway.Add(layerGateway);
                    }
                    tmpGeoGroupe.Layers = layersGateway;
                    listGeoGroupe.Add(tmpGeoGroupe);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + "\n" + ex.StackTrace);
                throw new Exception("Récupération des infos des GeoGroupes associés au profil");
            }
            return listGeoGroupe;
        }

        /// <summary>
        /// Conversion des caractères spéciaux de l'API
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <returns></returns>
        public string ConvertEncodedCharacters(string aConvertir)
        {
            string newString;
            Dictionary<string, string> charConversion = new Dictionary<string, string>()
            {
                { "\\u00e0", "à" },
                { "\\u00e2", "â" },
                { "\\u00e4", "ä" },
                { "\\u00e7", "ç" },
                { "\\u00e8", "è" },
                { "\\u00e9", "é" },
                { "\\u00ea", "ê" },
                { "\\u00eb", "ë" },
                { "\\u00ee", "î" },
                { "\\u00ef", "ï" },
                { "\\u00f4", "ô" },
                { "\\u00f6", "ö" },
                { "\\u00f9", "ù" },
                { "\\u00fb", "û" },
                { "\\u00fc", "ü" }
            };
            newString = aConvertir;
            foreach (KeyValuePair<string, string> kvp in charConversion)
            {
                newString = newString.Replace(kvp.Key, kvp.Value);
            }
            return newString;
        }

        /// <summary>
        /// Récupération des thèmes à afficher dans le profil
        /// </summary>
        /// <param name="groupFilters"></param>
        /// <param name="idGeogroupe"></param>
        /// <returns></returns>
        public List<string> GetFilter(MatchCollection groupFilters, string idGeogroupe)
        {
            List<string> filteredThemes = new List<string>();
            foreach (Match groupFilter in groupFilters)
            {
                string[] listElements = groupFilter.Value.Split(':');
                string idGroupe = listElements[1].Split(',')[0];
                bool processFilter = false;
                if (idGeogroupe == "")
                {
                    processFilter = true;
                }
                else
                {
                    int idGeogroupeInt = Int32.Parse(idGeogroupe);
                    int idGroupeInt = Int32.Parse(idGroupe);
                    int intDiff = idGeogroupeInt - idGroupeInt;
                    if (intDiff == 0)
                    {
                        processFilter = true;
                    }
                    else
                    {
                        processFilter = false;
                    }
                }
                if (processFilter)
                {
                    string listThemesTmp = listElements[listElements.Length - 1];
                    // listThemesTmp = ["Test_signalement","test levé","Theme_table_bool","Theme_table_recette_mobile"]}
                    int start = 1;
                    int length = listThemesTmp.Length - 2;
                    string sliceListThemesTmp = listThemesTmp.Substring(start, length);
                    // sliceListThemesTmp = "Test_signalement","test levé","Theme_table_bool","Theme_table_recette_mobile"
                    MatchCollection collection = Regex.Matches(sliceListThemesTmp, "\".*?\"");
                    foreach (Match match in collection)
                    {
                        string val = ConvertEncodedCharacters(match.Value.Trim('\"'));
                        filteredThemes.Add(val);
                    }
                }
            }
            return filteredThemes;
        }

        public ConcurrentDictionary<string, List<ThemeAttributes>> GetThemesAttributes(XPathNodeIterator iterator)
        {
            ConcurrentDictionary<string, List<ThemeAttributes>> themesAttributesDict = new ConcurrentDictionary<string, List<ThemeAttributes>>();
            foreach (XPathNavigator val in iterator)
            {
                ThemeAttributes themeAttribut = new ThemeAttributes
                {
                    Theme = EncodeToUTF8(val.SelectSingleNode("NOM").Value),
                    Name = EncodeToUTF8(val.SelectSingleNode("ATT").Value),
                    Type = EncodeToUTF8(val.SelectSingleNode("TYPE").Value),
                    Values = new List<string>()
                };
                XPathNavigator obligatoire = val.SelectSingleNode("OBLIGATOIRE");
                if (obligatoire != null)
                {
                    themeAttribut.Required = true;                   
                }

                XPathNavigator valVALEURS = val.SelectSingleNode("VALEURS");
                XPathNodeIterator valIt = valVALEURS.SelectChildren(XPathNodeType.Element);
                List<string> lTmp = new List<string>();
                foreach (XPathNavigator valeurs in valIt)
                {
                    if (valeurs.Name == "DEFAULTVAL")
                    {
                        themeAttribut.DefaultValue = EncodeToUTF8(valeurs.InnerXml);
                    }
                    if (valeurs.Name == "VAL")
                    {
                        lTmp.Add(EncodeToUTF8(valeurs.InnerXml));
                    }
                }
                themeAttribut.Values = lTmp;
                themesAttributesDict.AddOrUpdate(themeAttribut.Theme, new List<ThemeAttributes> { themeAttribut }, (nomTheme, attTheme) => { attTheme.Add(themeAttribut); return attTheme; });
            };
            return themesAttributesDict;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filtreIterator"></param>
        /// <returns></returns>
        public List<string> GetFilteredThemes(XPathNodeIterator filtreIterator, string idGeogroupe)
        {
            List<string> listFilter = new List<string>();
            if (filtreIterator.MoveNext())
            {
                string sfiltre = EncodeToUTF8(filtreIterator.Current.InnerXml);
                MatchCollection collection = Regex.Matches(sfiltre, "\\{.*?\\}");
                listFilter = GetFilter(collection, idGeogroupe);
            }
            return listFilter;
        }

        /// <summary>
        /// Extraction des thèmes associés au profil
        /// </summary>
        /// <returns></returns>
        public void GetThemes(ref List<Theme> themes, ref List<string> filteredThemes)
        {
            try
            {
                navigator.MoveToRoot();
                XPathExpression expr = navigator.Compile("/geors/THEMES/ATTRIBUT");
                XPathNodeIterator iterator = navigator.Select(expr);
                ConcurrentDictionary<string, List<ThemeAttributes>> themesAttributesDict = GetThemesAttributes(iterator);

                // Récupération du filtre sur les thèmes
                navigator.MoveToRoot();
                XPathExpression filtreExpr = navigator.Compile("/geors/PROFIL/FILTRE");
                XPathNodeIterator filtreIterator = navigator.Select(filtreExpr);
                filteredThemes = GetFilteredThemes(filtreIterator, "");
                
                // Récupération des thèmes avec leurs attributs et le filtre
                navigator.MoveToRoot();
                XPathExpression themeExpr = navigator.Compile("/geors/THEMES/THEME");
                XPathNodeIterator themeIterator = navigator.Select(themeExpr);
                foreach (XPathNavigator val in themeIterator)
                {
                    Theme tmpTheme = new Theme()
                    {
                        Group = new Groupe()
                    };

                    string nom = EncodeToUTF8(val.SelectSingleNode("NOM").Value);
                    tmpTheme.Group.Name = nom;
                    if (filteredThemes.Contains(nom))
                    {
                        tmpTheme.Filtered = true;
                    }
                    if (themesAttributesDict.ContainsKey(nom))
                    {
                        List<ThemeAttributes> tmp = new List<ThemeAttributes>();
                        themesAttributesDict.TryGetValue(nom, out tmp);
                        tmpTheme.Attributes = tmp;
                    }
                    tmpTheme.Group.Id = val.SelectSingleNode("ID_GEOGROUPE").Value;

                    themes.Add(tmpTheme);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + "\n" + ex.StackTrace);
                throw new Exception("Récupération des thèmes associés au profil");
            }
        }

        /// <summary>
        /// Extraction des remarques de la réponse xml
        /// </summary>
        /// <param name="remarques">la liste de remarques</param>
        /// <returns>la liste de remarques (dans l'ordre inverse d'identifiants)</returns>
        public List<Report> ExtractSignalements(List<Report> signalements)
        {
            SortedDictionary<UInt64, Report> dicSignalement = new SortedDictionary<ulong, Report>();
            dicSignalement = this.ExtractSignalements(dicSignalement);
            signalements = new List<Report>(dicSignalement.Values);
            signalements.Reverse();

            return signalements;
        }

        /// <summary>
        /// Extraction des remarques de la réponse xml
        /// </summary>
        /// <param name="remarques">SortedDictionary key:indentifiant de la remarque, value: la remarque</param>
        /// <returns>le dictionnaire de remarques</returns>
        public SortedDictionary<UInt64, Report> ExtractSignalements(SortedDictionary<UInt64, Report> signalements)
        {

            Report rem = null;
            string remXpath = "/geors/GEOREM";

            try
            {
                XPathExpression expr = navigator.Compile(remXpath);
                XPathNodeIterator iterator = navigator.Select(expr);

                foreach (XPathNavigator val in iterator)
                {
                    List<Theme> themes = new List<Theme>();
                    rem = new Report();
                    val.MoveToFollowing("ID_GEOREM", "");
                    rem.Id = Convert.ToUInt64(val.InnerXml);

                    val.MoveToFollowing("AUTORISATION", "");
                    rem.Autorisation = val.InnerXml;

                    val.MoveToParent();
                    XPathNodeIterator it = val.Select("THEME");
                    foreach (XPathNavigator v in it)
                    {
                        string nomGroupe = v.SelectSingleNode("NOM").Value;
                        string idGroupe = v.SelectSingleNode("ID_GEOGROUPE").Value;
               
                        nomGroupe = EncodeToUTF8( nomGroupe);


                        Theme theme = new Theme
                        {
                            Group = new Groupe()
                        };
                        theme.Group.Id = idGroupe;
                        theme.Group.Name = nomGroupe;
                        themes.Add(theme);
                    }

                    rem.Themes = themes;

                    val.MoveToFollowing("LIEN", "");
                    rem.Lien =  val.InnerXml.Replace("&amp;" , "&");
                    val.MoveToFollowing("LIEN_PRIVE", "");
                    rem.LienPrive = val.InnerXml.Replace("&amp;" , "&");

                    DateTime dateValue = new DateTime();
                    val.MoveToFollowing("DATE", "");
                    string d = val.InnerXml;
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
                    rem.Departement.Name = EncodeToUTF8( val.InnerXml);

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
                    gr.Name = val.InnerXml;
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

                     if (signalements.ContainsKey(rem.Id))
                     {                 
                         return signalements;
                     }
                     signalements.Add(rem.Id,rem);
                }          
            }

            catch (Exception e)
            {
                logger.Error(e.Message + "\n" + e.StackTrace);
                throw new Exception("Une erreur est survenue dans l'impor des signalements " + rem.Id);
            }

            return signalements;
        }

        /// <summary>
        ///  Extrait les croquis d'un signalement et les ajoute dans l'objet Signalement 
        ///  passé en paramètre
        /// </summary>
        /// <param name="rem">un objet Signalement</param>
        /// <param name="val">xpathnavigator</param>
        /// <returns>XPathNodeIterator</returns>
        private XPathNodeIterator GetCroquisForRem(Report rem, XPathNavigator val )
        {
            val.MoveToParent();
            XPathNodeIterator it = val.Select("CROQUIS/objet");

            List<Point> points = new List<Point>();
            foreach (XPathNavigator v in it)
            {
                Sketch croquis = new Sketch();
                Sketch.SketchType type =
                    (Sketch.SketchType)Enum.Parse(typeof(Sketch.SketchType), v.GetAttribute("type", ""), true);

                string nomCr = EncodeToUTF8(v.SelectSingleNode("nom").Value);

                //attributs
                XPathNodeIterator itAttribut = v.Select("attributs/attribut");
                foreach (XPathNavigator att in itAttribut)
                {
                    SketchAttributes attribut = new SketchAttributes
                    {
                        Nom = EncodeToUTF8(att.GetAttribute("name", "")),
                        Valeur = EncodeToUTF8(att.InnerXml)
                    };
                    croquis.AddAttribute(attribut);
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

                string sCoord = v.InnerXml;
                string s = " ";

                string[] tCoord = sCoord.Split(s.ToCharArray(0, 1));


                Point pt = new Point();
                for (int i = 0; i < tCoord.Length; i++)
                {                 
                    pt = new Point();
                    string[] latlon = tCoord[i].Split(',');
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
                croquis.Name = nomCr;
                rem.AddCroquis(croquis);
            }
            return it;
        }


        /// <summary>
        /// Extraction des documents attachés à un signalement
        /// </summary>
        /// <param name="rem">le signalement</param>
        /// <param name="val">XPathNavigator (le xml contenant le signalement)</param>
        private void GetDoc(Report rem, XPathNavigator val)
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
        private void GetGeoRep(Report rem, XPathNavigator val)
        {
            XPathNodeIterator it = val.Select("GEOREP");

            foreach (XPathNavigator v in it)
            {
                GeoReponse georep = new GeoReponse();

                Groupe gr = new Groupe
                {
                    Id = v.SelectSingleNode("ID_GEOREP").Value,
                    Name = v.SelectSingleNode("TITRE").Value
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
        /// Extraction des thèmes liés à un signalement
        /// </summary>
        /// <param name="valRem">XPathNavigator (le xml contenant le signalement)</param>
        /// <returns>la liste de thèmes</returns>
        private List<Theme> GetGeomRemThemes(XPathNavigator valRem)
        {
            List<Theme> themes = new List<Theme>();

           string remXpath ="/geors/GEOREM/THEME";

            try
            {    
                XPathNodeIterator iterator = valRem.SelectDescendants(XPathNodeType.Element, false);
             
                foreach (XPathNavigator val in iterator)
                {
                    Theme theme = new Theme
                    {
                        Group = new Groupe()
                    };

                    theme.Group.Name = val.SelectSingleNode(valRem.Compile(remXpath + "/NOM")).Value;
                    theme.Group.Id = val.SelectSingleNode(valRem.Compile(remXpath + "/ID_GEOGROUPE")).Value;
                    themes.Add(theme);    
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + "\n" + ex.StackTrace);
                throw new Exception("Erreur dans l'extraction des thèmes du signalement");

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
            string xpath = "/geors/PAGE/TOTAL";

            try
            {
                XPathExpression expr = navigator.Compile(xpath);
                XPathNodeIterator iterator = navigator.Select(expr);
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
        public string GetVersion()
        {
            string v = "";
            string xpath = "/geors";

            try
            {
                XPathExpression expr = navigator.Compile(xpath);
                XPathNodeIterator iterator = navigator.Select(expr);
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
        public string GetDate()
        {
            string sdate = "";
            string xpath = "/geors/PAGE/DATE";

            try
            {
                XPathExpression expr = navigator.Compile(xpath);
                XPathNodeIterator iterator = navigator.Select(expr);
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
        /// Transformation d'un string en Stream
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
        protected static string EncodeToUTF8(string str)
        {            
            byte[] bytes = Encoding.Default.GetBytes(str);
            str = Encoding.UTF8.GetString(bytes);
            return str;
        }
    }
}