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

        ITargetDocument FindDocument(ISourceDocument sourceDoc);
        ITargetDocument FindMaster(ISourceDocument sourceDoc);
        ITargetDocument CreateMaster(ISourceDocument sourceDoc);
        ITargetDocument UpdateMaster(ITargetDocument masterDoc, ISourceDocument sourceDoc);
        ITargetDocument CreateSlave(ITargetDocument master, ISourceDocument sourceDoc);
        ITargetDocument UpdateSlave(ITargetDocument masterDoc, ITargetDocument slaveDoc, ISourceDocument sourceDoc);

        void DeleteNotInList(IList<Guid> guids);

    }
}
