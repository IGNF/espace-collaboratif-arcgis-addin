using ArcGIS.Desktop.Core;
using ArcGIS.Core.Data;
using System;
using System.Collections.Generic;

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

        #endregion

        #region Constructors

        public CollaborativeSpaceGeodatabase()
        {
            Uri gdbUri = new Uri(uriString: GeoDatabasePath);
            FileGeodatabaseConnectionPath fileGeodatabaseConnectionPath = new FileGeodatabaseConnectionPath(gdbUri);
            this.Geodatabase = new Geodatabase(fileGeodatabaseConnectionPath);
        }

        #endregion

        #region Other methods

        /// <summary>
        /// Vérifie si une feature class existe dans une Geodatabase.
        /// </summary>
        /// <param name="featureClassName"> Nom de la feature class à chercher.
        /// <returns>true si la feature class existe dans la geodatabase, false sinon.</returns>
        public bool IsFeatureClassExistInGeodatabase(string featureClassName)
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
                throw new Exception(exObj.Message);
            }
            
            return false;
        }

        #endregion

        #region Select rows in table

        /// <summary>
        /// Sélectionne des enregistrements dans une table en fionction d'une liste de valeurs
        /// </summary>
        /// <param name="tableName">le nom de la table à requêter</param>
        /// <param name="fieldName">le nom du champ</param>
        /// <param name="fieldType">le type du champ</param>
        /// <param name="listValue">les valeurs à trouver</param>
        /// <returns>une liste remplie, vide sinon</returns>
        public List<Row> SelectRowInTable(string tableName, string fieldName, string fieldType, List<string> listValue)
        {
            List<Row> rows = new List<Row>();

            try
            {
                using (Table table = OpenTable(tableName))
                {
                    // si la table n'existe pas, on renvoie une liste vide
                    if (table == null)
                    {
                        return rows;
                    }

                    // Est-ce que le champ existe
                    if (!IsFieldExistInTable(table, fieldName))
                    {
                        throw new Exception(string.Format("Le champ n'existe pas dans la table {0}. Il faut demander l'aide du support collaboratif", table.GetName()));
                    }
                    
                    foreach (string value in listValue)
                    {
                        QueryFilter queryFilter = MadeQueryFilter(fieldType, fieldName, value);
                        rows = GetListRows(table, queryFilter);
                    }
                }
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }

            return rows;
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
        private bool IsFieldExistInTable(Table table, string fieldName)
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
        /// <param name="value">la valeur du champ</param>
        /// <returns>retourne la requête remplie</returns>
        private QueryFilter MadeQueryFilter(string fieldType, string fieldName, string value)
        { 
            string whereClause = "";
            if (fieldType == "long")
            {
                whereClause = string.Format("{0} = {1}", fieldName, value);
            }
            if (fieldType == "string")
            {
                whereClause = string.Format("{0} = '{1}'", fieldName, value);
            }
            QueryFilter queryFilter = new QueryFilter
            {
                WhereClause = whereClause
            };

            return queryFilter;
        }

        /// <summary>
        /// Retourne les enregistrements liés à la table à partir de la requête fournie en entrée.
        /// </summary>
        /// <param name="table">la table a requêter</param>
        /// <param name="queryFilter">la requête</param>
        /// <returns>une liste remplie ou vide si problème</returns>
        private List<Row> GetListRows(Table table, QueryFilter queryFilter)
        {
            List<Row> rows = new List<Row>();
            try
            {
                using (RowCursor rowCursor = table.Search(queryFilter, false))
                {
                    while (rowCursor.MoveNext())
                    {
                        rows.Add(rowCursor.Current);
                    }
                }
            }
            catch (GeodatabaseFieldException fieldException)
            {
                throw new Exception(fieldException.Message);
            }

            return rows;
        }

        #endregion

    }
}
