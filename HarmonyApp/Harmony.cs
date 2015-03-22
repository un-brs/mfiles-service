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
        private readonly List<Guid> _processed = new List<Guid>();
        private readonly bool _isDeleteUnprocessed;

        public Harmony(ISource source, ITarget target, bool isDeleteUnprocessed = true)
        {
            _source = source;
            _target = target;
            _isDeleteUnprocessed = isDeleteUnprocessed;
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
                if (!sourceDoc.CanBeSynchronized)
                {
                    Logger.Warn("Document {0}.{1} could not be synchronized", sourceDoc.File.Name, sourceDoc.File.Extension);
                    continue;
                }
                _processed.Add(sourceDoc.Guid);
                var targetDoc = _target.FindDocument(sourceDoc);
                if (targetDoc == null) {
                    Logger.Info("Document {0}.{1} {2}", sourceDoc.File.Name, sourceDoc.File.Extension, sourceDoc.ModifiedDate.ToShortDateString());
                    var master = _target.FindMaster(sourceDoc);
                    if (master == null)
                    {
                        Logger.Info("Create master document {0}.{1}", sourceDoc.File.Name, sourceDoc.File.Extension);
                        master = _target.CreateMaster(sourceDoc);
                    }
                    if (master != null) {
                        if (master.Guid != sourceDoc.Guid)
                        {
                            Logger.Info("Create slave document {0}.{1}", sourceDoc.File.Name, sourceDoc.File.Extension);
                        }
                        _target.CreateSlave(master, sourceDoc);
                    }
                } else if (targetDoc.ModifiedDate != sourceDoc.ModifiedDate)
                {
                    var master = _target.FindMaster(sourceDoc);
                    if (master.Guid == targetDoc.Guid)
                    {
                        Logger.Info("Update master document {0}.{1}", sourceDoc.File.Name, sourceDoc.File.Extension);
                        _target.UpdateMaster(master, sourceDoc);
                    }
                    else
                    {
                        Logger.Info("Update slave document {0}.{1}", sourceDoc.File.Name, sourceDoc.File.Extension);
                    }
                    _target.UpdateSlave(master, targetDoc, sourceDoc);
                }

            }
            if (_isDeleteUnprocessed)
            {
                _target.DeleteNotInList(_processed);
            }
        }

    }
}
