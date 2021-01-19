using System;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Classe qui gére les communications de haut niveaux entre le poste client et le serveur EspaceCollaboratif.
    /// </summary>
    public interface IClient
    {
        /// <summary>
        /// Getteur pour la version du serveur EspaceCollaboratif sur lequel on est connecté.
        /// </summary>
        /// <returns></returns>
        String GetVersion();

        /// <summary>
        /// Getteur pour obtenir le profil du client connecté en cours.
        /// </summary>
        /// <returns></returns>
        Profil GetProfil();

        /// <summary>
        /// Renvoie la liste des remarques EspaceCollaboratif pour une zone données
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="box"></param>
        /// <param name="date"></param>
        /// <param name="idGroupe"></param>
        /// <returns></returns>
        //List<Remarque> GetRemarques(ZoneGeographique zone, Box box, DateTime date, int idGroupe);

        /// <summary>
        /// Renvoie la liste des remarques EspaceCollaboratif pour une zone données
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="box"></param>
        /// <param name="pagination"></param>
        /// <param name="date"></param>
        /// <param name="idGroupe"></param>
        /// <returns></returns>
       // List<Remarque> GetRemarques(ZoneGeographique zone, Box box, int pagination, DateTime date, int idGroupe);

        /// <summary>
        /// Renvoie la remarques EspaceCollaboratif donnée par son identifiant
        /// </summary>
        /// <param name="idRemarque"></param>        
        /// <returns></returns>
       // Remarque GetRemarque(UInt64 idRemarque);

        /// <summary>
        /// Ajoute une nouvelle réponse à une remarque EspaceCollaboratif donnée. Sert aussi à modifer son statut.
        /// </summary>
        /// <param name="remarque"></param>
        /// <param name="reponse"></param>       
        /// <returns></returns>
        Remarque AddReponse(Remarque remarque, string reponse, string titre );

        /// <summary>
        /// Crée une nouvelle remarque
        /// </summary>        
        Remarque CreateRemarque(Remarque nouvelleRemarque                                                                            );

        int Get_MAX_TAILLE_UPLOAD_FILE();
        int Get_NB_DEFAULT_REMARQUES_PAGINATION();
        
    }
}