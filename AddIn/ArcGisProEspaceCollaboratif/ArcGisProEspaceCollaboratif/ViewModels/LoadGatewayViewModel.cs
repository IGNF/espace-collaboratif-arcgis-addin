using ArcGisProEspaceCollaboratif.Core;
using ArcGisProEspaceCollaboratif.Views;
using System.Collections.Generic;

namespace ArcGisProEspaceCollaboratif.ViewModels
{
    class LoadGatewayViewModel : ViewModelBase
    {
        #region Parameters
        /// <summary>
        /// L'instance du dialogue "Charger les couches de mon groupe"
        /// </summary>
        public LoadGatewayView loadGatewayView;

        /// <summary>
        /// Toutes les données contextuelles liées à la carte et à l'Espace collaboratif
        /// </summary>
        public Contexte Context { get; set; }

        /// <summary>
        /// La liste de toutes les couches disponibles pour le groupe de l'utilisateur 
        /// </summary>
        public List<LayerGateway> ListLayers { get; set; }

        /// <summary>
        /// Le profil de l'utilisateur
        /// </summary>
        public Profil UserProfile { get; set; }

        public const string strLabel = "Groupe actif : ";

        /// <summary>
        /// Balises <ROLE>
        /// Role de la couche dans le cadre d'un guichet
        /// - visu = couche servant de fond uniquement
        /// - ref = couche servant de référence pour la saisie (snapping ou routing)
        /// - edit, ref-edit = couche éditable sur le guichet
        ///
        /// Le dictionnaire est de la forme "clé xml":"valeur affichage boite"
        /// </summary>
        readonly Dictionary<string, string> roleKeyValue = new Dictionary<string, string>
        {
            { "edit" , "Edition" },
            { "ref-edit" , "Edition" },
            { "visu" , "Visualisation" },
            { "ref" , "Visualisation" }
        };
        #endregion

        #region Constructors
        /// <summary>
        /// Initialisation du dialogue "Charger les couches de mon groupe"
        /// </summary>
        public LoadGatewayViewModel(Contexte context)
        {
            this.Context = context;
            this.AtiveGroupLabelContent = string.Format("{0}{1}", strLabel, this.Context.Profil.Group.Name);
            this.loadGatewayView = new LoadGatewayView();
        }
        #endregion

        #region Bindings
        /// <summary>
        /// Indication du groupe actif, c'est à dire le groupe
        /// à partir duquel l'utilisateur va pouvoir sélectionner ses couches WFS et WMTS
        /// </summary>
        public string AtiveGroupLabelContent { get; set; } = strLabel;
        #endregion

        #region Commands
        #endregion
    }
}
