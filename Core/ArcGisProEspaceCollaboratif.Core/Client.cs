using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using log4net;
using System.Xml.Linq;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Implémentation de l'interface IClient
    /// Cette classe sert de client pour le service de l'espace collaboratif
    /// </summary>
    public class Client
    {
        #region Parameters
        /// <summary>
        /// L'url du service de l'espace collaboratif
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Le login de l'utilisateur
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Le mot de passe de l'utilisateur
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// La version du service
        /// </summary>
        private readonly string version = "";

        /// <summary>
        /// Le profil de l'utilisateur
        /// </summary>
        public Profile Profile { get; set; }

        /// <summary>
        /// Message d'erreur lors de la connexion ou d'un appel au service ("OK" ou message d'erreur)
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// La barre de progression durant le chargement des signalements
        /// </summary>
        private System.Windows.Forms.ProgressBar progressbar = new System.Windows.Forms.ProgressBar();

        /// <summary>
        /// Le logger qui permet d'enregistrer des informations sur le processus
        /// </summary>
        private static readonly Logger riplogger = Logger.Instance;
        private static readonly ILog logger = LogManager.GetLogger(typeof(Client));

        #endregion

        #region Constructeur

        /// <summary> 
        /// Initialisation du client et connexion au service EspaceCollaboratif
        /// </summary>
        /// <param name="url">url du service EspaceCollaboratif</param>
        /// <param name="login">login de l'utilisateur</param>
        /// <param name="password">mot de passe</param>
        public Client(string url, string login, string password)
        {
            this.Url = url;
            this.Login = login;
            this.Password = password;
        }

        #endregion

        #region Class methods

        /// <summary>
        /// Requête GET
        /// </summary>
        /// <param name="uri">partie de l'url définissant la requête à effectuer</param>
        /// <param name="parameters">paramètres à envoyer en GET</param>
        /// <returns>Resultat de la requête (xml)</returns>
        public string MakeGetRequest(string url, Dictionary<string, string> parameters)
        {
            string res = null;

            WebClient client = new WebClient();
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(this.Login + ":" + this.Password));
            client.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
            /*System.Net.ServicePointManager.ServerCertificateValidationCallback =
                new System.Net.Security.RemoteCertificateValidationCallback(ServiceRequest.ValidateRemoteCertificate);*/
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                new System.Net.Security.RemoteCertificateValidationCallback(delegate { return true; });

            string paramString = "";
            if (parameters != null)
            {
                foreach (var item in parameters)
                {
                    paramString += item.Key + "=" + item.Value + "&";
                }
            }
            try
            {
                res = paramString == "" ? client.DownloadString(url) : client.DownloadString(url + "?" + paramString);
            }
            catch (Exception e)
            {
                string err = "";
                if (e.Message.Contains("(401) Unauthorized"))
                {
                    err = "(401) Unauthorized";
                }
                else
                {
                    err = e.Message;
                }
                client.Dispose();
                logger.Error(string.Format("Client.MakeGetRequest : {0}\n", err));
                throw new Exception(err);
            }
            client.Dispose();
            return res;
        }

        /// <summary>
        ///  Requête POST 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public string MakePostRequest(string uri, Dictionary<string, string> parameters)
        {
            string res = null;

            WebClient wclient = new WebClient();
          
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(this.Login + ":" + this.Password));
            wclient.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;

            string paramString = "";
            if (parameters != null)
            {
                foreach (var item in parameters)
                {
                    paramString += item.Key + "=" + item.Value + "&";
                }
            }
            paramString = paramString.Substring(0, paramString.Length - 1);

            wclient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded; charset=utf-8";

            try
            {
                res = wclient.UploadString(uri, paramString);
            }
            catch (Exception e)
            {
                wclient.Dispose();
                logger.Error(string.Format("Client.MakePostRequest : {0}\n", e.Message));
            }

            wclient.Dispose();
            return res;
        }

        /// <summary>
        ///  Requête POST avec paramètres et fichier (multipart/form-data)
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="parameters">parameters to send</param>
        /// <param name="docs">files to upload</param>
        /// <returns></returns>
        public string MakeMultiPartPostRequest(string url, Dictionary<string, string> parameters, Dictionary<string, string> docs)
        {
            string result = "";
            string boundary = "-----------------------------19746328113";
            byte[] boundarybytes = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;

            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(this.Login + ":" + this.Password));
            wr.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;

            string contentType = "multipart/form-data";

            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in parameters.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, parameters[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            if (docs != null)
            {
                foreach (KeyValuePair<string, string> kv in docs)
                {
                    string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                    string filename = System.IO.Path.GetFileName(kv.Value);

                    string header = string.Format(headerTemplate, kv.Key, filename, contentType);
                    byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                    rs.Write(headerbytes, 0, headerbytes.Length);

                    FileStream fileStream = new FileStream(kv.Value, FileMode.Open, FileAccess.Read);

                    byte[] buffer = new byte[4096];
                    int bytesRead = 0;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        rs.Write(buffer, 0, bytesRead);
                    }
                    fileStream.Close();
                    rs.Write(boundarybytes, 0, boundarybytes.Length);
                }
            }

            byte[] trailer = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                result = reader2.ReadToEnd();
                Console.WriteLine(string.Format("File uploaded, server response is: {0}", reader2.ReadToEnd()));
            }
            catch (Exception ex)
            {
                string message = string.Format("Création d'un nouveau signalement : {0}", ex.Message);
                logger.Error(string.Format("Client.MakeMultiPartPostRequest : {0}\n", message));

                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
                message += string.Format("\nLe signalement n'a pas pu être ajouté {0}", ex.Message);
                throw new Exception(message);
            }
            finally
            {
                wr = null;
            }
            return result;
        }

        /// <summary>
        /// retourne la version du service EspaceCollaboratif
        /// </summary>
        /// <returns>version du service</returns>
        public string GetVersion()
        {
            return this.version;
        }

        /// <summary>
        /// retourne le profil de l'utilisateur
        /// </summary>
        /// <returns>le profil </returns>
        public Profile GetProfile()
        {
            if (this.Profile == null)
            {
                this.Profile = this.GetProfileFromService();
            }
            return this.Profile;
        }

        /// <summary>
        /// Requête au service pour le profil de l'utilisateur
        /// </summary>
        /// <returns>le profil de l'utilisateur</returns>
        private Profile GetProfileFromService()
        {
            Profile profile = new Profile();
            string data = "";

            data = this.MakeGetRequest(string.Format("{0}/api/georem/geoaut_get.xml", this.Url), null);

            XmlResponse xmlResponse = new XmlResponse(data);
            Dictionary<string, string> errMessage = xmlResponse.CheckResponseValidity();

            if (errMessage["code"].Equals("OK"))
            {
                profile = xmlResponse.ExtractProfile();
            }
            else {
                logger.Error(string.Format("Client.GetProfileFromService : {0}\n", errMessage["code"]));
                throw new Exception(errMessage["code"]);
            }
            return profile;
        }

        /// <summary>
        /// Retourne les signalements
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<Report> GetGeoRems(Dictionary<string, string> parameters)
        {
            List<Report> signalements = new List<Report>();

            //on stocke d'abord les objets Signalement dans un dictionnaire, pour éviter d'éventuels doublons.
            SortedDictionary<UInt64, Report> dicoRems = new SortedDictionary<ulong, Report>();

            Dictionary<string, string> totalAndDate = GetAllReports(parameters, dicoRems);
            int total = Int32.Parse(totalAndDate["total"]);
            string sdate = totalAndDate["sdate"];

            //Mise-à-jour éventuelle à partir du dateTime du début de la requête
            while (total > 1)
            {
                parameters["updatingDate"] = sdate;

                totalAndDate = GetAllReports(parameters, dicoRems);
                total = Int32.Parse(totalAndDate["total"]);

                sdate = totalAndDate["sdate"];
            }

            signalements = new List<Report>(dicoRems.Values);

            return signalements;
        }

        /// <summary>
        /// Recherche les signalements et retourne le nombre total de signalements trouvés
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="dicoSignalements"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetAllReports(Dictionary<string, string> parameters, SortedDictionary<UInt64, Report> dicoSignalements)
        {
            int pagination = 100;
            int total = 0;
            int count = 0;
            string sdate = "";

            string uri = string.Format("{0}/api/georem/georems_get.xml", this.Url);
            var data = this.MakeGetRequest(uri, parameters);

            XmlResponse xmlResponse = new XmlResponse(data);
            Dictionary<string, string> errMessage = xmlResponse.CheckResponseValidity();

            Dictionary<string, string> totalAndDate = new Dictionary<string, string>();

            if (errMessage["code"].Equals("OK"))
            {
                total = xmlResponse.GetTotalResponse();
                sdate = xmlResponse.GetDate();

                dicoSignalements = xmlResponse.ExtractReports(dicoSignalements);

                pagination = dicoSignalements.Count;
                count = pagination;

                this.progressbar.Maximum = pagination > total ? pagination : total;
                this.progressbar.Step = pagination;

                while (total - count > 0)
                {
                    parameters["offset"] = count.ToString();

                    data = this.MakeGetRequest(string.Format("{0}/api/georem/georems_get.xml",this.Url), parameters);
                    xmlResponse = new XmlResponse(data);

                    if (errMessage["code"].Equals("OK"))
                    {
                        dicoSignalements = xmlResponse.ExtractReports(dicoSignalements);
                    }

                    this.progressbar.Increment(pagination);

                    count += pagination;
                }
            }

            totalAndDate.Add("total", total.ToString());
            totalAndDate.Add("sdate", sdate);

            return totalAndDate;
        }

        /// <summary>
        /// Définition de la processBar
        /// </summary>
        /// <param name="progressbar"></param>
        /*public void SetProgressBar(System.Windows.Forms.ProgressBar progressbar)
        {
            this.progressbar = progressbar;
            this.progressbar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
        }*/


        /// <summary>
        /// Retourne un simple signalement, donné par son identifiant
        /// </summary>
        /// <param name="idReport">identifiant du signalement</param>
        /// <returns>le signalement</returns>
        public Report GetGeoRem(string idReport)
        {
            Report report = new Report();
            List<Report> reports = new List<Report>();

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            var data = this.MakeGetRequest(string.Format("{0}/api/georem/georem_get/{1}.xml",this.Url, idReport), null);

            XmlResponse xmlResponse = new XmlResponse(data);
            Dictionary<string, string> errMessage = xmlResponse.CheckResponseValidity();

            int total = xmlResponse.GetTotalResponse();

            if (errMessage["code"].Equals("OK"))
            {
                if (total == 1)
                {
                    reports = xmlResponse.ExtractReports(reports);
                    report = reports[0];
                }
            }

            return report;
        }

        /// <summary>
        /// Ajoute une réponse à un signalement
        /// </summary>
        /// <param name="report">Le signalement</param>
        /// <param name="response">la réponse</param>
        /// <returns>le signalement à laquelle a été ajoutée la réponse</returns>
        public Report AddReponse(Report report, string response)
        {
            Report reportUpdating = null;
            try
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    { "id", report.Id.ToString() },
                    { "content",response },
                    { "status", report.Status.ToString().ToLower() },
                    { "title", "" }
                };

                string data = this.MakeMultiPartPostRequest(this.Url + "/api/georem/georep_post.xml", parameters, null);

                XmlResponse xmlResponse = new XmlResponse(data);
                Dictionary<string, string> errMessage = xmlResponse.CheckResponseValidity();
                if (errMessage["code"].Equals("OK"))
                {
                    List<Report> reports = new List<Report>();
                    reports = xmlResponse.ExtractReports(reports);
                    if (reports.Count == 1)
                    {
                        reportUpdating = reports[0];
                    }
                    else
                    {
                        string message = "Problème lors de l'ajout d'une réponse";
                        logger.Error(string.Format("Client.AddReponse : {0}\n", message));
                        throw new Exception(message);
                    }
                }
                else
                {
                    logger.Error(string.Format("Client.AddReponse : {0}\n", errMessage["message"]));
                    throw new Exception(errMessage["message"]);
                }
            }
            catch (Exception e) {
                logger.Error(string.Format("Client.AddReponse : {0}\n", e.Message));
                throw new Exception(e.Message);
            }

            return reportUpdating;
        }

        /// <summary>
        /// Création d'un nouveau signalement
        /// </summary>
        /// <param name="tmpReport">Le signalement temporaire à compléter</param>
        /// <returns>L'objet signalement créé</returns>
        public Report CreateReport(Report tmpReport)
        {
            Report newReport = null;
            try
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    { "version", Constantes.EspaceCollaboratif_CLIENT_VERSION },
                    { "protocol", Constantes.EspaceCollaboratif_CLIENT_PROTOCOL },
                    { "comment", tmpReport.Commentary }
                };

                string geometry = "POINT(" + Convert.ToString(tmpReport.GetLongitude(), Constantes.invC) + " " +
                                 Convert.ToString(tmpReport.GetLatitude(), Constantes.invC) + ")";
                parameters.Add("geometry", geometry);

                // zone géographique 
                parameters.Add("territory", this.GetProfile().Zone.ToString());

                // Ajout des thèmes selectionnés 
                List<Theme> themes = tmpReport.Themes;
                if (themes != null && themes.Count > 0)
                {
                    XDocument doc = new XDocument(new XElement("THEMES"));
                    string attributes = "";
                    foreach (Theme t in themes)
                    {
                        string groupIdAndName = "\"" + t.Group.Id + "::" + t.Group.Name;
                        attributes += groupIdAndName + "\"=>\"1\",";
                        foreach(ThemeAttributes att in t.Attributes)
                        {
                            attributes += groupIdAndName + "::" + att.TagName + "\"=>\"" + att.UserSelectedValue + "\",";
                        }
                    }
                    attributes = attributes.Substring(0, attributes.Length - 1);
                    parameters.Add("attributes", attributes);
                }

                //ajout des croquis
                if (!tmpReport.IsCroquisEmpty())
                {
                    List<Sketch> sketches = tmpReport.Sketches;
                    XNamespace gml = "http://www.opengis.net/gml";

                    XDocument doc = new XDocument(new XElement("CROQUIS",
                        new XAttribute(XNamespace.Xmlns + "gml", "http://www.opengis.net/gml")));

                    foreach (Sketch cr in sketches)
                    {
                        doc = cr.EncodeToXML(doc, gml);
                    }

                    parameters.Add("sketch", doc.ToString());
                }

                //ajout des documents joints      
                List<string> documents = tmpReport.Documents;

                int docCount = 0;

                Dictionary<string, string> docs = new Dictionary<string, string>();
                foreach (string document in documents)
                {
                    if (File.Exists(document))
                    {
                        docCount++;
                        FileStream fs = File.Open(document, FileMode.Open);
                        if (fs.Length > Constantes.MAX_TAILLE_UPLOAD_FILE)
                        {
                            string message = string.Format("Le fichier {0} est de taille supérieure à {1}", document, Constantes.MAX_TAILLE_UPLOAD_FILE);
                            throw new Exception(message);
                        }

                        fs.Close();
                        docs.Add("upload" + docCount, document);
                    }
                }

                //envoi de la requête
                string data = this.MakeMultiPartPostRequest(Url + "/api/georem/georem_post.xml", parameters, docs);

                XmlResponse xmlResponse = new XmlResponse(data);
                Dictionary<string, string> errMessage = xmlResponse.CheckResponseValidity();
                if (errMessage["code"].Equals("OK"))
                {
                    List<Report> reports = new List<Report>();
                    reports = xmlResponse.ExtractReports(reports);
                    if (reports.Count == 1)
                    {
                        newReport = reports[0];
                    }
                    else
                    {
                        string message = "Problème lors de l'ajout du signalement";
                        throw new Exception(message);
                    }
                }
                else
                {
                    throw new Exception(errMessage["message"]);
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Client.CreateReport : {0}\n", e.Message));
                throw new Exception(e.Message);
            }

            return newReport;
        }

        /// <summary>
        /// retourne le nombre de signalements par page (valeur par défaut)
        /// </summary>
        /// <returns></returns>
        public int Get_NB_DEFAULT_SIGNALEMENTS_PAGINATION()
        {
            return Constantes.NB_DEFAULT_SIGNALEMENTS_PAGINATION;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idProfil"></param>
        /// <returns></returns>
        public (Profile, string) SetChangeUserProfil(string idProfil)
        {
            Profile profil = new Profile();
            string message = "";
            string url = string.Format("{0}/api/georem/geoaut_switch_profile/{1}", this.Url, idProfil);
            string data = this.MakeGetRequest(url, null);
            XmlResponse xmlResponse = new XmlResponse(data);
            Dictionary<string, string> errMessage = xmlResponse.CheckResponseValidity();

            if (errMessage["code"].Equals("OK"))
            {
                profil = xmlResponse.ExtractProfile();
            }
            else if (errMessage["message"] != "")
            {
                message = errMessage["message"];
            }
            return (profil, message);
        }

        /// <summary>
        /// Récupération des layers GéoPortail valides en fonction de la clé Geoportail utilisateur
        /// </summary>
        /// <param name="cle"></param>
        /// <returns></returns>
        public List<LayerGeoportail> GetLayersFromCleGeoportailUser(string cle)
        {
            List<LayerGeoportail> layers = new List<LayerGeoportail>();
            string cleGeoportail = "";
            if (string.IsNullOrEmpty(cle))
            {
                cleGeoportail = "choisirgeoportail";
            }
            if (cle == "Démonstration")
            {
                cleGeoportail = "choisirgeoportail";
            }
            else if (cle != "")
            {
                cleGeoportail = cle;
            }

            string url = string.Format("https://wxs.ign.fr/{0}/autoconf?gp-access-lib=2.1.2&output=xml", cleGeoportail);
            logger.Debug(string.Format("{0} {1}", "GetLayersFromCleGeoportailUser", url));
            string data = this.MakeGetRequest(url, null);
            XmlResponse xmlResponse = new XmlResponse(data);
            Dictionary<string, string> errMessage = xmlResponse.CheckResponseValidity();
            if (errMessage["message"].Contains("ViewContext id=\"autoConf\""))
            {
                layers = xmlResponse.ExtractLayersFromCleGeoportailUser();
            }
            else
            {
                string message = string.Format("{0} : votre clé Géoportail semble erronée. Vous pouvez utiliser la clé de démonstration.", errMessage["code"]);
                logger.Error(string.Format("Client.GetLayersFromCleGeoportailUser : {0}\n", message));
                throw new Exception(message);
            }
            return layers;
        }

        #endregion
    }
}
