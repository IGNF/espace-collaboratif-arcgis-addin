using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using System.IO;
using log4net;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using static ArcGisProEspaceCollaboratif.Core.Status;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe pour le parsing des réponses xml et l'extraction des informations nécessaires
    /// </summary>
    /// 
    public class XmlResponse
    {
        //la réponse du serveur (au format xml)
        public readonly string response;

        public readonly XPathDocument docxpath;

        public readonly XPathNavigator navigator;

        /// <summary>
        /// Le logger qui permet d'enregistrer des informations sur le processus
        /// </summary>
        private static readonly Logger riplogger = Logger.Instance;
        private static readonly ILog logger = LogManager.GetLogger(typeof(XmlResponse));

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
                string xpath = "/geors/REPONSE/ERREUR";
                XPathExpression expr = navigator.Compile(xpath);
                XPathNodeIterator iterator = navigator.Select(expr);
                iterator.MoveNext();
                errMessage["message"] = EncodeToUTF8(iterator.Current.InnerXml);
                errMessage["code"] = EncodeToUTF8(iterator.Current.GetAttribute("code", ""));
            }
            catch (Exception e)
            {
                logger.Error(string.Format("XMLResponse.CheckResponseValidity : {0}\n", e.Message));
                throw new Exception(e.Message);
            }

            return errMessage;
        }

        /// <summary>
        /// Extraction des Aleas
        /// </summary>
        /// <returns>Une liste contenant les 2 aleas</returns>
        public List<string> GetAleas()
        {
            List<string> aleas = new List<string>();           
            try
            {
                string xpath = "/geors/REPONSE/ALEA1";
                XPathExpression expr = navigator.Compile(xpath);
                XPathNodeIterator iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                {
                    aleas.Add(iterator.Current.InnerXml);
                }
                else
                {
                    string message = string.Format("Balise '{0}' inexistante dans la réponse xml", xpath);
                    throw new ArgumentNullException(message);
                }

                xpath = "/geors/REPONSE/ALEA2";
                expr = navigator.Compile(xpath);
                iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                {
                    aleas.Add(iterator.Current.InnerXml);
                }
                else
                {
                    string message = string.Format("Balise '{0}' inexistante dans la réponse xml", xpath);
                    throw new ArgumentNullException(message);
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("XMLResponse.GetAleas : {0}\n", e.Message));
                throw new Exception(e.Message);
            }

            return aleas;
        }

        /// <summary>
        /// Extraction des paramètres de connexion
        /// </summary>
        /// <returns>Un dictionnaire contenant les paramètres de connexion (ID_AUTEUR, JETON, SITE)</returns>
        public Dictionary<string, string> GetConnectValues() {

            Dictionary<string, string> connectValues = new Dictionary<string, string>();
            try
            {
                string xpath = "/geors/REPONSE/ID_AUTEUR";
                XPathExpression expr = navigator.Compile(xpath);
                XPathNodeIterator iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                {
                    connectValues.Add("ID_AUTEUR", iterator.Current.InnerXml);
                }
                else
                {
                    string message = string.Format("Balise '{0}' inexistante dans la réponse xml", xpath);
                    throw new ArgumentNullException(message);
                }

                xpath = "/geors/REPONSE/JETON";
                expr = navigator.Compile(xpath);
                iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                {
                    connectValues.Add("JETON", iterator.Current.InnerXml);
                }
                else
                {
                    string message = string.Format("Balise '{0}' inexistante dans la réponse xml", xpath);
                    throw new ArgumentNullException(message);
                }

                xpath = "/geors/REPONSE/SITE";
                expr = navigator.Compile(xpath);
                iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                {
                    connectValues.Add("SITE", iterator.Current.InnerXml);
                }
                else
                {
                    string message = string.Format("Balise '{0}' inexistante dans la réponse xml", xpath);
                    throw new ArgumentNullException(message);
                }

            }
            catch (Exception e)
            {
                logger.Error(string.Format("XMLResponse.GetConnectValues : {0}\n", e.Message));
                throw new Exception(e.Message);
            }

            return connectValues;
        }

        /// <summary>
        /// Extraction du nouveau jeton
        /// </summary>
        /// <returns>Le jeton</returns>
        public string GetCurrentToken() {
            string token = "";
            try
            {
                string xpath = "/geors/REPONSE/JETON";
                XPathExpression expr = navigator.Compile(xpath);
                XPathNodeIterator iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                {
                    token = iterator.Current.InnerXml;
                }
                else
                {
                    throw new ArgumentNullException(string.Format("Balise '{0}' inexistante dans la réponse xml", xpath));
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("XMLResponse.GetCurrentToken : {0}\n", e.Message));
                throw new Exception(e.Message);
            }

            return token;
        }

        /// <summary>
        /// Extraction des couches Geoportail en fonction de la clé utilisateur
        /// </summary>
        /// <returns>La liste des couches Geoportail</returns>
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
            catch (Exception e)
            {
                logger.Error(string.Format("XMLResponse.ExtractLayersFromCleGeoportailUser : {0}\n", e.Message));
                throw new Exception(e.Message);
            }

            return layers;
        }

        /// <summary>
        ///  Extraction du profil à partir de la réponse xml fournie par le service
        /// </summary>
        /// <returns>Le profil de l'utilisateur</returns>
        public Profile ExtractProfile()
        {
            Profile profile = new Profile();
            try
            {
                string xpath = "/geors/AUTEUR/NOM";
                XPathExpression expr = navigator.Compile(xpath);
                XPathNodeIterator iterator = navigator.Select(expr);
                profile.Author = new Author();
                if (iterator.MoveNext())
                {
                    profile.Author.Name = EncodeToUTF8(iterator.Current.InnerXml);
                }
                else
                {
                    string message = string.Format("Balise '{0}' inexistante dans la réponse xml", xpath);
                    throw new ArgumentNullException(message);
                }

                string xpathprofil = "/geors/PROFIL";

                xpath = string.Format("{0}/ID_GEOPROFIL", xpathprofil);
                expr = navigator.Compile(xpath);
                iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                {
                    profile.Id_Geoprofil = iterator.Current.InnerXml;
                }
                else
                {
                    string message = string.Format("Balise '{0}' inexistante dans la réponse xml", xpath);
                    throw new ArgumentNullException(message);
                }

                xpath = string.Format("{0}/TITRE", xpathprofil);
                expr = navigator.Compile(xpath);
                iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                {
                    profile.Title = EncodeToUTF8(iterator.Current.InnerXml);
                }
                else
                {
                    string message = string.Format("Balise '{0}' inexistante dans la réponse xml", xpath);
                    throw new ArgumentNullException(message);
                }

                Group gr = new Group();
                xpath = string.Format("{0}/ID_GEOGROUPE", xpathprofil);
                expr = navigator.Compile(xpath);
                iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                {
                    gr.Id = iterator.Current.InnerXml;

                }
                else
                {
                    string message = string.Format("Balise '{0}' inexistante dans la réponse xml", xpath);
                    throw new ArgumentNullException(message);
                }

                xpath = string.Format("{0}/GROUPE", xpathprofil);
                expr = navigator.Compile(xpath);
                iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                {
                    gr.Name = EncodeToUTF8(iterator.Current.InnerXml);
                }
                else
                {
                    string message = string.Format("Balise '{0}' inexistante dans la réponse xml", xpath);
                    throw new ArgumentNullException(message);
                }
                profile.Group = gr;

                xpath = string.Format("{0}/LOGO", xpathprofil);
                expr = navigator.Compile(xpath);
                iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                {
                    profile.Logo = iterator.Current.InnerXml;
                }
                else
                {
                    string message = string.Format("Balise '{0}' inexistante dans la réponse xml", xpath);
                    throw new ArgumentNullException(message);
                }

                xpath = string.Format("{0}/FILTRE", xpathprofil);
                expr = navigator.Compile(xpath);
                iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                {
                    profile.Filter = iterator.Current.InnerXml;
                }
                else
                {
                    string message = string.Format("Balise '{0}' inexistante dans la réponse xml", xpath);
                    throw new ArgumentNullException(message);
                }

                xpath = string.Format("{0}/PRIVE", xpathprofil);
                expr = navigator.Compile(xpath);
                iterator = navigator.Select(expr);
                if (iterator.MoveNext())
                {
                    profile.Private = iterator.Current.InnerXml.Equals("1");
                }
                else
                {
                    string message = string.Format("Balise '{0}' inexistante dans la réponse xml", xpath);
                    throw new ArgumentNullException(message);
                }

                // Les thèmes associés au profil
                List<Theme> themes = new List<Theme>();
                List<string> filteredThemes = new List<string>();
                GetThemes(ref themes, ref filteredThemes);
                profile.Themes = themes;
                profile.FilteredThemes = filteredThemes;

                // Les infos sur tous les geogroupes de l'utilisateur
                profile.Geogroupes = GetGeoGroupes();
            }
            catch (Exception e)
            {
                logger.Error(string.Format("XMLResponse.ExtractProfile : {0}\n", e.Message));
                throw new Exception(e.Message);
            }
            return profile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ToTransform"></param>
        /// <param name="newString"></param>
        /// <param name="toInt"></param>
        /// <returns></returns>
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
        public List<GeoGroup> GetGeoGroupes()
        {
            List<GeoGroup> listGeoGroupe = new List<GeoGroup>();
            try
            {
                navigator.MoveToRoot();
                string xpath = "/geors/GEOGROUPE";
                XPathExpression expr = navigator.Compile(xpath);
                XPathNodeIterator iterator = navigator.Select(expr);
                if (iterator == null)
                {
                    string message = string.Format("Balise '{0}' inexistante dans la réponse xml", xpath);
                    throw new ArgumentNullException(message);
                }
                foreach (XPathNavigator val in iterator)
                {
                    // Infos générales sur le GeoGroupe
                    GeoGroup tmpGeoGroupe = new GeoGroup
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
                            Group = new Group()
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
                        layerGateway.Opacity = Double.Parse(lay.SelectSingleNode("OPACITY").Value, Constantes.invC);

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
            catch (Exception e)
            {
                logger.Error(string.Format("XMLResponse.GetGeoGroupes : {0}\n", e.Message));
                throw new Exception(e.Message);
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
                    /* listThemesTmp = ["Test_signalement","test levé","Theme_table_bool","Theme_table_recette_mobile"]} */
                    int start = 1;
                    int length = listThemesTmp.Length - 2;
                    string sliceListThemesTmp = listThemesTmp.Substring(start, length);
                    /* sliceListThemesTmp = "Test_signalement","test levé","Theme_table_bool","Theme_table_recette_mobile" */
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iterator"></param>
        /// <returns></returns>
        public ConcurrentDictionary<string, List<ThemeAttributes>> GetThemesAttributes(XPathNodeIterator iterator)
        {
            ConcurrentDictionary<string, List<ThemeAttributes>> themesAttributesDict = new ConcurrentDictionary<string, List<ThemeAttributes>>();
            ConcurrentDictionary<string, List<string>> attributesDisplay = new ConcurrentDictionary<string, List<string>>();
            ConcurrentDictionary<string, List<string>> valuesDisplay = new ConcurrentDictionary<string, List<string>>();
            try
            {
                foreach (XPathNavigator val in iterator)
                {
                    ThemeAttributes themeAttributes = new ThemeAttributes
                    {
                        ThemeName = EncodeToUTF8(val.SelectSingleNode("NOM").Value),
                        Type = EncodeToUTF8(val.SelectSingleNode("TYPE").Value),
                        TagName = EncodeToUTF8(val.SelectSingleNode("ATT").Value),
                        TagDisplay = EncodeToUTF8(val.SelectSingleNode("ATT").Value),
                        Values = new Dictionary<string, string>()
                    };

                    XPathNavigator pathNavigator = val.SelectSingleNode("ATT");
                    if (pathNavigator.HasAttributes)
                    {
                        themeAttributes.TagDisplay = EncodeToUTF8(pathNavigator.GetAttribute("display", ""));
                    }

                    XPathNavigator obligatoire = val.SelectSingleNode("OBLIGATOIRE");
                    if (obligatoire != null)
                    {
                        themeAttributes.Required = true;
                    }

                    XPathNavigator valVALEURS = val.SelectSingleNode("VALEURS");
                    XPathNodeIterator valIt = valVALEURS.SelectChildren(XPathNodeType.Element);
                    Dictionary<string, string> lTmp = new Dictionary<string, string>();
                    foreach (XPathNavigator valeurs in valIt)
                    {
                        if (valeurs.Name == "DEFAULTVAL")
                        {
                            themeAttributes.DefaultValue = EncodeToUTF8(valeurs.InnerXml);
                        }
                        if (valeurs.Name == "VAL")
                        {
                            string valeur = valeurs.InnerXml;
                            if (valeurs.HasAttributes)
                            {
                                valeur = valeurs.GetAttribute("display", "");
                            }

                            if (!lTmp.ContainsKey(valeurs.InnerXml))
                                lTmp.Add(EncodeToUTF8(valeurs.InnerXml), EncodeToUTF8(valeur));
                        }
                    }
                    themeAttributes.Values = lTmp;
                    themesAttributesDict.AddOrUpdate(themeAttributes.ThemeName, new List<ThemeAttributes> { themeAttributes }, (nomTheme, attTheme) => { attTheme.Add(themeAttributes); return attTheme; });
                };
            }
            catch (Exception e)
            {
                logger.Error(string.Format("XMLResponse.GetThemesAttributes : {0}\n", e.Message));
                throw new Exception(e.Message);
            }
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
                        Group = new Group()
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
            catch (Exception e)
            {
                logger.Error(string.Format("XMLResponse.GetThemes : {0}\n", e.Message));
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Extraction des signalements de la réponse xml
        /// </summary>
        /// <param name="signalements">la liste de signalements</param>
        /// <returns>La liste de signalements (dans l'ordre inverse d'identifiants)</returns>
        public List<Report> ExtractReports(List<Report> signalements)
        {
            SortedDictionary<UInt64, Report> dicSignalement = new SortedDictionary<ulong, Report>();
            dicSignalement = this.ExtractReports(dicSignalement);
            signalements = new List<Report>(dicSignalement.Values);
            signalements.Reverse();

            return signalements;
        }

        /// <summary>
        /// Extraction des remarques de la réponse xml
        /// </summary>
        /// <param name="remarques">SortedDictionary key:indentifiant du signalement, value: le signalement</param>
        /// <returns>le dictionnaire de signalements</returns>
        public SortedDictionary<UInt64, Report> ExtractReports(SortedDictionary<UInt64, Report> signalements)
        {
            Report report = null;
            try
            {
                string xpath = "/geors/GEOREM";
                XPathExpression expr = navigator.Compile(xpath);
                XPathNodeIterator iterator = navigator.Select(expr);
                if (iterator == null)
                {
                    string message = string.Format("Balise '{0}' inexistante dans la réponse xml", xpath);
                    throw new ArgumentNullException(message);
                }

                foreach (XPathNavigator val in iterator)
                {
                    List<Theme> themes = new List<Theme>();
                    report = new Report();
                    val.MoveToFollowing("ID_GEOREM", "");
                    report.Id = Convert.ToUInt64(val.InnerXml);

                    val.MoveToFollowing("AUTORISATION", "");
                    report.Authorisation = val.InnerXml;

                    val.MoveToParent();
                    XPathNodeIterator it = val.Select("THEME");
                    foreach (XPathNavigator v in it)
                    {
                        string nomGroupe = v.SelectSingleNode("NOM").Value;
                        string idGroupe = v.SelectSingleNode("ID_GEOGROUPE").Value;
                        nomGroupe = EncodeToUTF8( nomGroupe);
                        Theme theme = new Theme
                        {
                            Group = new Group()
                        };
                        theme.Group.Id = idGroupe;
                        theme.Group.Name = nomGroupe;
                        themes.Add(theme);
                    }

                    report.Themes = themes;

                    val.MoveToFollowing("LIEN", "");
                    report.Lien =  val.InnerXml.Replace("&amp;" , "&");
                    val.MoveToFollowing("LIEN_PRIVE", "");
                    report.LienPrive = val.InnerXml.Replace("&amp;" , "&");

                    DateTime dateValue = new DateTime();
                    val.MoveToFollowing("DATE", "");
                    string d = val.InnerXml;
                    report.DateCreation = (d != null) ? Convert.ToDateTime(d) : Convert.ToDateTime("");

                    val.MoveToFollowing("MAJ", "");
                    d = val.InnerXml;
                    report.DateUpdate = (d != null) ? Convert.ToDateTime(d) : Convert.ToDateTime("");

                    val.MoveToFollowing("DATE_VALID", "");
                    d = val.InnerXml;
                    if (DateTime.TryParse(d, out dateValue))
                    {
                        report.DateValidation = Convert.ToDateTime(d);
                    }

                    val.MoveToFollowing("LON", "");
                    double lon = Double.Parse(val.InnerXml, Constantes.invC);
                    val.MoveToFollowing("LAT", "");
                    double lat = Double.Parse(val.InnerXml, Constantes.invC);
                    report.Position = new Point(lon, lat);

                    val.MoveToFollowing("STATUT", "");
                    try
                    {
                        report.Status = (EnumStatus)Enum.Parse(typeof(EnumStatus), val.InnerXml, true);
                    }
                    catch (Exception e)
                    {
                        string message = string.Format("Erreur : Signalement.Statut non valide : {0} Id = {1}\n{2}", val.InnerXml, report.Id, e.Message);
                        throw new Exception(message);
                    }

                    report.Departement = new Group();
                    val.MoveToFollowing("ID_DEP", "");
                    report.Departement.Id = val.InnerXml;
                    val.MoveToFollowing("DEPARTEMENT", "");
                    report.Departement.Name = EncodeToUTF8( val.InnerXml);

                    val.MoveToFollowing("COMMUNE", "");
                    report.Commune = EncodeToUTF8(val.InnerXml);
                    val.MoveToFollowing("COMMENTAIRE", "");
                    report.Commentary = EncodeToUTF8(val.InnerXml);

                    Author author = new Author();
                    val.MoveToFollowing("ID_AUTEUR", "");
                    author.Id = val.InnerXml;
                    val.MoveToFollowing("AUTEUR", "");
                    author.Name = val.InnerXml;
                    report.Author = author;

                    Group gr = new Group();
                    val.MoveToFollowing("ID_GEOGROUPE", "");
                    gr.Id = val.InnerXml;
                    val.MoveToFollowing("GROUPE", "");
                    gr.Name = val.InnerXml;
                    report.Group = gr;

                    val.MoveToFollowing("ID_PARTITION", "");
                    report.Id_partition = val.InnerXml;

                    //croquis  
                    GetSketchForReport(report, val);

                    //documents  (DOC)
                     GetDocument(report, val);

                    //réponses (GEOREP)
                     GetGeoRep(report, val);
    
                     report.Source = val.SelectSingleNode(val.Compile(xpath + "/SOURCE")).Value;

                     if (signalements.ContainsKey(report.Id))
                     {                 
                         return signalements;
                     }
                     signalements.Add(report.Id,report);
                }          
            }

            catch (Exception e)
            {
                string message = string.Format("Une erreur est survenue dans l'import d'un signalement {0}\n{1}", report.Id, e.Message);
                logger.Error(string.Format("XMLResponse.ExtractReports : {0}\n", message));
                throw new Exception(message);
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
        private XPathNodeIterator GetSketchForReport(Report rem, XPathNavigator val )
        {
            val.MoveToParent();
            XPathNodeIterator it = val.Select("CROQUIS/objet");
            try
            {
                foreach (XPathNavigator v in it)
                {
                    Sketch sketch = new Sketch();
                    Sketch.SketchType type =
                        (Sketch.SketchType)Enum.Parse(typeof(Sketch.SketchType), v.GetAttribute("type", ""), true);

                    string nameSketch = EncodeToUTF8(v.SelectSingleNode("nom").Value);

                    //attributs
                    XPathNodeIterator itAttribut = v.Select("attributs/attribut");
                    foreach (XPathNavigator att in itAttribut)
                    {
                        SketchAttributes attribut = new SketchAttributes
                        {
                            Name = EncodeToUTF8(att.GetAttribute("name", "")),
                            Value = EncodeToUTF8(att.InnerXml)
                        };
                        sketch.AddAttribute(attribut);
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

                    for (int i = 0; i < tCoord.Length; i++)
                    {
                        Point pt = new Point();
                        string[] latlon = tCoord[i].Split(',');
                        if (latlon.Length == 4)
                        {
                            pt.Longitude = double.Parse(latlon[0] + "." + latlon[1], Constantes.invC);
                            pt.Latitude = double.Parse(latlon[2] + "." + latlon[3], Constantes.invC);
                        }
                        else if (latlon.Length == 2)
                        {
                            pt.Longitude = double.Parse(latlon[0], Constantes.invC);
                            pt.Latitude = double.Parse(latlon[1], Constantes.invC);
                        }
                        if (!double.IsNaN(pt.Latitude) && !double.IsNaN(pt.Longitude))
                        {
                            sketch.AddPoint(pt);
                        }
                        else
                        {
                            logger.Debug("none sCoord");
                        }
                    }

                    sketch.Type = type;
                    sketch.Name = nameSketch;
                    rem.AddCroquis(sketch);
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("XMLResponse.GetSketchForReport : {0}\n", e.Message));
                throw new Exception(e.Message);
            }
            return it;
        }


        /// <summary>
        /// Extraction des documents attachés à un signalement
        /// </summary>
        /// <param name="rem">le signalement</param>
        /// <param name="val">XPathNavigator (le xml contenant le signalement)</param>
        private void GetDocument(Report rem, XPathNavigator val)
        {
            XPathNodeIterator it = val.Select("DOC");            
            foreach (XPathNavigator v in it)
            {
                rem.AddDocument(v.InnerXml);
            }            
        }

        /// <summary>
        /// Extraction des réponses d'un signalement
        /// </summary>
        /// <param name="rem">le signalement</param>
        /// <param name="val">XPathNavigator (le xml contenant le signalement)</param>
        private void GetGeoRep(Report rem, XPathNavigator val)
        {
            XPathNodeIterator it = val.Select("GEOREP");
            foreach (XPathNavigator v in it)
            {
                GeoResponse georep = new GeoResponse();

                Group gr = new Group
                {
                    Id = v.SelectSingleNode("ID_GEOREP").Value,
                    Name = v.SelectSingleNode("TITRE").Value
                };
                georep.Group = gr;

                georep.Author = new Author
                {
                    Id = v.SelectSingleNode("ID_AUTEUR").Value,
                    Name = EncodeToUTF8(v.SelectSingleNode("AUTEUR").Value)
                };

                georep.Statut =(EnumStatus) Enum.Parse(typeof(EnumStatus),v.SelectSingleNode("STATUT").Value,true);      
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
            string xpath ="/geors/GEOREM/THEME";
            try
            {    
                XPathNodeIterator iterator = valRem.SelectDescendants(XPathNodeType.Element, false);
                if (iterator == null)
                {
                    string message = string.Format("Balise '{0}' inexistante dans la réponse xml", xpath);
                    throw new ArgumentNullException(message);
                }

                foreach (XPathNavigator val in iterator)
                {
                    Theme theme = new Theme
                    {
                        Group = new Group()
                    };

                    theme.Group.Name = val.SelectSingleNode(valRem.Compile(xpath + "/NOM")).Value;
                    theme.Group.Id = val.SelectSingleNode(valRem.Compile(xpath + "/ID_GEOGROUPE")).Value;
                    themes.Add(theme);    
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("XMLResponse.GetGeomRemThemes : {0}\n", e.Message));
                throw new Exception(e.Message);
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
                if (iterator.MoveNext())
                {
                    total = int.Parse(iterator.Current.InnerXml);
                }
                else
                {
                    throw new ArgumentNullException(string.Format("Balise '{0}' inexistante dans la réponse xml", xpath));
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("XMLResponse.GetTotalResponse : {0}\n", e.Message));
                throw new Exception(e.Message);
            }

            return total;
        }

        /// <summary>
        /// retourne la version du service EspaceCollaboratif
        /// </summary>
        /// <returns>version du service EspaceCollaboratif</returns>
        public string GetVersion()
        {
            string version = "";
            string xpath = "/geors";
            try
            {
                XPathExpression expr = navigator.Compile(xpath);
                XPathNodeIterator iterator = navigator.Select(expr);
                if(iterator.MoveNext())
                {
                    version = iterator.Current.GetAttribute("version", "");
                }
                else
                {
                    throw new ArgumentNullException(string.Format("Balise '{0}' inexistante dans la réponse xml", xpath));
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("XMLResponse.GetVersion : {0}\n", e.Message));
                throw new Exception(e.Message);
            }

            return version;
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
                if(iterator.MoveNext())
                {
                    sdate = iterator.Current.InnerXml;
                }
                else
                {
                    throw new ArgumentNullException(string.Format("Balise '{0}' inexistante dans la réponse xml", xpath));
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("XMLResponse.GetDate : {0}\n", e.Message));
                throw new Exception(e.Message);
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
