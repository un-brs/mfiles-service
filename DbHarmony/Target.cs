using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Conventions.MFiles.Models;
using HarmonyInterfaces;
using NLog;

namespace DbHarmony
{
    public class Target : ITarget
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly DocumentsContext _ctx;

        public Target()
        {
            _ctx = new DocumentsContext();
        }
        public bool Connect()
        {
            Logger.Info("Connect to database {0}", _ctx.Database.Connection.ConnectionString);
            _ctx.Database.CreateIfNotExists();
            _ctx.Database.Connection.Open();

            return _ctx.Database.Connection.State == ConnectionState.Open;
        }

        public ITargetDocument FindDocument(Guid guid)
        {
            return _ctx.MFilesDocuments.FirstOrDefault(d => d.Guid == guid);
        }

 
        public ITargetDocument FindMaster(string unNumber)
        {
            return _ctx.MFilesDocuments.FirstOrDefault(x => x.Document.UnNumber == unNumber);
        }

        public ITargetDocument CreateMaster(ISourceDocument sourceDoc)
        {
            var targetDoc = _ctx.MFilesDocuments.Create();
            targetDoc.Guid = sourceDoc.Guid;
            targetDoc.CreatedDate = sourceDoc.CreatedDate;
            targetDoc.ModifiedDate = sourceDoc.ModifiedDate;

            var masterDoc = _ctx.Documents.Create();
            masterDoc.MFilesDocument = targetDoc;
            masterDoc.UnNumber = sourceDoc.UnNumber;
            masterDoc.Vault = sourceDoc.Repository.Name;
            masterDoc.PublicationDate = targetDoc.CreatedDate;
            masterDoc.PublicationUpdateDate = targetDoc.ModifiedDate;

            var status = true;
            using (var trans = _ctx.Database.BeginTransaction())
            {
                try
                {
                    _ctx.MFilesDocuments.Add(targetDoc);
                    _ctx.SaveChanges();
                    _ctx.Documents.Add(masterDoc);
                    _ctx.SaveChanges();
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    Logger.Fatal("SQL exception {0}", ex.Message);
                    status = false;
                }
            }
            return status? targetDoc: null;
        }
    }
}
