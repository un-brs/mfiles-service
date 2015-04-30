using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonyInterfaces
{
    public interface ITarget
    {
        bool Connect();

        void OnBeforeUpdateDocument();
        void OnAfterDocument();

        ITargetDocument FindDocument(ISourceDocument sourceDoc);
        ITargetDocument FindMaster(ISourceDocument sourceDoc);
        ITargetDocument FindMasterById(Guid guid);
        ITargetDocument CreateMaster(ISourceDocument sourceDoc);
        ITargetDocument UpdateMaster(ITargetDocument masterDoc, ISourceDocument sourceDoc);
        ITargetDocument CreateSlave(ITargetDocument master, ISourceDocument sourceDoc);
        ITargetDocument UpdateSlave(ITargetDocument masterDoc, ITargetDocument slaveDoc, ISourceDocument sourceDoc);

        void DeleteDocument(ITargetDocument targetDocument);
        void DeleteNotInList(ICollection<Guid> guids);

    }
}
