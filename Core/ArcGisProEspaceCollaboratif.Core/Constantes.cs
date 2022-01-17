using System.Collections.Generic;
using System.Globalization;

namespace ArcGisProEspaceCollaboratif.Core
{
    public class Constantes
    {
        //variable permettant de rendre indifférent à la culture (par exemple pour les nombre à virgule)
        public static CultureInfo invC = CultureInfo.InvariantCulture;

        //taille maximale pour un document uploadé
        public const long MAX_TAILLE_UPLOAD_FILE = 16000000;

        //nombre par défaut de remarques par page
        public const int NB_DEFAULT_SIGNALEMENTS_PAGINATION = 100;

        // Définition du protocole signant au près du service EspaceCollaboratif l'origine de ce programme.
        public const string EspaceCollaboratif_CLIENT_PROTOCOL = "_RIPART_AGIS_64512";

        // Définition donnant la version de ce programme.
        public const string EspaceCollaboratif_CLIENT_VERSION = "1_1_0";

        public const string EspaceCollaboratif_GEOREM_GET = "georem_get";
        public const string EspaceCollaboratif_GEOREM_POST = "georem_post";
        public const string EspaceCollaboratif_GEOREM_PUT = "georem_put";
        public const string EspaceCollaboratif_CONNECT = "connect";
        public const string EspaceCollaboratif_GEOAUT_GET = "geoaut_get";
        public const string EspaceCollaboratif_QUESTION_POST = "geoquestion_post";
        public const string EspaceCollaboratif_QUESTION_GET = "geoquestion_get";

        public const string HELP_FILE_URL = "http://logiciels.ign.fr/IMG/pdf/add-in-EspaceCollaboratif_1-0.pdf";

        public const string WFS = "WFS";
        public const string WMTS = "WMTS";
        public const string COLLABORATIF = "collaboratif.ign.fr";
        public const string COLLABORATIF_QLF = "qlf-collaboratif.ign.fr/collaboratif-3.0";
        public const string WXSIGN = "wxs.ign.fr";
        public const string AUCUN = "Aucun";
        public const string NAMELOGFILE = "_EspaceCollaboratif.log";

        public const string ERROR = "IGN Espace collaboratif - ERREUR";
        public const string WARNING = "IGN Espace collaboratif - WARNING";
        public const string STOP = "IGN Espace collaboratif - STOP";
        public const string QUESTION = "IGN Espace collaboratif - QUESTION";
        public const string INFORMATION = "IGN Espace collaboratif - INFORMATION";

        public const string IMAGEFILE = "Image files (*.BMP;*.GIF;*.JPG;*.JPEG;*.PNG)|*.bmp;*.gif;*.jpg;*.jpeg;*.png";
        public const string TRACKFILE = "Track files (*.GPX)|*.gpx";
        public const string TXTFILE = "Text files (*.DOC;*.DOCX;*.ODT;*.PDF;*.TXT)|*.doc;*.docx;*.odt;*.pdf;*.txt";
        public const string SHEETFILE = "Sheet files (*.CSV;*.KML;*.ODS;*XLS;*.XLSX)|*.csv;*.kml;*.ods;*.xls;*.xlsx";
        public const string ZIPFILE = "Zip files (*.ZIP;*.7Z)|*.zip;*.7z";
        public const string ALLFILE = "All files(*.*)|*.*";
        public const string NOFILE = "Pas de document sélectionné";

        static public readonly List<string> listAuthorisation = new List<string>()
        {
            "RW",
            "RW+",
            "RW-"
        };

        public const string N_REPORT_IN_GDB = "NoSignalement";
        public const string LIEN_REPORT = "Lien_signalement";
        public const string DEFAULT_DATE_EXTRACTION = "2000-01-01 00:00:00";
    }
}