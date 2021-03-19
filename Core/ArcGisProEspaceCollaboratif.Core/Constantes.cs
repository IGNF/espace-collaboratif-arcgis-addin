 using System;
using System.Globalization;

namespace ArcGisProEspaceCollaboratif.Core
{
    public class Constantes
    {

        //variable permettant de rendre indifférent à la culture (par exemple pour les nombre à virgule)
        public static CultureInfo invC = CultureInfo.InvariantCulture;

       // public const uint MAX_TENTATIVE = 2;

        //taille maximale pour un document uploadé
        public const int MAX_TAILLE_UPLOAD_FILE =  5000000;

        //nombre par défaut de remarques par page
        public const int NB_DEFAULT_SIGNALEMENTS_PAGINATION = 100;

        // Définition du protocole signant au près du service EspaceCollaboratif l'origine de ce programme. 
        public const string EspaceCollaboratif_CLIENT_PROTOCOL = "_EspaceCollaboratif_AGIS_64512";

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

        public const string DEMO = "Démonstration";
        public const string CLEGEOPORTAILSTANDARD = "choisirgeoportail";
        public const string WFS = "WFS";
        public const string GEOPORTAIL = "GeoPortail";
        public const string COLLABORATIF = "collaboratif.ign.fr";
        public const string COLLABORATIF_QLF = "qlf-collaboratif.ign.fr/collaboratif-3.0";
        public const string WXSIGN = "wxs.ign.fr";
        public const string AUCUN = "Aucun";

        public const string ERROR = "IGN Espace collaboratif - ERREUR";
        public const string WARNING = "IGN Espace collaboratif - WARNING";
        public const string STOP = "IGN Espace collaboratif - STOP";

        public const string IMAGEFILE = "Image files (*.BMP;*.JPG;*.GIF;*.JPG2000;*.TIFF;*.ECW;*.PSD)|*.bmp;*.jpg;*.jp2;*.tiff;*.ecw;*.psd";
        public const string TRACKFILE = "Track files (*.KML;*.GPX;*.SWG;*.WMF;*.AI)|*.kml;*.gpx;*.swg;*.wmf;*.ai";
        public const string TXTFILE = "Text files (*.TXT;*.PDF;*.RTF;*.DOC;*.DOCX;*.ODT)|*.txt;*.pdf;*.rtf;*.doc;*.docx;*.odt";
        public const string DBFILE = "Database files (*.MDB;*.MDBX;*.ODB;*.DBF)|*.mdb;*.mdbx;*.odb;*.dbf";
        public const string SHEETFILE = "(*.XML;*.CSV;*XLS;*.XLSX;*.ODS)|*.xml;*.csv;*.xls;*.xlsx;*.ods";
        public const string SIGFILE = "Sig files (*.SHP;*.LYR;*.GDB;*.MXD;*GCM;*.GCR;*.DXF;*.DWG;*.QGS;*.MIF;*MID)|*.shp;*.lyr;*.gdb;*.mxd;*gcm;*.gcr;*.dxf;*.dwg;*.qgs;*.mif;*.mid";
        public const string ALLFILE = "All files(*.*)|*.*";
        public const string NOFILE = "Pas de document sélectionné";
    }
}