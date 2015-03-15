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

        ITargetDocument FindDocument(Guid guid);
        ITargetDocument FindMaster(string unNumber);
        ITargetDocument CreateMaster(ISourceDocument sourceDoc);

    }
}
