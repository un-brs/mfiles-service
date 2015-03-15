using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HarmonyInterfaces
{
    public interface ISource
    {
        bool Connect();
        IEnumerable<IRepository> GetRepositories();
        IEnumerable<ISourceDocument> GetDocuments(IRepository vault);


    }
}
