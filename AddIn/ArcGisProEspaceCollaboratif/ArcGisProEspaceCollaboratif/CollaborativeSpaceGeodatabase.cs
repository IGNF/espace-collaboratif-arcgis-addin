using ArcGIS.Desktop.Core;
using ArcGIS.Core.Data;
using System;
using System.Collections.Generic;
using log4net;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Core.Data.Exceptions;
using ArcGIS.Core.Data.DDL;
using ArcGisProEspaceCollaboratif.Core;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading;
using ArcGIS.Core.Internal.CIM;
using System.IO;
using ArcGIS.Core.Data.UtilityNetwork.Trace;

namespace ArcGisProEspaceCollaboratif
{
    public class CollaborativeSpaceGeodatabase
    {

        #region Parameters

        /// <summary>
        /// 
        /// </summary>
        public string GeoDatabasePath { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Geodatabase Geodatabase { get; private set; }

        public FileGeodatabaseConnectionPath FileGeodatabaseConnectionPath { get; private set; }

        public bool IsOpen { get; private set; }

        /// <summary>
        /// Le logger qui permet d'enregistrer des informations sur le processus
        /// </summary>
        private static readonly Logger riplogger = Logger.Instance;
        public static readonly log4net.ILog logger = LogManager.GetLogger(typeof(CollaborativeSpaceGeodatabase));

        #endregion

        #region Constructors

        public CollaborativeSpaceGeodatabase()
        {
            string gdbDirectory = System.IO.Path.GetDirectoryName(CoreModule.CurrentProject.DefaultGeodatabasePath);
            // Définir le chemin complet de la geodatabase
            this.GeoDatabasePath = System.IO.Path.Combine(gdbDirectory, $"{Constantes.ESPACECO_GDB}.gdb");

            // Créer la geodatabase si elle n'existe pas
            CreateOrNotFileGeodatabase(GeoDatabasePath, gdbDirectory, Constantes.ESPACECO_GDB);

            // Initialiser les objets de connexion
            Uri gdbUri = new Uri(GeoDatabasePath);
            this.FileGeodatabaseConnectionPath = new FileGeodatabaseConnectionPath(gdbUri);
            this.Geodatabase = new Geodatabase(FileGeodatabaseConnectionPath);
            this.IsOpen = true;
        }

        public void Close()
        {
            this.Geodatabase.Dispose();
            IsOpen = false;
        }

        private static async void CreateOrNotFileGeodatabase(string gdbPath, string folderPath, string gdbName)
        {
            try
            {
                // Vérifie si la geodatabase existe déjà
                if (Directory.Exists(gdbPath))
                {
                    logger.Info($"CollaborativeSpaceGeodatabase.CreateOrNotFileGeodatabase, la geodatabase existe déjà : {gdbPath}");
                    return;
                }

                await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(async () =>
                {
                    var parameters = ArcGIS.Desktop.Core.Geoprocessing.Geoprocessing.MakeValueArray(folderPath, gdbName, "CURRENT");
                    var result = await ArcGIS.Desktop.Core.Geoprocessing.Geoprocessing.ExecuteToolAsync("management.CreateFileGDB", parameters, null);
                    if (result.IsFailed)
                    {
                        logger.Error($"CollaborativeSpaceGeodatabase.CreateOrNotFileGeodatabase, échec de la création de la geodatabase : {gdbPath}");
                        throw new Exception();
                    }
                });
            }
            catch (Exception ex)
            {
                logger.Error($"CollaborativeSpaceGeodatabase.CreateOrNotFileGeodatabase : {ex.Message}\n");
                throw;
            }
        }

        #endregion

        #region Other methods

        /// <summary>
        /// Vérifie si une table existe dans une Geodatabase.
        /// </summary>
        /// <param name="tableName"> Nom de la table à chercher.
        /// <returns>true si la table existe dans la geodatabase, false sinon.</returns>
        public bool IsTableExists(string tableName)
        {
            try
            {
                TableDefinition tableDefinition = this.Geodatabase.GetDefinition<TableDefinition>(tableName);
                tableDefinition.Dispose();
                return true;
            }
            catch
            {
                // GetDefinition throws an exception if the definition doesn't exist
                return false;
            }
        }

        /// <summary>
        /// Vérifie si une feature class existe dans une Geodatabase.
        /// </summary>
        /// <param name="featureClassName"> Nom de la feature class à chercher.
        /// <returns>true si la feature class existe dans la geodatabase, false sinon.</returns>
        public bool IsFeatureClassExists(string featureClassName)
        {
            try
            {
                FeatureClassDefinition featureClassDefinition = this.Geodatabase.GetDefinition<FeatureClassDefinition>(featureClassName);
                featureClassDefinition.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
            
        }

        #endregion

        #region Empty feature class
        /// <summary>
        /// Vide une feature class de la Geodatabase si elle existe.
        /// </summary>
        /// <param name="featureClassName"> Nom de la feature class à vider.
        /// <returns>void</returns>
        public void EmptyFeatureClass(string featureClassName)
        {
            try
            {
                string featureClassPath = string.Format("{0}\\{1}", this.GeoDatabasePath, featureClassName);
                Task<IGPResult> igpresult = Geoprocessing.ExecuteToolAsync("TruncateTable_management", Geoprocessing.MakeValueArray(featureClassPath));
            }
            catch (GeodatabaseException exObj)
            {
                logger.Error(string.Format("CollaborativeSpaceGeodatabase.IsFeatureClassInGeodatabase : {0}\n", exObj.Message));
                throw new Exception(exObj.Message);
            }
        }

        #endregion

        #region Select rows in table


        private static ArcGIS.Core.Data.QueryFilter EmptySelectionFilter()
            => new ArcGIS.Core.Data.QueryFilter { WhereClause = "1=0" };

        /// <summary>
        /// Création d'une requête en fonction d'une liste de valeurs
        /// </summary>
        /// <param name="tableName">le nom de la table à requêter</param>
        /// <param name="fieldName">le nom du champ</param>
        /// <param name="fieldType">le type du champ</param>
        /// <param name="listValue">les valeurs à trouver</param>
        /// <returns>la requête construite</returns>
        public ArcGIS.Core.Data.QueryFilter GetQueryFilter(string tableName, string fieldName, string fieldType, List<string> listValue)
        {
            ArcGIS.Core.Data.QueryFilter queryFilter = new ArcGIS.Core.Data.QueryFilter();

            try
            {
                using Table table = OpenTable(tableName);

                // Est-ce que le champ existe
                if (!IsFieldInTable(table, fieldName))
                {
                    string message = string.Format("Le champ n'existe pas dans la table {0}. Il faut demander l'aide du support collaboratif", table.GetName());
                    throw new Exception(message);
                }

                // Liste vide ou nulle
                if (listValue == null || listValue.Count == 0)
                {
                    return EmptySelectionFilter();
                }

                queryFilter = MakeQueryFilter(fieldType, fieldName, listValue);
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
       /// <returns>Une reférence si la table existe</returns>
        private Table OpenTable(string tableName)
        {
            Table table;
            try
            {
                table = Geodatabase.OpenDataset<Table>(tableName);
            }
            catch (Exception e)
            {
                string message = string.Format("La GeoDatabase est impossible à ouvrir ou la table {0} n'existe pas.\n{1}", tableName, e.Message);
                logger.Fatal(string.Format("CollaborativeSpaceGeodatabase.OpenTable : {0}\n", message));
                throw new Exception(message);
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

        public bool IsFieldsInTable(string nameTable, Dictionary<string, KeyValuePair<string, string>> dictFieldsName)
        {
            Table table = this.OpenTable(nameTable);
            foreach (KeyValuePair<string, KeyValuePair<string, string>> kvp in dictFieldsName)
            {
                if (!this.IsFieldInTable(table, kvp.Key))
                {
                    table.Dispose();
                    return false;
                }
            }
            table.Dispose();
            return true;
        }

        /// <summary>
        /// Constitue la requête en fonction du type de champ
        /// </summary>
        /// <param name="fieldType">le type du champ long, string, ...</param>
        /// <param name="fieldName">le nom du champ</param>
        /// <param name="values">les valeurs à chercher</param>
        /// <returns>retourne la requête remplie</returns>
        static private ArcGIS.Core.Data.QueryFilter MakeQueryFilter(string fieldType, string fieldName, List<string> values)
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

            ArcGIS.Core.Data.QueryFilter queryFilter = new ArcGIS.Core.Data.QueryFilter
            {
                WhereClause = whereClause
            };

            return queryFilter;
        }

        #endregion
    }
}
