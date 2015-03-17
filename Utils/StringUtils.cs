using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public  class StringUtils
    {
        public delegate string Indexer<T>(T obj);
        public static string Concatenate<T>(IEnumerable<T> collection, Indexer<T> indexer, char separator)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T t in collection) sb.Append(indexer(t)).Append(separator);
            if (sb.Length > 0)
            {
                return sb.Remove(sb.Length - 1, 1).ToString();
            }
            return "";
        }
    }
}
