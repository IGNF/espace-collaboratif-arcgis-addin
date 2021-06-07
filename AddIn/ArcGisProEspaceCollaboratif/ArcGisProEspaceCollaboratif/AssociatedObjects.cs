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
            if (string.IsNullOrEmpty(result))
            {
                return;
            }
            else if (result == strSketch)
            {
                this.SelectAssociatedReports();
            }
            else if (result == strReport)
            {
                this.SelectAssociatedSketchs();
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
            try
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
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(
                e.Message,
                Constantes.ERROR,
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Error
                );
                logger.Error(string.Format("Problème dans la sélection des objets pour voir les objets associés : {0}\n{1}", e.Message, e.StackTrace));
            }

            return "";
        }

        /// <summary>
        /// Sélectionne les signalements associés aux croquis sélectionnés
        /// </summary>
        private void SelectAssociatedReports()
        {
            List<string> listNumberReports = new List<string>();
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
                        listNumberReports.Add(attributes[Constantes.LIEN_REPORT]);
                    });
                }
            }
            this.Context.SelectReportsByListNumber(listNumberReports);
        }

        /// <summary>
        /// Sélectionne les croquis associés aux signalements sélectionnés
        /// </summary>
        private void SelectAssociatedSketchs()
        {
           
        }

        #endregion
    }
}
