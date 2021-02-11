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
        private readonly String response;

        private XPathDocument docxpath;

        private XPathNavigator nav;

        private readonly CultureInfo invC = CultureInfo.InvariantCulture;


        private readonly Logger riplogger = Logger.Instance;
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
        public Dictionary<String, String> CheckResponseValidity() {

            Dictionary<String, String> errMessage = new Dictionary<string, string>();

            try
            {
                XPathExpression expr = nav.Compile("/geors/REPONSE/ERREUR");
                XPathNodeIterator iterator = nav.Select(expr);
                iterator.MoveNext();
                errMessage["message"] = iterator.Current.InnerXml;
                errMessage["code"] = iterator.Current.GetAttribute("code", "");

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
        public String GetCurrentJeton() {
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
            try
            {
                string profilXpath = "/geors/AUTEUR/";
                XPathExpression expr = nav.Compile(profilXpath + "NOM");
                XPathNodeIterator iterator = nav.Select(expr);
                profil.Auteur = new Auteur();
                if (iterator.MoveNext())
                {
                    profil.Auteur.Nom = EncodeToUTF8(iterator.Current.InnerXml);
                }

                profilXpath = "/geors/PROFIL/";
                expr = nav.Compile(profilXpath + "ID_GEOPROFIL");
                iterator = nav.Select(expr);
                if (iterator.MoveNext())
                {
                    profil.Id_Geoprofil = iterator.Current.InnerXml;
                }

                expr = nav.Compile(profilXpath + "TITRE");
                iterator = nav.Select(expr);
                if (iterator.MoveNext())
                {
                    profil.Titre = EncodeToUTF8(iterator.Current.InnerXml);
                }

                Groupe gr = new Groupe();
                expr = nav.Compile(profilXpath + "ID_GEOGROUPE");
                iterator = nav.Select(expr);
                if (iterator.MoveNext())
                {
                    gr.Id = iterator.Current.InnerXml;

                }
                expr = nav.Compile(profilXpath + "GROUPE");
                iterator = nav.Select(expr);
                if (iterator.MoveNext())
                {
                    gr.Nom = EncodeToUTF8(iterator.Current.InnerXml);
                }
                profil.groupe = gr;

                expr = nav.Compile(profilXpath + "LOGO");
                iterator = nav.Select(expr);
                if (iterator.MoveNext())
                {
                    profil.Logo = iterator.Current.InnerXml;
                }

                expr = nav.Compile(profilXpath + "FILTRE");
                iterator = nav.Select(expr);
                if (iterator.MoveNext())
                {
                    profil.Filter = iterator.Current.InnerXml;
                }

                expr = nav.Compile(profilXpath + "PRIVE");
                iterator = nav.Select(expr);
                if (iterator.MoveNext())
                {
                    profil.Prive = iterator.Current.InnerXml.Equals("1");
                }

                // Les thèmes associés au profil
                List<Theme> themes = new List<Theme>();
                List<string> filteredThemes = new List<string>();
                GetThemes(ref themes, ref filteredThemes);
                profil.Themes = themes;
                profil.filteredThemes = filteredThemes;

                // Les infos de tous les geogroupes de l'utilisateur
                profil.geogroupes = GetGeoGroupes();

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + "\n" + ex.StackTrace);
                throw ex;
            }
            return profil;
        }

        /*
             def getInfosGeogroupe(self):
        """Extraction des infos utilisateur sur ses geogroupes
        :return les infos
        """
        infosgeogroupes = []

        try:
            # informations sur le geogroupe
            nodesGr = self.root.findall('GEOGROUPE')
            for nodegr in nodesGr:
                infosgeogroupe = InfosGeogroupe()
                infosgeogroupe.groupe = Groupe()
                infosgeogroupe.groupe.nom = (nodegr.find('NOM')).text
                infosgeogroupe.groupe.id = (nodegr.find('ID_GEOGROUPE')).text

                # Récupération du commentaire par défaut des signalements
                infosgeogroupe.georemComment = nodegr.find('COMMENTAIRE_GEOREM').text

                # Récupération des layers du groupe
                for nodelayer in nodegr.findall('LAYERS/LAYER'):
                    layer = Layer()
                    layer.type = nodelayer.find('TYPE').text
                    layer.nom = nodelayer.find('NOM').text
                    layer.description = nodelayer.find('DESCRIPTION').text
                    layer.minzoom = nodelayer.find('MINZOOM').text
                    layer.maxzoom = nodelayer.find('MAXZOOM').text
                    layer.extent = nodelayer.find('EXTENT').text
                    # cas particulier de la balise <ROLE> qui n'existe
                    # que dans la base de qualification
                    role = nodelayer.find('ROLE')
                    if role is not None:
                        layer.role = role.text
                    layer.visibility = nodelayer.find('VISIBILITY').text
                    layer.opacity = nodelayer.find('OPACITY').text
                    tilezoom = nodelayer.find('TILEZOOM')
                    if tilezoom is not None:
                        layer.tilezoom = tilezoom.text
                    url = nodelayer.find('URL')
                    if url is not None:
                        layer.url = url.text

                    infosgeogroupe.layers.append(layer)

                # Récupération des thèmes du groupe
                themesAttDict = {}

                try:

                    thAttributs = []
                    thAttNodes = nodegr.findall('THEMES/ATTRIBUT')
                    for attNode in thAttNodes:

                        nomTh = ClientHelper.notNoneValue(attNode.find('NOM').text)
                        nomAtt = attNode.find('ATT').text
                        thAttribut = ThemeAttribut(nomTh, nomAtt, None)

                        attType = attNode.find('TYPE').text
                        thAttribut.setType(attType)

                        attObligatoire = attNode.find('OBLIGATOIRE')
                        if attObligatoire is not None:
                            thAttribut.setObligatoire()

                        for val in attNode.findall('VALEURS/VAL'):
                            thAttribut.addValeur(val.text)

                        for val in attNode.findall('VALEURS/DEFAULTVAL'):
                            thAttribut.defaultval = val.text

                        thAttributs.append(thAttribut)
                        if nomTh not in themesAttDict:
                            themesAttDict[nomTh] = []
                        themesAttDict[nomTh].append(thAttribut)

                    nodes = nodegr.findall('THEMES/THEME')

                    # Récupérer les thèmes à afficher dans le profil (balise <FILTER>)
                    # Exemple : [{"group_id":375,"themes":["Test_signalement","test leve",
                    # "Theme_table_bool_TestEcriture"]},{"group_id":1,"themes":["Bati"]}]

                    filterDict = nodegr.find('FILTER').text
                    groupFilters = re.findall('\{.*?\}',filterDict)
                    filteredThemes = self.getFilteredThemes(groupFilters, infosgeogroupe.groupe.id)
                    infosgeogroupe.filteredThemes = filteredThemes

                    for node in nodes:
                        theme = Theme()
                        theme.groupe = Groupe()

                        nom = (node.find('NOM')).text
                        theme.groupe.nom = nom
                        if nom in filteredThemes:
                             theme.isFiltered = True

                        theme.groupe.id = infosgeogroupe.groupe.id
                        if ClientHelper.notNoneValue(theme.groupe.nom) in themesAttDict:
                            theme.attributs.extend(themesAttDict[ClientHelper.notNoneValue(theme.groupe.nom)])

                        infosgeogroupe.themes.append(theme)

                except Exception as e:
                    self.logger.error(str(e))
                    raise Exception("Erreur dans la récupération des thèmes du groupe")

                infosgeogroupes.append(infosgeogroupe)

        except Exception as e:
            self.logger.error(str(e))
            raise Exception("Erreur dans la récupération des informations sur le GEOGROUPE")

        return infosgeogroupes
        */

        ///
        ///
        public List<GeoGroupe> GetGeoGroupes()
        {
            List<GeoGroupe> listGeoGroupe = new List<GeoGroupe>();

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
        public List<string> GetFilteredThemes(MatchCollection groupFilters, string idGeogroupe)
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

        /// <summary>
        /// Extraction des thèmes associés au profil
        /// </summary>
        /// <returns></returns>
        public void GetThemes(ref List<Theme> themes, ref List<string> filteredThemes)
        {
            try
            {
                ConcurrentDictionary<string, List<ThemeAttribut>> themesAttributesDict = new ConcurrentDictionary<string, List<ThemeAttribut>>();
                nav.MoveToRoot();
                XPathExpression expr = nav.Compile("/geors/THEMES/ATTRIBUT");
                XPathNodeIterator iterator = nav.Select(expr);
                foreach (XPathNavigator val in iterator)
                {
                    ThemeAttribut themeAttribut = new ThemeAttribut
                    {
                        Theme = EncodeToUTF8(val.SelectSingleNode("NOM").Value),
                        Nom = EncodeToUTF8(val.SelectSingleNode("ATT").Value),
                        Type = EncodeToUTF8(val.SelectSingleNode("TYPE").Value),
                        Valeurs = new List<string>()
                    };

                    XPathNavigator obligatoire = val.SelectSingleNode("OBLIGATOIRE");
                    if (obligatoire != null)
                    {
                        themeAttribut.Obligatoire = obligatoire.Value;
                    }

                    XPathNavigator valVALEURS = val.SelectSingleNode("VALEURS");
                    XPathNodeIterator valIt = valVALEURS.SelectChildren(XPathNodeType.Element);
                    List<string> lTmp = new List<string>();
                    foreach (XPathNavigator valeurs in valIt)
                    {
                        if (valeurs.Name == "DEFAULTVAL")
                        {
                            themeAttribut.DefaultVal = EncodeToUTF8(valeurs.InnerXml);
                        }
                        if (valeurs.Name == "VAL")
                        {
                            lTmp.Add(EncodeToUTF8(valeurs.InnerXml));
                        }
                    }
                    themeAttribut.Valeurs = lTmp;
                    themesAttributesDict.AddOrUpdate(themeAttribut.Theme, new List<ThemeAttribut> { themeAttribut }, (nomTheme, attTheme) => { attTheme.Add(themeAttribut); return attTheme; });
                }

                // Récupération du filtre sur les thèmes
                nav.MoveToRoot();
                XPathExpression filtreExpr = nav.Compile("/geors/PROFIL/FILTRE");
                XPathNodeIterator filtreIterator = nav.Select(filtreExpr);
                if (filtreIterator.MoveNext())
                {
                    string sfiltre = EncodeToUTF8(filtreIterator.Current.InnerXml);
                    MatchCollection collection = Regex.Matches(sfiltre, "\\{.*?\\}");
                    filteredThemes = GetFilteredThemes(collection, "");
                }

                // Récupération des thèmes avec leurs attributs et le filtre
                nav.MoveToRoot();
                XPathExpression themeExpr = nav.Compile("/geors/THEMES/THEME");
                XPathNodeIterator themeIterator = nav.Select(themeExpr);
                foreach (XPathNavigator val in themeIterator)
                {
                    Theme tmpTheme = new Theme();
                    string nom = EncodeToUTF8(val.SelectSingleNode("NOM").Value);
                    tmpTheme.groupe.Nom = nom;
                    if (filteredThemes.Contains(nom))
                    {
                        tmpTheme.Filtered = true;
                    }
                    if (themesAttributesDict.ContainsKey(nom))
                    {
                        List<ThemeAttribut> tmp = new List<ThemeAttribut>();
                        themesAttributesDict.TryGetValue(nom, out tmp);
                        tmpTheme.Attributs = tmp;
                    }
                    tmpTheme.groupe.Id = val.SelectSingleNode("ID_GEOGROUPE").Value;

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
        public List<Signalement> ExtractSignalements(List<Signalement> signalements)
        {
            SortedDictionary<UInt64, Signalement> dicSignalement = new SortedDictionary<ulong, Signalement>();
            dicSignalement = this.ExtractSignalements(dicSignalement);
            signalements = new List<Signalement>(dicSignalement.Values);
            signalements.Reverse();

            return signalements;
        }

        /// <summary>
        /// Extraction des remarques de la réponse xml
        /// </summary>
        /// <param name="remarques">SortedDictionary key:indentifiant de la remarque, value: la remarque</param>
        /// <returns>le dictionnaire de remarques</returns>
        public SortedDictionary<UInt64, Signalement> ExtractSignalements(SortedDictionary<UInt64, Signalement> signalements)
        {

            Signalement rem = null;
            String remXpath = "/geors/GEOREM";

            try
            {
                XPathExpression expr = nav.Compile(remXpath);
                XPathNodeIterator iterator = nav.Select(expr);

                foreach (XPathNavigator val in iterator)
                {
                    List<Theme> themes = new List<Theme>();
                    rem = new Signalement();
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
                            groupe = new Groupe(idGroupe, nomGroupe)
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
                throw new Exception("Une erreur est survenue dans l'importation des remarques");
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
        private XPathNodeIterator GetCroquisForRem(Signalement rem, XPathNavigator val )
        {
            val.MoveToParent();
            XPathNodeIterator it = val.Select("CROQUIS/objet");

            List<Point> points = new List<Point>();
            foreach (XPathNavigator v in it)
            {
                Sketch croquis = new Sketch();
                Sketch.SketchType type =
                    (Sketch.SketchType)Enum.Parse(typeof(Sketch.SketchType), v.GetAttribute("type", ""), true);

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
        private void GetDoc(Signalement rem, XPathNavigator val)
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
        private void GetGeoRep(Signalement rem, XPathNavigator val)
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
        /// Extraction des thèmes liés à un signalement
        /// </summary>
        /// <param name="valRem">XPathNavigator (le xml contenant le signalement)</param>
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
                        groupe = new Groupe()
                    };

                    theme.groupe.Nom = val.SelectSingleNode(valRem.Compile(remXpath + "/NOM")).Value;
                    theme.groupe.Id = val.SelectSingleNode(valRem.Compile(remXpath + "/ID_GEOGROUPE")).Value;
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