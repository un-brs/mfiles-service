using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbHarmony;
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
                new string[] { "Rotterdam", "Notexists"},
                "AM1"
             ));
            container.Register<ITarget, Target>();
            container.Register<IHarmony, Harmony>();

            var app = container.GetInstance<IHarmony>();
            app.Harmonize();

            Console.WriteLine();
            Console.WriteLine("Finished. Press any key to exit ...");
            Console.ReadKey();
        }
    }
}
