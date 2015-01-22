using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TermsCrmJson
{
    class Program
    {
        static void Main(string[] args)
        {

            var result = "{";
            var sctx = new TermsCrm.TermsServiceReference.asbMeetingEntities(new Uri(Settings.Default.TermsServiceUri));

            var chemicalQuery = from term in sctx.Terms where term.ParentTermNames.Contains("Chemicals") select term;
            result+= "\"chemicals\": " + JsonConvert.SerializeObject(chemicalQuery, Formatting.Indented);
            

            var programmesQuery = from term in sctx.Terms where term.ParentTermNames.Contains("Programmes") select term;
            result += ", \"programmes\": " + JsonConvert.SerializeObject(programmesQuery, Formatting.Indented);

            var termsQuery = from term in sctx.Terms where term.ParentTermNames.Contains("Scientific and Technical Publications Terms") select term;
            result += ", \"terms\": " + JsonConvert.SerializeObject(termsQuery, Formatting.Indented);

            var meetingsQuery = from term in sctx.Terms where term.ParentTermNames.Contains("Meetings") select term;
            result += ", \"meetings\":" + JsonConvert.SerializeObject(meetingsQuery, Formatting.Indented);
            result += "}";
            Console.WriteLine(result);
            File.WriteAllText(@"terms.json", result);
            Console.WriteLine("Press any key ...");
            Console.ReadKey();

        }
    }
}
