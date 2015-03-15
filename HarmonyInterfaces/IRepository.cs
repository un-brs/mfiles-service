using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonyInterfaces
{
    public interface IRepository
    {
        string Name { get; }
        Guid Guid { get; }
    }
}
