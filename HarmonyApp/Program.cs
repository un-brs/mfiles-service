using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbHarmony;
using HarmonyApp.Properties;
using HarmonyInterfaces;
using MFilesHarmony;
using SimpleInjector;

namespace HarmonyApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new Container();
            container.Register<ISource>(() => new Source(
                ConfigurationManager.AppSettings["MFilesUser"],
                ConfigurationManager.AppSettings["MFilesPassword"],
                ConfigurationManager.AppSettings["MFilesHost"],
                Settings.Default.Vaults.Cast<string>().ToArray(),
                Settings.Default.View,
                Settings.Default.StartDate
             ));
            var downloadUrls = new Dictionary<string, string>();
            foreach (var urlPair in Settings.Default.Urls)
            {
                var vaultUrl = urlPair.Split('\t');
                downloadUrls[vaultUrl[0]] = vaultUrl[1];
            }
            container.Register<ITarget>(() => new Target(downloadUrls));
            container.Register<IHarmony, Harmony>();

            var app = container.GetInstance<IHarmony>();
            app.Harmonize();

            Console.WriteLine();
            Console.WriteLine("Finished. Press any key to exit ...");
            Console.ReadKey();
        }
    }
}
