using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonyInterfaces
{
    public interface IListProperty
    {
        Guid? Guid { get; }
        string Value { get; }
        bool IsStringProperty { get;  }
    }
}
