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
            foreach (var sourceDoc in _source.GetDocuments(repository)) {
  
                var targetDoc = _target.FindDocument(sourceDoc);
                if (targetDoc == null) {
                    Logger.Info("Document {0}.{1}", sourceDoc.File.Name, sourceDoc.File.Extension);
                    var master = _target.FindMaster(sourceDoc) ?? ProcessMasterDocument(sourceDoc);
                    if (master != null) {
                        ProcessSlaveDoc(master, sourceDoc);
                    }
                } else {

                }

            }
        }

 
        private ITargetDocument ProcessMasterDocument(ISourceDocument doc)
        {
            return _target.CreateMaster(doc);
        }
        private ITargetDocument ProcessSlaveDoc(ITargetDocument masterDoc, ISourceDocument sourceDoc)
        {
            return _target.CreateSlave(masterDoc, sourceDoc);
        }
    }
}
