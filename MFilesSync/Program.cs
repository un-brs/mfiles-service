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

        private static void ProcessVault(DocumentsContext ctx, string vaultName, Vault vault, string viewName,
            DateTime startDate)
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

        private static void ProcessView(DocumentsContext ctx, string vaultName, Vault vault, string viewName, IView view,
            DateTime startDate)
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

                internalDocuments.AddRange(from ObjectVersion obj in objects
                    select new MFilesInternalDocument(internalVault, obj));
            }

            var dict = internalDocuments.GroupBy(w => w.UnNumber).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var unNumber in dict)
            {
                var internalDocument =
                    unNumber.Value.FirstOrDefault(d => d.File.Extension == "pdf" && d.Language == "English") ??
                    unNumber.Value[0];
                var mainDocument = ProcessMainDocument(ctx, internalDocument);

                foreach (var other in unNumber.Value.FindAll(d => d != internalDocument))
                {
                    ProcessDependantDocument(ctx, mainDocument, other);
                }
                ctx.Documents.Add(mainDocument);
                ctx.SaveChanges();

            }
        }

        private static Document ProcessMainDocument(DocumentsContext ctx, MFilesInternalDocument internalDocument)
        {
            var doc = ctx.Documents.Create();
            doc.InternalDocument = internalDocument.MFilesDocument;
            doc.PublicationDate = doc.InternalDocument.CreatedDate;
            doc.PublicationUpdateDate = doc.InternalDocument.ModifiedDate;
            doc.Vault = internalDocument.VaultName;
            doc.UnNumber = internalDocument.UnNumber;

            ProcessDependantDocument(ctx, doc, internalDocument);
            return doc;
        }

        private static void ProcessDependantDocument(DocumentsContext ctx, Document doc,
            MFilesInternalDocument internalDocument)
        {
            var title = new TitleValue();
            var description = new DescriptionValue();
            var file = new File();

            title.Document = doc;
            description.Document = doc;
            file.Document = doc;


            var mfilesDocument =
                ctx.MFilesDocuments.FirstOrDefault(d => d.MFilesDocumentGuid == internalDocument.ObjectGuid) ??
                internalDocument.MFilesDocument;

            title.MFilesDocument = mfilesDocument;
            description.MFilesDocument = mfilesDocument;
            file.MFilesDocument = mfilesDocument;

            title.Value = internalDocument.GetStringValue("Name or title");
            description.Value = internalDocument.GetStringValue("Description");

            title.Language = internalDocument.Language.Substring(0, 2);
            description.Language = internalDocument.Language.Substring(0, 2);
            file.Language = internalDocument.Language.Substring(0, 2);
            file.Name = internalDocument.File.Title;
            file.MimeType = internalDocument.File.Extension;
            file.Size = internalDocument.File.LogicalSize;

            doc.Titles.Add(title);
            doc.Descriptions.Add(description);
            doc.Files.Add(file);

        }
    }
}