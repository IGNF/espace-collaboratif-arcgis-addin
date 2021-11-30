using ArcGIS.Desktop.Core;
using ArcGIS.Core.Data;
using System;
using System.Collections.Generic;
using log4net;
using ArcGisProEspaceCollaboratif.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Core.Geoprocessing;

namespace ArcGisProEspaceCollaboratif
{
    public class CollaborativeSpaceGeodatabase
    {

        #region Parameters

        /// <summary>
        /// 
        /// </summary>
        public string GeoDatabasePath { get; set; } = CoreModule.CurrentProject.DefaultGeodatabasePath;

        /// <summary>
        /// 
        /// </summary>
        public Geodatabase Geodatabase { get; set; }

        public FileGeodatabaseConnectionPath FileGeodatabaseConnectionPath { get; set; }

        /// <summary>
        /// Le logger qui permet d'enregistrer des informations sur le processus
        /// </summary>
        private static readonly Logger riplogger = Logger.Instance;
        public static readonly log4net.ILog logger = LogManager.GetLogger(typeof(CollaborativeSpaceGeodatabase));

        #endregion

        #region Constructors

        public CollaborativeSpaceGeodatabase()
        {     
            Uri gdbUri = new Uri(uriString: GeoDatabasePath);
            this.FileGeodatabaseConnectionPath = new FileGeodatabaseConnectionPath(gdbUri);
            //this.Geodatabase = new Geodatabase(this.FileGeodatabaseConnectionPath);
        }

        #endregion

        #region Other methods

        public async System.Threading.Tasks.Task InitAsync()
        {
            await QueuedTask.Run(() => {
                this.Geodatabase = new Geodatabase(this.FileGeodatabaseConnectionPath);
            });
        }

        /// <summary>
        /// Vérifie si une feature class existe dans une Geodatabase.
        /// </summary>
        /// <param name="featureClassName"> Nom de la feature class à chercher.
        /// <returns>true si la feature class existe dans la geodatabase, false sinon.</returns>
        public bool IsFeatureClassInGeodatabase(string featureClassName)
        {
            try
            {
                IReadOnlyList<FeatureClassDefinition> listFeatureClassDefinition = this.Geodatabase.GetDefinitions<FeatureClassDefinition>();
                foreach (FeatureClassDefinition lfcd in listFeatureClassDefinition)
                {
                    if (lfcd.GetName() == featureClassName)
                    {
                        return true;
                    }
                }
            }
            catch (GeodatabaseException exObj)
            {
                logger.Error(string.Format("CollaborativeSpaceGeodatabase.IsFeatureClassInGeodatabase : {0}\n", exObj.Message));
                throw new Exception(exObj.Message);
            }
            
            return false;
        }

        #endregion

        #region Empty feature class
        /// <summary>
        /// Vide une feature class de la Geodatabase si elle existe.
        /// </summary>
        /// <param name="featureClassName"> Nom de la feature class à vider.
        /// <returns>void</returns>
        public void EmptyFeatureClass (string featureClassName)
        {
            try
            {
                if (!IsFeatureClassInGeodatabase(featureClassName))
                {
                    return;
                }
                string featureClassPath = string.Format("{0}\\{1}", this.GeoDatabasePath, featureClassName);
                Geoprocessing.ExecuteToolAsync("TruncateTable_management", Geoprocessing.MakeValueArray(featureClassPath));
                // WARNING: this action creates a layer in the active map.
            }
            catch (GeodatabaseException exObj)
            {
                logger.Error(string.Format("CollaborativeSpaceGeodatabase.IsFeatureClassInGeodatabase : {0}\n", exObj.Message));
                throw new Exception(exObj.Message);
            }
        }

        #endregion

        #region Select rows in table

        /// <summary>
        /// Création d'une requête en fonction d'une liste de valeurs
        /// </summary>
        /// <param name="tableName">le nom de la table à requêter</param>
        /// <param name="fieldName">le nom du champ</param>
        /// <param name="fieldType">le type du champ</param>
        /// <param name="listValue">les valeurs à trouver</param>
        /// <returns>la requête construite</returns>
        public QueryFilter GetQueryFilter(string tableName, string fieldName, string fieldType, List<string> listValue)
        {
            QueryFilter queryFilter = new QueryFilter();

            try
            {
                using (Table table = OpenTable(tableName))
                {
                    // si la table n'existe pas, on renvoie une liste vide
                    if (table == null)
                    {
                        return queryFilter;
                    }

                    // Est-ce que le champ existe
                    if (!IsFieldInTable(table, fieldName))
                    {
                        string message = string.Format("Le champ n'existe pas dans la table {0}. Il faut demander l'aide du support collaboratif", table.GetName());
                        throw new Exception(message);
                    }

                    queryFilter = MakeQueryFilter(fieldType, fieldName, listValue);
                }
            }
            catch(Exception e)
            {
                logger.Error(string.Format("CollaborativeSpaceGeodatabase.GetQueryFilter : {0}\n", e.Message));
                throw new Exception(e.Message);
            }

            return queryFilter;
        }

       /// <summary>
       /// Ouvre une table
       /// </summary>
       /// <param name="tableName">le nom de la table</param>
       /// <returns>Une reférence si la table existe, null sinon</returns>
        private Table OpenTable(string tableName)
        {
            Table table;
            try
            {
                table = Geodatabase.OpenDataset<Table>(tableName);
            }
            catch
            {
                string message = string.Format("La table {0} n'existe pas dans la GeoDatabase", tableName);
                logger.Fatal(string.Format("CollaborativeSpaceGeodatabase.OpenTable : {0}\n", message));              
                return null;
            }
            return table;
        }

        /// <summary>
        /// Vérifie si un champ existe dans la table
        /// </summary>
        /// <param name="table">le nom de la table</param>
        /// <param name="fieldName">le nom du champ</param>
        /// <returns>true si le champ existe, false sinon</returns>
        private bool IsFieldInTable(Table table, string fieldName)
        {
            TableDefinition tableDefinition = table.GetDefinition();
            int res = tableDefinition.FindField(fieldName);
            if (res == -1)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Constitue la requête en fonction du type de champ
        /// </summary>
        /// <param name="fieldType">le type du champ long, string, ...</param>
        /// <param name="fieldName">le nom du champ</param>
        /// <param name="values">les valeurs à chercher</param>
        /// <returns>retourne la requête remplie</returns>
        private QueryFilter MakeQueryFilter(string fieldType, string fieldName, List<string> values)
        { 
            string tmp = string.Format("{0} IN (", fieldName) ;
            foreach (string value in values)
            {
                if (fieldType == "long")
                {
                    tmp += string.Format("{0},", value);
                }
            }
            string whereClause = tmp.Remove((tmp.Length - 1), 1);
            whereClause += ")";

            QueryFilter queryFilter = new QueryFilter
            {
                WhereClause = whereClause
            };

            return queryFilter;
        }

        #endregion
    }
}
