using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ArcGisProEspaceCollaboratif.Core
{
    public class Wording
    {
        /// <summary>
        /// Libellé serveur du nouveau statut
        /// </summary>
        public Status.EnumStatus Exact { get; set; }

        /// <summary>
        /// Libellé rédigé pour l'utilisateur
        /// </summary>
        public string Full { get; set; }
    }

    public class Status
    {
        /// <summary>
        /// Statut d'un signalement
        /// </summary>
        public enum EnumStatus
        {
            undefined,
            submit,
            pending,
            pending0,
            pending1,
            valid,
            valid0,
            reject,
            reject0,
            pending2
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

    }
}
