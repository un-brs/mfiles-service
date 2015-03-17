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

        public File(ObjectFile file)
        {
            _file = file;
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
    }
}
