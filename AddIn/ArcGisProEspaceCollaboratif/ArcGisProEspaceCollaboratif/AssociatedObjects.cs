using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using log4net;
using System;
using System.Collections.Generic;

namespace ArcGisProEspaceCollaboratif.Core
{
    class AssociatedObjects
    {
        #region Parameters

        private readonly string strSketch = "sketch";
        private readonly string strReport = "report";

        private static readonly log4net.ILog logger = LogManager.GetLogger(typeof(CreateReport));

        /// <summary>
        /// 
        /// </summary>
        public Context Context { get; set; }

        #endregion

        #region Contructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public AssociatedObjects(Context context)
        {
            this.Context = context;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sélectionne les croquis associés a une ou plusieurs signalements 
        /// ou les signalements associés à un ou plusieurs croquis
        /// </summary>
        public void SelectObjects()
        {
            string result = this.CheckSelectedObjects();
            
            // Sélectionne les signalements associés aux croquis sélectionnés
            if (result == strSketch)
            {
                List<string> layersName = new List<string>
                {
                    Helper.name_layer_Signalement
                };
                List<string> listID = this.SelectAssociatedID(Constantes.LIEN_REPORT);
                this.SelectAssociatedObjectsFromList(layersName, Constantes.N_REPORT_IN_GDB, listID);
            }
            // Sélectionne les croquis associés aux signalements sélectionnés
            else if (result == strReport)
            {
                List<string> layersName = new List<string>
                {
                    Helper.name_layer_Croquis_Ligne,
                    Helper.name_layer_Croquis_Point,
                    Helper.name_layer_Croquis_Polygone
                };
                List<string> listID = this.SelectAssociatedID(Constantes.N_REPORT_IN_GDB);
                this.SelectAssociatedObjectsFromList(layersName, Constantes.LIEN_REPORT, listID);
            }
        }

        /// <summary>
        /// Contrôle si un des cas suivants est vrai:
        ///    1) un ou plusieurs croquis sélectionnés
        ///    2) une ou plusieurs signalements sélectionnés
        /// </summary>
        /// <returns>
        ///     - Une chaine de caractères vide si pas de sélection ou sélection d'objets de type différents
        ///     - "sketch" si des croquis sont sélectionnés, 
        ///     - "report" si des signalements sont sélectionnés
        /// </returns>
        private string CheckSelectedObjects()
        {    
            bool selectedSketch = false;
            bool selectedReport = false;

            var selectedFeatures = this.Context.MapActiveView.Map.GetSelection();
            foreach (KeyValuePair<MapMember, List<long>> kvp in selectedFeatures)
            {
                if (kvp.Key.Name == Helper.name_layer_Signalement &&
                    kvp.Value.Count > 0)
                {
                    selectedReport = true;
                }
                if (kvp.Key.Name == Helper.name_layer_Croquis_Polygone ||
                    kvp.Key.Name == Helper.name_layer_Croquis_Ligne ||
                    kvp.Key.Name == Helper.name_layer_Croquis_Point &&
                    kvp.Value.Count > 0)
                {
                    selectedSketch = true;
                }
            }

            if (selectedReport && selectedSketch)
            {
                throw new Exception("Veuillez sélectionner des signalements ou des croquis (mais pas les deux !)");
            }
            else if (selectedSketch)
            {
                return strSketch;
            }
            else if (selectedReport)
            {
                return strReport;
            }
            else
            {
                throw new Exception("Aucun croquis ou signalement sélectionné");
            }
        }

        /// <summary>
        /// En fonction des objets sélectionnés, retourne une liste d'identifiants correspondant
        /// au nom de l'attribut donné en entrée
        /// </summary>
        /// <param name="fieldName">Le nom du champ contenant les identifiants</param>
        /// <returns>La liste des identifiants</returns>
        private List<string> SelectAssociatedID(string fieldName)
        {
            List<string> listObjects = new List<string>();

            var selectedFeatures = this.Context.MapActiveView.Map.GetSelection();
            foreach (KeyValuePair<MapMember, List<long>> kvp in selectedFeatures)
            {
                var featureLayer = kvp.Key as FeatureLayer;
                List<FieldDescription> fieldDescription = featureLayer.GetFieldDescriptions();
                List<long> lOid = kvp.Value;
                foreach (long oid in lOid)
                {
                    QueuedTask.Run(() =>
                    {
                        var inspector = featureLayer.Inspect(oid);
                        Dictionary<string, string> attributes = Helper.GetAttributes(inspector, fieldDescription);
                        listObjects.Add(attributes[fieldName]);
                    });
                }
                featureLayer.ClearSelection();
            }
            return listObjects;
        }

        /// <summary>
        /// Sélectionne dans la carte les objets de la liste fournie en entrée
        /// </summary>
        /// <param name="listObjects"></param>
        private void SelectAssociatedObjectsFromList(List<string> layersName, string fieldName, List<string> listObjects)
        {
            try
            {
                foreach (string layerName in layersName)
                {
                    QueryFilter queryFilter = this.Context.CollaborativeSpaceGeodatabase.GetQueryFilter(layerName, fieldName, "long", listObjects);
                    var method = SelectionCombinationMethod.New;
                    FeatureLayer featureLayer = this.Context.GetLayerByName(layerName);
                    featureLayer.Select(queryFilter, method);
                }
            }
            catch (Exception e)
            {
                string message = string.Format("{0}\n{1}", e.Message, e.StackTrace);
                throw new Exception(message);
            }    
        }

        #endregion
    }
}
