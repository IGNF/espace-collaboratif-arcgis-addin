using System.Collections.ObjectModel;

namespace ArcGisProEspaceCollaboratif.Core
{
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
            EnumStatus.undefined,
            EnumStatus.submit,
            EnumStatus.pending,
            EnumStatus.pending0,
            EnumStatus.pending1,
            EnumStatus.pending2
        };

        static public readonly ObservableCollection<string> ListStatutWording = new ObservableCollection<string>()
        {
            "En cours de traitement",
            "En attente de saisie",
            "Pris en compte",
            "Déjà pris en compte",
            "Rejeté (hors spéc.)",
            "Rejeté (hors de propos)"
        };


    }
}