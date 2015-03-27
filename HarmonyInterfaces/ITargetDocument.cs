using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonyInterfaces
{
    public interface ITargetDocument
    {
        Guid Guid { get; }

        DateTime ModifiedDate { get; }
        DateTime CreatedDate { get; }

        string GetRepositoryName();
    }
}
