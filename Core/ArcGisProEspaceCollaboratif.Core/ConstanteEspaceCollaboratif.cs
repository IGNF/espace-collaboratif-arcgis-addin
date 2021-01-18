 using System;
using System.Globalization;

namespace EspaceCollaboratif.Core
{
    public class  ConstanteEspaceCollaboratif
    {
        //variable permettant de rendre indifférent à la culture (par exemple pour les nombre à virgule)
        public static CultureInfo invC = CultureInfo.InvariantCulture;

       // public const uint MAX_TENTATIVE = 2;

        //taille maximale pour un document uploadé
        public const int MAX_TAILLE_UPLOAD_FILE =  5000000;

        //nombre par défaut de remarques par page
        public const int NB_DEFAULT_REMARQUES_PAGINATION = 100;

        // Définition du protocole signant au près du service EspaceCollaboratif l'origine de ce programme. 
        public const String EspaceCollaboratif_CLIENT_PROTOCOL = "_EspaceCollaboratif_AGIS_64512";

        // Définition donnant la version de ce programme.
        public  const String EspaceCollaboratif_CLIENT_VERSION = "1_1_0";

        public const String EspaceCollaboratif_GEOREM_GET = "georem_get";
        public const String EspaceCollaboratif_GEOREM_POST = "georem_post";
        public const String EspaceCollaboratif_GEOREM_PUT = "georem_put";
        public const String EspaceCollaboratif_CONNECT = "connect";
        public const String EspaceCollaboratif_GEOAUT_GET = "geoaut_get";
        public const String EspaceCollaboratif_QUESTION_POST = "geoquestion_post";
        public const String EspaceCollaboratif_QUESTION_GET = "geoquestion_get";

        public const String HELP_FILE_URL = "http://logiciels.ign.fr/IMG/pdf/add-in-EspaceCollaboratif_1-0.pdf";
    }
}