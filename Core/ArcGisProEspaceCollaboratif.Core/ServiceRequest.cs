using log4net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe pour les requêtes http vers le service EspaceCollaboratif
    /// </summary>
    class ServiceRequest
    {
        private static readonly Logger riplogger = Logger.Instance;
        static readonly ILog logger = LogManager.GetLogger(typeof(ServiceRequest));

        /// <summary>
        /// Callback pour validation du certificat SSL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="cert"></param>
        /// <param name="chain"></param>
        /// <param name="policyErrors"></param>
        /// <returns></returns>
        public static bool ValidateRemoteCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate cert, X509Chain chain, SslPolicyErrors policyErrors)
        {
            bool result = false;
            if (cert.Subject.Contains("C=FR") &&
				cert.Subject.Contains("CN=*.ign.fr"))
            {
                result = true;
            }
            return result;
        }
    }
}