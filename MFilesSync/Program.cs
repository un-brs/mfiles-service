using System;
using System.Configuration;
using MFilesAPI;
using MFilesSync.Properties;
using NLog;
using NLog.Fluent;

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
                NetworkAddress:ConfigurationManager.AppSettings["MFilesHost"]);

            foreach (string vaultName in Settings.Default.Vaults)
            {
                VaultOnServer svault = server.GetVaults().GetVaultByName(vaultName);
                Vault vault = svault.LogIn();

                ProcessVault(vaultName, vault, Settings.Default.View, Settings.Default.StartDate);
            }


            /*var ctx = new DocumentsContext();
            ctx.Database.CreateIfNotExists();
            ctx.Database.Connection.Open();

            var testDocument = ctx.Documents.Create();
            testDocument.UnNumber = "Hello";
            testDocument.Vault = "My Vault";
            
            testDocument.CreatedDate = DateTime.Now;
            testDocument.ModifiedDate = DateTime.Now;

            var title = new TitleValue {Language = "en", Value = "Test"};


            var descr = new DescriptionValue {Language = "en", Value = "Test"};

            testDocument.Titles.Add(title);
            testDocument.Descriptions.Add(descr);


            var chemical = new ChemicalValue {Language = "en", Value = "Default value"};

            var chemicalEs = new ChemicalValue {Language = "es", Value = "Spanish name", Parent = chemical};

            testDocument.Chemicals.Add(chemical);
            testDocument.Chemicals.Add(chemicalEs);


            
            ctx.Documents.Add(testDocument);
            try
            {
                ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                foreach (var e in ctx.GetValidationErrors())
                {
                    foreach (var e1 in e.ValidationErrors)
                    {


                        Console.WriteLine(e1.PropertyName+" "+e1.ErrorMessage);
                    }
                }
                throw ex;
            }*/
        }

        private static void ProcessVault(string vaultName, Vault vault, string viewName, DateTime startDate)
        {
            logger.Info(string.Format("Process vault {0}", vaultName));
            foreach (IView view in vault.ViewOperations.GetViews())
            {
                if (view.Name == viewName)
                {
                    ProcessView(vault, viewName, view, startDate);
                    return;
                }
            }

            logger.Error(String.Format("Could not find view {0}", viewName));
        }

        private static void ProcessView(Vault vault, string viewName, IView view, DateTime startDate)
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

            while (currentDateTime < DateTime.Now)
            {
                conditions[conditions.Count - 1].TypedValue.SetValue(MFDataType.MFDatatypeDate, currentDateTime);
                currentDateTime = currentDateTime.AddMonths(1);
                conditions[conditions.Count].TypedValue.SetValue(MFDataType.MFDatatypeDate, currentDateTime);


                ObjectSearchResults objects = vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(conditions,
                    MFSearchFlags.MFSearchFlagReturnLatestVisibleVersion, false, 0);

                foreach (ObjectVersion obj in objects)
                {
                    logger.Info(obj.Title + " " + obj.LastModifiedUtc.ToShortDateString() + " " + obj.ObjectGUID);
                }
            }
        }

        private enum LogSeverity
        {
            Info = 0,
            Warning = 1,
            Error = 2
        }
    }
}