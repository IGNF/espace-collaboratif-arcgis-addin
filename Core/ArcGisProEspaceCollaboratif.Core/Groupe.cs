namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// Un groupe de l'Espace collaboratif
    /// </summary>
    public class Groupe
    {
        /// <summary>
        /// L'identifiant du groupe
        /// </summary>
        public string id;
        /// <summary>
        /// Le nom du groupe
        /// </summary>
        public string nom;

        public string Id { get; set; }
        public string Nom { get; set; }

        /// <summary>
        /// constructeur
        /// </summary>
        public Groupe()
        {
            id = string.Empty;
            nom = string.Empty;
        }

        /// <summary>
        /// Constructeur 
        /// </summary>
        /// <param name="id">identifiant du groupe</param>
        /// <param name="nom">nom du groupe</param>
        public Groupe(string id, string nom)
        {
            this.id  = id;
            this.nom = nom;
        }
    }
}
