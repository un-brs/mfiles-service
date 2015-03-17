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
        string Name { get; }
        string Description { get; }
        string Title { get; }
        string Language { get; }
        string Author { get; }
        string Country { get; }
        string Copyright { get; }
        DateTime PublicationDate { get; }

        Tuple<DateTime, DateTime> GetPeriod();

 
        IList<IListProperty> Types { get; }
        IList<IListProperty> Chemicals { get; }
        IList<IListProperty> Programs { get; }
        IList<IListProperty> Terms { get; }

        IListProperty Meeting { get; }
        IListProperty MeetingType { get; }
        IRepository Repository { get; }

        DateTime ModifiedDate { get; }
        DateTime CreatedDate { get; }

        IFile File { get; }

    }
}
