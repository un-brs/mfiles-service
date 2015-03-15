using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyInterfaces;
using NLog;
using Utils;


namespace HarmonyApp
{
    public class Harmony : IHarmony
    {
        private readonly ISource _source;
        private readonly ITarget _target;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Harmony(ISource source, ITarget target)
        {
            _source = source;
            _target = target;
        }
        public void Harmonize()
        {
            Logger.Info("Start harmonizing...");

            var result = _source.Connect();
            if (!result) return;

            result = _target.Connect();
            if (!result) return;

            var vaults = _source.GetRepositories().ToArray();
            Logger.Info("Process vaults {0} ", StringUtils.Concatenate(vaults, (IRepository r) => r.Name, ','));

            foreach (var vault in vaults) {
                HarmonizeVault(vault);
            }
        }

        private void HarmonizeVault(IRepository repository)
        {
            Logger.Info("Process vault {0}", repository.Name);
            string newUnNumber = null;
            IList<ISourceDocument> newDocuments = new List<ISourceDocument>();
            foreach (var sourceDoc in _source.GetDocuments(repository)) {
                Logger.Info("Document {0}", sourceDoc.Guid);
                var currentUnNumber = sourceDoc.UnNumber;
                if (currentUnNumber != newUnNumber) {
                    ProcessNewDocuments(newDocuments);
                    newDocuments.Clear();
                    newUnNumber = currentUnNumber;
                }
                var targetDoc = _target.FindDocument(sourceDoc.Guid);
                if (targetDoc == null)
                {
                    var master = _target.FindMaster(currentUnNumber);
                    if (master == null)
                    {
                        newUnNumber = sourceDoc.UnNumber;
                        newDocuments.Add(sourceDoc);
                    }
                    else
                    {
                        ProcessSlaveDoc(master, sourceDoc);
                    }
                }
                else
                {
                    
                }

            }
        }

        private void ProcessNewDocuments(IList<ISourceDocument> sourceDocuments)
        {
            if (sourceDocuments.Count > 0) {
                var masterDoc = ProcessMasterDocument(sourceDocuments[0]);
                if (masterDoc != null) {
                    foreach (var sourceDoc in sourceDocuments) {
                        ProcessSlaveDoc(masterDoc, sourceDoc);
                    }
                }
            }
        }

        private ITargetDocument ProcessMasterDocument(ISourceDocument doc)
        {
            Logger.Info("Process master document {0}", doc.UnNumber);
            return _target.CreateMaster(doc);
        }
        private void ProcessSlaveDoc(ITargetDocument masterDoc, ISourceDocument sourceDoc)
        {
            Logger.Info("Process slave document {0}", sourceDoc.UnNumber);
        }
    }
}
