using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Conventions.MFiles.Models;
using MFilesAPI;
using MFilesSync.Properties;
using NLog;

namespace MFilesSync
{
    internal class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            var server = new MFilesServerApplication();
            logger.Error(ConfigurationManager.AppSettings["MFilesHost"]);
            MFServerConnection conn = server.Connect(MFAuthType.MFAuthTypeSpecificMFilesUser,
                ConfigurationManager.AppSettings["MFilesUser"],
                ConfigurationManager.AppSettings["MFilesPassword"],
                NetworkAddress: ConfigurationManager.AppSettings["MFilesHost"]);

            var ctx = new DocumentsContext();
            ctx.Database.CreateIfNotExists();
            ctx.Database.Connection.Open();

            foreach (string vaultName in Settings.Default.Vaults)
            {
                VaultOnServer svault = server.GetVaults().GetVaultByName(vaultName);
                Vault vault = svault.LogIn();

                ProcessVault(ctx, vaultName, vault, Settings.Default.View, Settings.Default.StartDate);
            }


            
         
        }

        private static void ProcessVault(DocumentsContext ctx, string vaultName, Vault vault, string viewName, DateTime startDate)
        {
            logger.Info(string.Format("Process vault {0}", vaultName));
            foreach (IView view in vault.ViewOperations.GetViews())
            {
                if (view.Name == viewName)
                {
                    ProcessView(ctx, vaultName, vault, viewName, view, startDate);
                    return;
                }
            }

            logger.Error(String.Format("Could not find view {0}", viewName));
        }

        private static void ProcessView(DocumentsContext ctx, string vaultName, Vault vault, string viewName, IView view, DateTime startDate)
        {
            logger.Info(string.Format("Process view {0}", viewName));

            SearchConditions conditions = view.SearchConditions;
            var dfDate = new DataFunctionCall();
            dfDate.SetDataDate();

            var search = new SearchCondition();
            var expression = new Expression();
            var value = new TypedValue();

            expression.SetPropertyValueExpression((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModified,
                MFParentChildBehavior.MFParentChildBehaviorNone, dfDate);
            search.Set(expression, MFConditionType.MFConditionTypeGreaterThanOrEqual, value);

            conditions.Add(-1, search);

            search = new SearchCondition();
            expression = new Expression();
            value = new TypedValue();
            expression.SetPropertyValueExpression((int) MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModified,
                MFParentChildBehavior.MFParentChildBehaviorNone, dfDate);
            search.Set(expression, MFConditionType.MFConditionTypeLessThan, value);

            conditions.Add(-1, search);


            DateTime currentDateTime = startDate;

            var internalDocuments = new List<MFilesInternalDocument>();
            var internalVault = new MFilesVault(vaultName, vault);

            while (currentDateTime < DateTime.Now)
            {
                conditions[conditions.Count - 1].TypedValue.SetValue(MFDataType.MFDatatypeDate, currentDateTime);
                currentDateTime = currentDateTime.AddMonths(1);
                conditions[conditions.Count].TypedValue.SetValue(MFDataType.MFDatatypeDate, currentDateTime);


                ObjectSearchResults objects = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(conditions,
                    MFSearchFlags.MFSearchFlagReturnLatestVisibleVersion, false, 0);

                internalDocuments.AddRange(from ObjectVersion obj in objects select new MFilesInternalDocument(internalVault, obj));
            }

            var dict = internalDocuments.GroupBy(w => w.UnNumber).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var unNumber in dict)
            {
                var internalDocument = unNumber.Value.FirstOrDefault(d => d.File.Extension == "pdf") ??
                                       unNumber.Value[0];
                ProcessMainDocument(ctx, internalDocument);
            }
        }

        private static void ProcessMainDocument(DocumentsContext ctx, MFilesInternalDocument internalDocument)
        {
            var doc = ctx.Documents.Create();
            doc.InternalDocument = internalDocument.MFilesDocument;
        }
    }
}