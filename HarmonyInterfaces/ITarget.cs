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
        ITargetDocument CreateSlave(ITargetDocument master, ISourceDocument sourceDoc);

    }
}
