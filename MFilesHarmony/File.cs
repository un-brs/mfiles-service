using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyInterfaces;
using MFilesAPI;

namespace MFilesHarmony
{
    public class File : IFile
    {
        private ObjectFile _file;
        private ISourceDocument _doc;

        public File(ISourceDocument doc, ObjectFile file)
        {
            _file = file;
            _doc = doc;
        }
        public string Name
        {
            get { return _file.Title; }
        }

        public string Extension
        {
            get { return _file.Extension; }
        }


        public long Size
        {
            get { return _file.LogicalSize; }
        }

        public string GetUrl(string prefix=null)
        {
            if (_doc.Repository.Name == "Intranet")
            {
                return _doc.SourceUrl ?? "";
            }
            return string.Format("{0}{1}.{2}", prefix ?? "", Name, Extension);
        }
    }
}
