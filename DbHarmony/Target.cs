using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Conventions.MFiles.Models;
using HarmonyInterfaces;
using MimeSharp;
using NLog;
using Utils;

namespace DbHarmony
{
    public class Target : ITarget
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Mime Mime = new Mime();
        private readonly ICountries _countries;
        private  DocumentsContext _ctx;
        private readonly IDictionary<string, string> _repositoryUrls;
        private readonly int _reconnectAfter;
        private int _nDocuments = 0;

        public Target(IDictionary<string, string> repositoryUrls, ICountries countries, int reconnectAfter)
        {
            
            _repositoryUrls = repositoryUrls;
            _countries = countries;
            _reconnectAfter = reconnectAfter;
        }

        public bool Connect()
        {
            if (_ctx != null && (_ctx.Database.Connection.State == ConnectionState.Open))
            {
                _ctx.Database.Connection.Close();
            }
            _ctx = new DocumentsContext();
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
            var unNumber = string.IsNullOrEmpty(doc.UnNumber) ? doc.Name : doc.UnNumber;
            return _ctx.MFilesDocuments.FirstOrDefault(x => x.Document.UnNumber == unNumber);
        }

        public ITargetDocument CreateMaster(ISourceDocument sourceDoc)
        {
            var targetDoc = _ctx.MFilesDocuments.Create();
            _ctx.MFilesDocuments.Add(targetDoc);

            var masterDoc = new Document();
            masterDoc.MFilesDocument = targetDoc;
            _ctx.Documents.Add(masterDoc);

            return UpdateMaster(targetDoc, sourceDoc);
        }

        public ITargetDocument UpdateMaster(ITargetDocument imasterDoc, ISourceDocument sourceDoc)
        {
            var targetDoc = imasterDoc as MFilesDocument;
            
            targetDoc.Guid = sourceDoc.Guid;
            targetDoc.CreatedDate = sourceDoc.CreatedDate;
            targetDoc.ModifiedDate = sourceDoc.ModifiedDate;

            var masterDoc = targetDoc.Document;
            masterDoc.MFilesDocument = targetDoc;
            masterDoc.UnNumber = string.IsNullOrEmpty(sourceDoc.UnNumber) ? sourceDoc.Name : sourceDoc.UnNumber;
            masterDoc.Vault = sourceDoc.Repository.Name;
            masterDoc.Author = sourceDoc.Author;
            masterDoc.CountryFull = sourceDoc.Country;
            masterDoc.Country = _countries.GetCountryIsoCode2(masterDoc.CountryFull);
            masterDoc.Copyright = sourceDoc.Copyright;
            var period = sourceDoc.GetPeriod();
            if (period != null)
            {
                masterDoc.PeriodStartDate = period.Item1;
                masterDoc.PeriodEndDate = period.Item2;
            }
            masterDoc.PublicationDate = sourceDoc.PublicationDate;

            ProcessDocumentTypes(masterDoc, sourceDoc);
            ProcessMeetings(masterDoc, sourceDoc);
            ProcessMeetingTypes(masterDoc, sourceDoc);
            ProcessChemicals(masterDoc, sourceDoc);
            ProcessPrograms(masterDoc, sourceDoc);
            ProcessTerms(masterDoc, sourceDoc);
            ProcessTags(masterDoc, sourceDoc);

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
                    Logger.ErrorException("SQL exception {0} {1}", ex);
                    status = false;
                }
            }
            return status ? targetDoc : null;
        }

        public ITargetDocument CreateSlave(ITargetDocument master, ISourceDocument sourceDoc)
        {
            MFilesDocument targetDoc = master as MFilesDocument;
            if (sourceDoc.Guid != master.Guid)
            {
                targetDoc = _ctx.MFilesDocuments.Create();
            }
            return UpdateSlave(master, targetDoc, sourceDoc);
        }

        public ITargetDocument UpdateSlave(ITargetDocument imasterDoc, ITargetDocument itargetDoc, ISourceDocument sourceDoc)
        {
            _nDocuments++;
            var masterDoc = imasterDoc as MFilesDocument;
            var targetDoc = itargetDoc as MFilesDocument;

            if (sourceDoc.Guid != masterDoc.Guid)
            {
                targetDoc.Guid = sourceDoc.Guid;
                targetDoc.ModifiedDate = sourceDoc.ModifiedDate;
                targetDoc.CreatedDate = sourceDoc.CreatedDate;
            }
            else
            {
                targetDoc = masterDoc;
            }

            var doc = masterDoc.Document;
            Debug.Assert(doc != null);

            var languageCode = CultureUtils.GetLangTwoLetterCode(sourceDoc.Language);

            var title = doc.Titles.FirstOrDefault(t => t.Language == languageCode && t.Document == doc);
            if (title == null || title.MFilesDocument == targetDoc)
            {
                if (title == null)
                {
                    title = new Title();
                    title.MFilesDocument = targetDoc;
                    doc.Titles.Add(title);
                }

                title.Document = doc;
                title.Language = languageCode;
                title.LanguageFull = sourceDoc.Language;
                title.MFilesDocument = targetDoc;
                title.Value = sourceDoc.Title;
            }

            var descirpiton = doc.Descriptions.FirstOrDefault(t => t.Language == languageCode && t.Document == doc);
            if (descirpiton == null || descirpiton.MFilesDocument == targetDoc)
            {
                if (descirpiton == null)
                {
                    descirpiton = new Description();
                    doc.Descriptions.Add(descirpiton);
                }
                descirpiton.Document = doc;
                descirpiton.Language = languageCode;
                descirpiton.LanguageFull = sourceDoc.Language;
                descirpiton.MFilesDocument = targetDoc;
                descirpiton.Value = sourceDoc.Title;
            }


            var file = sourceDoc.File;
            var repositoryUrl = "";

            if (_repositoryUrls.ContainsKey(sourceDoc.Repository.Name))
            {
                repositoryUrl = _repositoryUrls[sourceDoc.Repository.Name];
            }
            var targetFile = doc.Files.FirstOrDefault(f => f.MFilesDocument == targetDoc);
            if (targetFile == null)
            {
                targetFile = new File();
                doc.Files.Add(targetFile);
            }
            targetFile.Document = doc;
            targetFile.MFilesDocument = targetDoc;
            targetFile.Language = languageCode;
            targetFile.LanguageFull = sourceDoc.Language;
            targetFile.Name = file.Name;
            targetFile.Extension = file.Extension;
            targetFile.Size = file.Size;
            targetFile.MimeType = Mime.Lookup(file.Name + "." + file.Extension);
            targetFile.Url = file.GetUrl(repositoryUrl);

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
                    Logger.ErrorException("", ex);
                }
            }

           

            return status ? targetDoc : null;
        }

        public void OnBeforeUpdateDocument()
        {
            _nDocuments++;
        }

        public void OnAfterDocument()
        {
            if ((_nDocuments % _reconnectAfter) == 0) {
                Connect();
            }
        }

        public void DeleteDocument(ITargetDocument targetDocument)
        {
            if (targetDocument == null)
            {
                return;
            }
            var doc = targetDocument as MFilesDocument;
            Debug.Assert(doc != null);
            if (doc. Title != null) {
                Logger.Info("Delete document '{0}'", doc.Title.Value);
            } else {
                Logger.Info("Delete document {0}", doc.Guid);
            }

            var documents = from x in _ctx.Documents where x.MFilesDocument.Guid == doc.Guid select x;
            _ctx.Documents.RemoveRange(documents.ToList());

            var titles = from x in _ctx.Titles where x.MFilesDocument.Guid == doc.Guid select x;
            _ctx.Titles.RemoveRange(titles.ToList());

            var descriptions = (from x in _ctx.Descriptions where x.MFilesDocument.Guid == doc.Guid select x).ToList();
            _ctx.Descriptions.RemoveRange(descriptions.ToList());

            var files = (from x in _ctx.Files where x.MFilesDocument.Guid == doc.Guid select x).ToList();
            _ctx.Files.RemoveRange(files.ToList());

            _ctx.MFilesDocuments.Remove(doc);

            using (var trans = _ctx.Database.BeginTransaction()) {
                try {
                    _ctx.SaveChanges();
                    trans.Commit();
                } catch (Exception ex) {
                    Logger.ErrorException("Delete document", ex);
                }
            }
        }

        public void DeleteNotInList(ICollection<Guid> guids)
        {
            Logger.Info("Find documents for removing...");
            //_ctx.Database.CommandTimeout= 600;
            var docs = (from mfdoc in _ctx.MFilesDocuments where guids.Contains(mfdoc.Guid) select mfdoc);
            var toDelete = _ctx.MFilesDocuments.Except(docs).ToList();
            //_ctx.Database.CommandTimeout = 60;
            Logger.Info("Number of files to remove = {0}", toDelete.Count);
            foreach (var doc in toDelete)
            {
                DeleteDocument(doc);
            }
        }

        private void ProcessTerms(Document masterDoc, ISourceDocument sourceDoc)
        {
            masterDoc.Terms.Clear();
            foreach (var source in sourceDoc.Terms)
            {
                var values = (source.IsStringProperty
                    ? source.Value.Split(',').Select(x => x.Trim()).ToArray()
                    : new[] {source.Value});
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
                        target = new TermValue
                        {
                            ListPropertyId = listPropertyId,
                            Value = value.Trim()
                        };
                        _ctx.Values.Add(target);
                    }
                    masterDoc.Terms.Add(target);
                }
            }
        }

        private void ProcessTags(Document masterDoc, ISourceDocument sourceDoc)
        {
            masterDoc.Tags.Clear();
            foreach (var source in sourceDoc.Tags) {
                var values = (source.IsStringProperty
                    ? source.Value.Split(',').Select(x => x.Trim().ToLower()).Where(x=>!String.IsNullOrEmpty(x)).ToArray()
                    : new[] { source.Value.ToLower() });
                foreach (var value in values) {
                    var target = _ctx.Values.OfType<TagValue>().FirstOrDefault(t => t.Value == value);
                    if (target == null) {
                        var listPropertyId = Guid.NewGuid();
                        if (!source.IsStringProperty && source.Guid.HasValue) {
                            listPropertyId = source.Guid.Value;
                        }
                        target = new TagValue() {
                            ListPropertyId = listPropertyId,
                            Value = value.Trim()
                        };
                        _ctx.Values.Add(target);
                    }
                    masterDoc.Tags.Add(target);
                }
            }
        }

        private void ProcessPrograms(Document masterDoc, ISourceDocument sourceDoc)
        {
            masterDoc.Programs.Clear();
            foreach (var source in sourceDoc.Programs)
            {
                var target = _ctx.Values.OfType<ProgramValue>().FirstOrDefault(t => t.Value == source.Value);
                if (target == null)
                {
                    target = new ProgramValue
                    {
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
            masterDoc.Chemicals.Clear();
            foreach (var source in sourceDoc.Chemicals)
            {
                var target = _ctx.Values.OfType<ChemicalValue>().FirstOrDefault(t => t.Value == source.Value);
                if (target == null)
                {
                    target = new ChemicalValue
                    {
                        ListPropertyId = source.Guid ?? Guid.NewGuid(),
                        Value = source.Value
                    };
                    _ctx.Values.Add(target);
                }
                masterDoc.Chemicals.Add(target);
            }
        }

        private void ProcessMeetings(Document masterDoc, ISourceDocument sourceDoc)
        {
            masterDoc.Meetings.Clear();
            foreach (var source in sourceDoc.Meetings) {
                var target = _ctx.Values.OfType<MeetingValue>().FirstOrDefault(t => t.Value == source.Value);
                if (target == null) {
                    target = new MeetingValue {
                        ListPropertyId = source.Guid ?? Guid.NewGuid(),
                        Value = source.Value
                    };
                    _ctx.Values.Add(target);
                }
                masterDoc.Meetings.Add(target);
            }
        }

        private void ProcessMeetingTypes(Document masterDoc, ISourceDocument sourceDoc)
        {
            masterDoc.MeetingsTypes.Clear();
            foreach (var source in sourceDoc.MeetingsTypes) {
                var target = _ctx.Values.OfType<MeetingTypeValue>().FirstOrDefault(t => t.Value == source.Value);
                if (target == null) {
                    target = new MeetingTypeValue {
                        ListPropertyId = source.Guid ?? Guid.NewGuid(),
                        Value = source.Value
                    };
                    _ctx.Values.Add(target);
                }
                masterDoc.MeetingsTypes.Add(target);
            }
        }

        private void ProcessDocumentTypes(Document masterDoc, ISourceDocument sourceDoc)
        {
            masterDoc.Types.Clear();
            foreach (var type in sourceDoc.Types)
            {
                var targetType = _ctx.Values.OfType<TypeValue>().FirstOrDefault(t => t.Value == type.Value);
                if (targetType == null)
                {
                    targetType = new TypeValue
                    {
                        ListPropertyId = Guid.NewGuid(),
                        Value = type.Value
                    };
                    _ctx.Values.Add(targetType);
                }
                masterDoc.Types.Add(targetType);
            }
        }
    }
}