using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonyInterfaces
{
    public interface ISourceDocument
    {
        Guid Guid { get; }
        string UnNumber { get; }
        IRepository Repository { get; }

        DateTime ModifiedDate { get; }
        DateTime CreatedDate { get; }
    }
}
