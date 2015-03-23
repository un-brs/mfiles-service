using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HarmonyInterfaces
{
    public interface IHarmony
    {
        void Process();
        void Start();
        void Stop();
    }
}
