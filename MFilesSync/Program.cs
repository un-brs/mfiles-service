using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using Conventions.MFiles.Models;
using MFilesAPI;
using MFilesModel;
using MFilesSync.Properties;
using MimeSharp;
using NLog;



namespace MFilesSync
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly  Mime Mime = new Mime();

        private const string ErrorNotMacthedCrm = "W01";

        private static void Main(string[] args)
        {
            var server = new MFilesServerApplication();
            Logger.Info("Start syncronization");
            Logger.Info(String.Format("MFiles Host:User = {0}:{1}", ConfigurationManager.AppSettings["MFilesHost"],
                ConfigurationManager.AppSettings["MFilesUser"])
            );
            var conn = server.Connect(MFAuthType.MFAuthTypeSpecificMFilesUser,
                ConfigurationManager.AppSettings["MFilesUser"],
                ConfigurationManager.AppSettings["MFilesPassword"],
                NetworkAddress: ConfigurationManager.AppSettings["MFilesHost"]);

            var ctx = new DocumentsContext();
            ctx.Database.CreateIfNotExists();
            ctx.Database.Connection.Open();

            ProcessTermsService(ctx, Settings.Default.TermsServiceUri);

            foreach (var vaultName in Settings.Default.Vaults)
            {
                var svault = server.GetVaults().GetVaultByName(vaultName);
                var vault = svault.LogIn();

                ProcessVault(ctx, vaultName, vault, Settings.Default.View, Settings.Default.StartDate);
            }

            ctx.SaveChanges();

            Console.WriteLine();
            Console.WriteLine("Finished. Press any key to exit ...");
            Console.ReadKey();
        }

        private static MFilesDocument CreateMFilesDocument(MFilesInternalDocument doc)
        {
            var mfilesDocument = new MFilesDocument
            {
                MFilesDocumentGuid = doc.ObjectGuid,
                CreatedDate = doc.CreatedDate,
                ModifiedDate = doc.ModifiedDate
            };

            return mfilesDocument;
        }

        private static void ProcessTermsService(DocumentsContext ctx, string termsServiceUri)
        { 
            
            var sctx = new TermsCrm.TermsServiceReference.asbMeetingEntities(new Uri(termsServiceUri));

            var chemicalQuery = from term in sctx.Terms where term.ParentTermNames.Contains("Chemicals") select term;
            foreach (var t in chemicalQuery)
            {
                var val = new ChemicalValue {Language = "en", Value = t.Name, ExternalTermId = t.TermId};
                ctx.Values.Add(val);
            }


            var programmesQuery = from term in sctx.Terms where term.ParentTermNames.Contains("Programmes") select term;
            foreach (var t in programmesQuery)
            {
                var val = new ProgramValue { Language = "en", Value = t.Name };
                val.ExternalTermId = t.TermId;
                ctx.Values.Add(val);
            }

            var termsQuery = from term in sctx.Terms where term.ParentTermNames.Contains("Scientific and Technical Publications Terms") select term;
            foreach (var t in termsQuery)
            {
                var val = new TermValue { Language = "en", Value = t.Name };
                val.ExternalTermId = t.TermId;
                ctx.Values.Add(val);
            }

            var meetingsQuery = from term in sctx.Terms where term.ParentTermNames.Contains("Meetings") select term;
            foreach (var t in meetingsQuery)
            {
                var val = new MeetingValue { Language = "en", Value = t.Name };
                val.ExternalTermId = t.TermId;
                ctx.Values.Add(val);
            }

        }

        private static void ProcessVault(DocumentsContext ctx, string vaultName, Vault vault, string viewName,
            DateTime startDate)
        {
            Logger.Info(string.Format("Process vault {0}", vaultName));
            var internalVault = new MFilesVault(vaultName, vault);

            ProcessVaultDocumentTypes(ctx, internalVault);
            ctx.SaveChanges();
            foreach (IView view in vault.ViewOperations.GetViews())
            {
                if (view.Name == viewName)
                {
                    ProcessView(ctx, internalVault, viewName, view, startDate);
                    return;
                }
            }

            Logger.Error(String.Format("Could not find view {0}", viewName));
        }

        private static void ProcessView(DocumentsContext ctx, MFilesVault internalVault, string viewName, IView view,
            DateTime startDate)
        {
            Logger.Info(string.Format("Process view {0}", viewName));

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


            while (currentDateTime < DateTime.Now)
            {
                conditions[conditions.Count - 1].TypedValue.SetValue(MFDataType.MFDatatypeDate, currentDateTime);
                currentDateTime = currentDateTime.AddMonths(1);
                conditions[conditions.Count].TypedValue.SetValue(MFDataType.MFDatatypeDate, currentDateTime);


                ObjectSearchResults objects = internalVault.Vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(conditions,
                    MFSearchFlags.MFSearchFlagReturnLatestVisibleVersion, false, 0);

                internalDocuments.AddRange(from ObjectVersion obj in objects
                    select new MFilesInternalDocument(internalVault, obj));
            }
            Logger.Info("Number of document to process {0}", internalDocuments.Count);
            var dict = internalDocuments.GroupBy(w => w.UnNumber).ToDictionary(g => g.Key, g => g.ToList());
            Logger.Info("Number of UN-Numbers to process {0}", dict.Count);
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
                try
                {
                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to save document " + internalDocument.FileName);
                    Logger.Error(ex.ToString());
                    Logger.Error(ex.StackTrace);
                    ctx.Documents.Remove(mainDocument);
                }

            }
        }

        private static Document ProcessMainDocument(DocumentsContext ctx, MFilesInternalDocument internalDocument)
        {
            var doc = ctx.Documents.Create();

            doc.MFilesDocument = CreateMFilesDocument(internalDocument);
            doc.PublicationUpdateDate = doc.MFilesDocument.ModifiedDate;
            doc.Vault = internalDocument.VaultName;
            doc.UnNumber = internalDocument.UnNumber;

            var copyright = internalDocument.GetCopyright();
            if (!String.IsNullOrEmpty(copyright)) {
                doc.Copyright = internalDocument.GetCopyright();
            }

  
            ProcessDependantDocument(ctx, doc, internalDocument);
            ProcessPublicationDates(doc, internalDocument);
            ProcessPeriods(doc, internalDocument);
            ProcessAuthorAndCountry(doc, internalDocument);
            ProcessDocumentTypes(ctx, doc, internalDocument);
            ProcessChemicals(ctx, doc, internalDocument);
            ProcessProgrammes(ctx, doc, internalDocument);
            ProcessTerms(ctx, doc, internalDocument);
            ProcessMeeting(ctx, doc, internalDocument);
  

            return doc;
        }

        private static void ProcessPeriods(Document doc, MFilesInternalDocument internalDocument)
        {
            var periods = internalDocument.GetPeriodBiennium();

            var iperiods = new List<int>();
            foreach (var period in periods)
            {
                var startAndEnd = period.Split('-');
                foreach (var p in startAndEnd)
                {
                    var year = 0;
                    if (int.TryParse(p, out year))
                    {
                        iperiods.Add(year);
                    }
                }     
            }

           
            if (iperiods.Count > 0)
            {
                doc.PeriodStartDate = new DateTime(iperiods[0],1,1);
            }

            if (iperiods.Count > 1)
            {
                doc.PeriodEndDate = new DateTime(iperiods[iperiods.Count-1], 1, 1);
            }
        }

        private static void ProcessPublicationDates(Document doc, MFilesInternalDocument internalDocument)
        {
            var d = internalDocument.GetTransmissionDate();
            if (d.HasValue)
            {
                doc.PublicationDate = d.Value;
                return;
            }

            d = internalDocument.GetDateIssuanceSignature();
            if (d.HasValue)
            {
                doc.PublicationDate = d.Value;
                return;
            }
            d = internalDocument.GetDateOfCorrespondence();
            if (d.HasValue)
            {
                doc.PublicationDate = d.Value;
                return;
            }

            d = internalDocument.GetDateStart();
            if (d.HasValue)
            {
                doc.PublicationDate = d.Value;
                return;
            }

            var sd = internalDocument.GetPublicationDateDisplay();
            try
            {
                var sdDate = DateTime.ParseExact(sd, "MMMM yyyy", CultureInfo.CurrentCulture);
                doc.PublicationDate = sdDate;
                return;
            }
            catch (FormatException)
            {

            }

            var pubMonth = internalDocument.GetPublicationDateMonth();
            var pubYear = internalDocument.GetPublicationDateYear();

            if (!(String.IsNullOrWhiteSpace(pubMonth) || String.IsNullOrWhiteSpace(pubYear)))
            {
                try
                {
                    var sdDate = DateTime.ParseExact(pubMonth + " " + pubYear, "MMMM yyyy", CultureInfo.CurrentCulture);
                    doc.PublicationDate = sdDate;
                    return;
                }
                catch (FormatException)
                {

                }
            }

            doc.PublicationDate = internalDocument.CreatedDate;

        }


        private static void ProcessAuthorAndCountry(Document doc, MFilesInternalDocument internalDocument)
        {
            var author = internalDocument.GetAuthor();
            if (!String.IsNullOrEmpty(author))
            {
                doc.Author = internalDocument.GetAuthor();
            }

            var player = internalDocument.GetPlayer();
            if (String.IsNullOrEmpty(player))
            {
                return;
            }

            var country = CultureUtil.GetCountryTwoLetterCode(player);
            if (country != null)
            {
                doc.Country = country;
            }
            else if (!String.IsNullOrWhiteSpace(doc.Author))
            {
                doc.Author = player;
            }
        }

        private static void ProcessDocumentTypes(DocumentsContext ctx, Document doc, MFilesInternalDocument internalDocument)
        {
            foreach (var docType in internalDocument.GetDocumentTypes())
            {
                doc.Types.Add(ctx.Values.OfType<TypeValue>().First(v => v.Value == docType));
            }
        }

        private static void ProcessChemicals(DocumentsContext ctx, Document doc, MFilesInternalDocument internalDocument)
        {

            ProcessListProperties("Chemicals", internalDocument.FileName, ctx, doc.Chemicals, internalDocument.GetChemicals());
        }


        private static void ProcessProgrammes(DocumentsContext ctx, Document doc, MFilesInternalDocument internalDocument)
        {

            ProcessListProperties("Programmes", internalDocument.FileName, ctx, doc.Programs, internalDocument.GetProgrammes());
        }

        private static void ProcessTerms(DocumentsContext ctx, Document doc, MFilesInternalDocument internalDocument)
        {

            ProcessListProperties("Terms", internalDocument.FileName, ctx, doc.Terms, internalDocument.GetTerms());
        }

        private static void ProcessMeeting(DocumentsContext ctx, Document doc, MFilesInternalDocument internalDocument)
        {
            string meeting = internalDocument.GetMeeting();
            if (String.IsNullOrEmpty(meeting))
            {
                return;
            }
            doc.Meeting =  ctx.Values.Where(w => w.Value == meeting).OfType<MeetingValue>().FirstOrDefault();
            if (doc.Meeting == null)
            {
                LogNotMatchedInCrm("Meeting", internalDocument.FileName, new string[]{meeting});        
            }
        }


        private static void ProcessListProperties<T>(string name, string fileName, DocumentsContext ctx, ICollection<T> target, IEnumerable<string> values) where T : ListProperty
        {
            foreach (var val in ctx.Values.Where(w => values.Contains(w.Value)).OfType<T>())
            {
                target.Add(val);
            }

            LogNotMatchedInCrm(name, fileName, values.Where(w => target.All(v => v.Value != w)).ToList());
        }

        private static void LogNotMatchedInCrm(string termTitle, string fileName, ICollection<string> notMatched)
        {
            if (notMatched.Count != 0)
            {
                Logger.Warn(ErrorNotMacthedCrm + " # " + fileName + " # " + termTitle + " # not matched in CRM # " + String.Join(", ", notMatched));
            }
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

            var twoLetterLanguage = CultureUtil.GetLangTwoLetterCode(internalDocument.Language);


            var mfilesDocument =
                ctx.MFilesDocuments.FirstOrDefault(d => d.MFilesDocumentGuid == internalDocument.ObjectGuid) ??
                (doc.MFilesDocument.MFilesDocumentGuid == internalDocument.ObjectGuid
                    ? doc.MFilesDocument
                    : CreateMFilesDocument(internalDocument)
                );

            title.MFilesDocument = mfilesDocument;
            description.MFilesDocument = mfilesDocument;
            file.MFilesDocument = mfilesDocument;

            title.Value = internalDocument.GetTitle();
            description.Value = internalDocument.GetDescription();

            title.Language = twoLetterLanguage;
            description.Language = twoLetterLanguage;
            file.Language = twoLetterLanguage;

            file.Name = internalDocument.File.Title;
            file.Extension = internalDocument.File.Extension.ToLower();
            file.MimeType = Mime.Lookup(file.Name + "." + file.Extension);
            file.Size = internalDocument.File.LogicalSize;
            string urlPropertieKey = doc.Vault.Replace(" ","") + "Url";
            if (Settings.Default.Properties[urlPropertieKey] != null)
            {
                file.Url = Settings.Default.Properties[urlPropertieKey].DefaultValue.ToString() + file.Name+"."+file.Extension;
            }
            else
            {
                Logger.Error("Could not find download url for " + doc.Vault);
                file.Url = file.Name + "." + file.Extension.ToLower();
            }

 
            if (null == doc.Titles.AsQueryable().FirstOrDefault(t => t.Language == twoLetterLanguage))
            {
                doc.Titles.Add(title);

                if (!string.IsNullOrEmpty(description.Value))
                {
                    doc.Descriptions.Add(description);
                }
            }
            doc.Files.Add(file);
        }

        private static void ProcessVaultDocumentTypes(DocumentsContext ctx, MFilesVault vault)
        {
            foreach (var cls in vault.GetDocumentTypes())
            {
                var docType = ctx.Values.FirstOrDefault(v => v.Value == cls);
                if (null != docType)
                {
                    continue;

                }
                docType = new TypeValue {Language = "en", Value = cls};

                ctx.Values.Add(docType);
                System.GC.Collect();
            }

        }
    }
}
