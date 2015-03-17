using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Markup;
using Conventions.MFiles.Models;
using HarmonyInterfaces;
using NLog;
using Utils;
using MimeSharp;

namespace DbHarmony
{
    public class Target : ITarget
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly DocumentsContext _ctx;
        private static readonly Mime Mime = new Mime();
        private readonly IDictionary<string, string> _repositoryUrls;

        public Target(IDictionary<string, string> repositoryUrls)
        {
            _ctx = new DocumentsContext();
            _repositoryUrls = repositoryUrls;
        }
        public bool Connect()
        {
            Logger.Info("Connect to database {0}", _ctx.Database.Connection.ConnectionString);
             _ctx.Database.CreateIfNotExists();
            _ctx.Database.Connection.Open();

            return _ctx.Database.Connection.State == ConnectionState.Open;
        }

        public ITargetDocument FindDocument(ISourceDocument doc)
        {
            return _ctx.MFilesDocuments.FirstOrDefault(d => d.Guid == doc.Guid);
        }

 
        public ITargetDocument FindMaster(ISourceDocument doc)
        {
            string unNumber = string.IsNullOrEmpty(doc.UnNumber) ? doc.Name : doc.UnNumber;
            return _ctx.MFilesDocuments.FirstOrDefault(x => x.Document.UnNumber == unNumber);
        }

        public ITargetDocument CreateMaster(ISourceDocument sourceDoc)
        {
            var targetDoc = _ctx.MFilesDocuments.Create();
            targetDoc.Guid = sourceDoc.Guid;
            targetDoc.CreatedDate = sourceDoc.CreatedDate;
            targetDoc.ModifiedDate = sourceDoc.ModifiedDate;
            _ctx.MFilesDocuments.Add(targetDoc);

            var masterDoc = _ctx.Documents.Create();
            masterDoc.MFilesDocument = targetDoc;
            masterDoc.UnNumber = string.IsNullOrEmpty(sourceDoc.UnNumber) ? sourceDoc.Name : sourceDoc.UnNumber;
            masterDoc.Vault = sourceDoc.Repository.Name;
            masterDoc.Author = sourceDoc.Author;
            masterDoc.CountryFull = sourceDoc.Country;
            masterDoc.Country = CultureUtils.GetCountryTwoLetterCode(masterDoc.CountryFull);
            masterDoc.Copyright = sourceDoc.Copyright;
            var period = sourceDoc.GetPeriod();
            if (period != null) {
                masterDoc.PeriodStartDate = period.Item1;
                masterDoc.PeriodEndDate = period.Item2;
            }
            masterDoc.PublicationDate = sourceDoc.PublicationDate;

            _ctx.Documents.Add(masterDoc);
            ProcessDocumentTypes(masterDoc, sourceDoc);
            ProcessMeeting(masterDoc, sourceDoc);
            ProcessMeetingType(masterDoc, sourceDoc);
            ProcessChemicals(masterDoc, sourceDoc);
            ProcessPrograms(masterDoc, sourceDoc);
            ProcessTerms(masterDoc, sourceDoc);

            var status = true;
            using (var trans = _ctx.Database.BeginTransaction())
            {
                try
                {
                    _ctx.SaveChanges();
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    Logger.Fatal("SQL exception {0} {1}", ex.Message, ex.InnerException.InnerException.Message);
                    status = false;
                }
            }
            return status? targetDoc: null;
        }

        private void ProcessTerms(Document masterDoc, ISourceDocument sourceDoc)
        {
            foreach (var source in sourceDoc.Terms) {
                 string[] values = (source.IsStringProperty
                          ?source.Value.Split(',').Select(x => x.Trim()).ToArray() 
                          : new string[]{source.Value});
                foreach (var value in values)
                {
                    var target = _ctx.Values.OfType<TermValue>().FirstOrDefault(t => t.Value == value);
                    if (target == null)
                    {
                        var listPropertyId = Guid.NewGuid();
                        if (!source.IsStringProperty && source.Guid.HasValue)
                        {
                            listPropertyId = source.Guid.Value;
                        }
                        target = new TermValue() {
                            ListPropertyId = listPropertyId,
                            Value = source.Value.Trim()
                        };
                        _ctx.Values.Add(target);
                    }
                    masterDoc.Terms.Add(target);    
                }
                
            }
        }

        private void ProcessPrograms(Document masterDoc, ISourceDocument sourceDoc)
        {
            foreach (var source in sourceDoc.Programs) {
                var target = _ctx.Values.OfType<ProgramValue>().FirstOrDefault(t => t.Value == source.Value);
                if (target == null) {
                    target = new ProgramValue() {
                        ListPropertyId = source.Guid ?? Guid.NewGuid(),
                        Value = source.Value
                    };
                    _ctx.Values.Add(target);
                }
                masterDoc.Programs.Add(target);
            }
        }

        private void ProcessChemicals(Document masterDoc, ISourceDocument sourceDoc)
        {
            foreach (var source in sourceDoc.Chemicals) {
                var target = _ctx.Values.OfType<ChemicalValue>().FirstOrDefault(t => t.Value == source.Value);
                if (target == null) {
                    target = new ChemicalValue() {
                        ListPropertyId = source.Guid ?? Guid.NewGuid(),
                        Value = source.Value
                    };
                    _ctx.Values.Add(target);
                }
                masterDoc.Chemicals.Add(target);
            }
        }

        private void ProcessMeetingType(Document masterDoc, ISourceDocument sourceDoc)
        {
            var item = sourceDoc.MeetingType;
            if (item == null) return;
            var targetValue = _ctx.Values.OfType<MeetingTypeValue>().FirstOrDefault(t => t.Value == item.Value);
            if (targetValue == null) {
                targetValue = new MeetingTypeValue() {
                    ListPropertyId = item.Guid ?? Guid.NewGuid(),
                    Value = item.Value
                };
                _ctx.Values.Add(targetValue);
            }
            masterDoc.MeetingType = targetValue;
        }

        private void ProcessMeeting(Document masterDoc, ISourceDocument sourceDoc)
        {
            var meeting = sourceDoc.Meeting;
            if (meeting == null) return;
            var targetValue = _ctx.Values.OfType<MeetingValue>().FirstOrDefault(t => t.Value == meeting.Value);
            if (targetValue == null)
            {
                targetValue = new MeetingValue()
                {
                    ListPropertyId = meeting.Guid??Guid.NewGuid(),
                    Value=meeting.Value
                };
                _ctx.Values.Add(targetValue);
            }
            masterDoc.Meeting = targetValue;
        }

        private void ProcessDocumentTypes(Document masterDoc, ISourceDocument sourceDoc)
        {
            foreach (var type in sourceDoc.Types)
            {
                var targetType = _ctx.Values.OfType<TypeValue>().FirstOrDefault(t => t.Value == type.Value);
                if (targetType == null)
                {
                    targetType = new TypeValue()
                    {
                        ListPropertyId = Guid.NewGuid(),
                        Value = type.Value
                    };
                    _ctx.Values.Add(targetType);
                }
                masterDoc.Types.Add(targetType);
            }
        }

        public ITargetDocument CreateSlave(ITargetDocument master, ISourceDocument sourceDoc)
        {
            MFilesDocument targetDocument = null;
            if (sourceDoc.Guid != master.Guid)
            {
                targetDocument = _ctx.MFilesDocuments.Create();
                targetDocument.Guid = sourceDoc.Guid;
                targetDocument.ModifiedDate = sourceDoc.ModifiedDate;
                targetDocument.CreatedDate = sourceDoc.CreatedDate;
                _ctx.MFilesDocuments.Add(targetDocument);
            }
            else
            {
                targetDocument = master as MFilesDocument;
            }

            var masterDoc = master as MFilesDocument;
            Debug.Assert(masterDoc != null);
            
            var doc = masterDoc.Document;
            Debug.Assert(doc != null);

            var languageCode = CultureUtils.GetLangTwoLetterCode(sourceDoc.Language);
            doc.Titles.Add( new Title()
            {
                Document = doc,
                Language = languageCode,
                LanguageFull = sourceDoc.Language,
                MFilesDocument = targetDocument,
                Value = sourceDoc.Title
            });

            doc.Descriptions.Add(new Description()
            {
                Document = doc,
                Language = languageCode,
                LanguageFull = sourceDoc.Language,
                MFilesDocument = targetDocument,
                Value = sourceDoc.Description
            });

            var file = sourceDoc.File;
            var repositoryUrl = "";
            if (_repositoryUrls.ContainsKey(sourceDoc.Repository.Name))
            {
                repositoryUrl = _repositoryUrls[sourceDoc.Repository.Name];
            }
            doc.Files.Add(new File()
            {
                Document = doc,
                MFilesDocument = targetDocument,
                Language = languageCode,
                LanguageFull = sourceDoc.Language,
                Name = file.Name,
                Extension = file.Extension,
                Size = file.Size,
                MimeType =  Mime.Lookup(file.Name + "." + file.Extension),
                Url = string.Format("{0}{1}.{2}", repositoryUrl, file.Name, file.Extension)
            });

            var status = true;
            using (var trans = _ctx.Database.BeginTransaction())
            {
                try
                {
                    _ctx.SaveChanges();
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    status = false;
                    Logger.Fatal(ex.Message+" " + ex.InnerException);
                }
            }

            return status ? targetDocument : null;
        }
    }
}
