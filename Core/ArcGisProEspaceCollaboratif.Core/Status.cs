using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ArcGisProEspaceCollaboratif.Core
{
    /*public class Wording
    {
        /// <summary>
        /// Libellé serveur du nouveau statut
        /// </summary>
        public Status.EnumStatus Exact { get; set; }

        /// <summary>
        /// Libellé rédigé pour l'utilisateur
        /// </summary>
        public string Full { get; set; }
    }*/

    public class Status
    {
        /// <summary>
        /// Statut d'un signalement
        /// </summary>
        public enum EnumStatus
        {
            submit,
            pending,
            pending0,
            pending1,
            pending2,
            valid,
            valid0,
            reject,
            reject0,
            undefined
        }

        static public readonly EnumStatus[] OpenStatut = new EnumStatus[]{
            EnumStatus.submit,
            EnumStatus.pending,
            EnumStatus.pending0,
            EnumStatus.pending1,
            EnumStatus.pending2
        };

        static public readonly ObservableCollection<string> ListWordings = new ObservableCollection<string>()
        {
            "En cours de traitement",
            "En attente de saisie",
            "Pris en compte",
            "Déjà pris en compte",
            "Rejeté (hors spéc.)",
            "Rejeté (hors de propos)"
        };

        static public readonly Dictionary<string, EnumStatus> CorrespondenceStatusWording = new Dictionary<string, EnumStatus>()
        {
            { "En cours de traitement", EnumStatus.pending},
            { "En attente de saisie", EnumStatus.pending1},
            { "Pris en compte", EnumStatus.valid},
            { "Déjà pris en compte", EnumStatus.valid0},
            { "Rejeté (hors spéc.)", EnumStatus.reject},
            { "Rejeté (hors de propos)", EnumStatus.reject0}
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statusToSearch"></param>
        /// <returns></returns>
        static public string GetDisplayStatus(EnumStatus statusToSearch)
        {
            Dictionary<EnumStatus, string> correspondance = new Dictionary<EnumStatus, string>()
            {
                { EnumStatus.submit, "Reçu dans nos services" },
                { EnumStatus.pending, "En cours de traitement"},
                { EnumStatus.pending0, "Demande de qualification" },
                { EnumStatus.pending1, "En attente de saisie"},
                { EnumStatus.valid, "Pris en compte" },
                { EnumStatus.valid0, "Déjà pris en compte"},
                { EnumStatus.reject, "Rejeté (hors spéc.)" },
                { EnumStatus.reject0, "Rejeté (hors de propos)" },
                { EnumStatus.pending2, "En attente de validation"}
            };

            if (correspondance.ContainsKey(statusToSearch))
            {
                return correspondance[statusToSearch];
            }
            else
            {
                // Couleur par défaut
                return "Indéfini";
            }

        }

        static public List<double> GetStatusColor(EnumStatus statusToSearch)
        {
            List<double> submitColor = new List<double> { 19, 115, 235 };
            List<double> pendingColor = new List<double> { 235, 136, 0 };
            List<double> validColor = new List<double> { 56, 168, 0 };
            List<double> rejectColor = new List<double> { 255, 0, 0 };
            List<double> defaultColor = new List<double> { 211, 211, 311 };

            Dictionary<EnumStatus, List<double>> colorDictionary = new Dictionary<EnumStatus, List<double>>()
            {
                { EnumStatus.submit, submitColor },
                { EnumStatus.pending, pendingColor},
                { EnumStatus.pending0, pendingColor },
                { EnumStatus.pending1, pendingColor},
                { EnumStatus.valid, validColor },
                { EnumStatus.valid0, validColor},
                { EnumStatus.reject, rejectColor },
                { EnumStatus.reject0, rejectColor },
                { EnumStatus.pending2, pendingColor}
            };

            if (colorDictionary.ContainsKey(statusToSearch))
            {
                return colorDictionary[statusToSearch];
            }
            else
            {
                // Couleur par défaut
                return defaultColor;
            }
        }

    }
}
