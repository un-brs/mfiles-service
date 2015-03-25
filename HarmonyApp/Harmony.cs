using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
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
        private readonly ISet<Guid> _processed = new HashSet<Guid>();
        private readonly bool _isDeleteUnprocessed;

        private readonly Timer _timer;
        private int _interval;

        public Harmony(ISource source, ITarget target, bool isDeleteUnprocessed, int interval)
        {
            _source = source;
            _target = target;
            _isDeleteUnprocessed = isDeleteUnprocessed;
            _interval = interval * 60 * 1000;
            _timer = new Timer(_interval) {AutoReset = true};
            _timer.Elapsed += ProcessOnTimer;
        }

        private void ProcessOnTimer(object source, ElapsedEventArgs args)
        {
            Process();
        }
        public void Process()
        {
            Logger.Info("Start harmonizing...");
            
            _timer.Enabled = false;
            try
            {
                var vaults = _source.GetRepositories().ToArray();
                Logger.Info("Process vaults: {0} ", StringUtils.Concatenate(vaults, (IRepository r) => r.Name, ','));

                foreach (var vault in vaults)
                {
                    HarmonizeVault(vault);
                }
                Logger.Info("Synchronization completed");

                if (_isDeleteUnprocessed)
                {
                    _target.DeleteNotInList(_processed);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorException("Main exception", ex);
            }

            _timer.Interval = _interval;
            _timer.Enabled = true;
        }

        private void HarmonizeVault(IRepository repository)
        {
            Logger.Info("Process vault {0}", repository.Name);
            foreach (var sourceDoc in _source.GetDocuments(repository))
            {
                if (!sourceDoc.CanBeSynchronized)
                {
                    Logger.Warn("Document {0}.{1} could not be synchronized", sourceDoc.File.Name, sourceDoc.File.Extension);
                    continue;
                }
                _processed.Add(sourceDoc.Guid);
                var targetDoc = _target.FindDocument(sourceDoc);
                if (targetDoc == null) {
                    _target.OnBeforeUpdateDocument();
                    var master = _target.FindMaster(sourceDoc);
                    if (master == null)
                    {
                        Logger.Info("Create master document {0}.{1} {2}", sourceDoc.File.Name, sourceDoc.File.Extension, sourceDoc.ModifiedDate.ToShortDateString());
                        master = _target.CreateMaster(sourceDoc);
                    }
                    if (master != null) {
                        if (master.Guid != sourceDoc.Guid)
                        {
                            Logger.Info("Create slave document {0}.{1}", sourceDoc.File.Name, sourceDoc.File.Extension);
                        }
                        _target.CreateSlave(master, sourceDoc);
                    }
                    _target.OnAfterDocument();
                } else if (targetDoc.ModifiedDate != sourceDoc.ModifiedDate)
                {
                    _target.OnBeforeUpdateDocument();

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

                    _target.OnAfterDocument();
                }
            }
            Logger.Info("Finished process vault {0}", repository.Name);
 
        }

        public void Start()
        {
            Logger.Info("Start service");
            var result = _source.Connect();
            if (!result) return;

            result = _target.Connect();
            if (!result) return;

            _timer.Interval = 5;
            _timer.Start();

        }
        public void Stop()
        {
            Logger.Info("Stop service");
            _timer.Stop();
        }

    }
}
