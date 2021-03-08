using System;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe qui gére les communications de haut niveaux entre le poste client et le serveur EspaceCollaboratif.
    /// </summary>
    public interface IClient
    {
        /// <summary>
        /// Getter pour la version du serveur EspaceCollaboratif sur lequel on est connecté.
        /// </summary>
        /// <returns></returns>
        String GetVersion();

        /// <summary>
        /// Getter pour obtenir le profil du client connecté en cours.
        /// </summary>
        /// <returns></returns>
        Profil GetProfil();

        /// <summary>
        /// Ajoute une nouvelle réponse à une remarque EspaceCollaboratif donnée. Sert aussi à modifer son statut.
        /// </summary>
        /// <param name="remarque"></param>
        /// <param name="reponse"></param>       
        /// <returns></returns>
        Signalement AddReponse(Signalement remarque, string reponse, string titre );

        /// <summary>
        /// Crée une nouvelle remarque
        /// </summary>        
        Signalement CreateSignalement(Signalement nouveauSignalement);

        int Get_MAX_TAILLE_UPLOAD_FILE();
        int Get_NB_DEFAULT_SIGNALEMENTS_PAGINATION();
    }
}