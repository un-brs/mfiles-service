using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using DbHarmony;
using HarmonyApp.Properties;
using HarmonyInterfaces;
using MFilesHarmony;
using SimpleInjector;
using Topshelf;
using TreatiesService;

namespace HarmonyApp
{
    internal class Program
    {
        static void PressKey()
        {
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
        private static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

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
            foreach (var vaultUrl in from string urlPair in Settings.Default.Urls select urlPair.Split('\t'))
            {
                downloadUrls[vaultUrl[0]] = vaultUrl[1];
            }
            container.Register<ICountries>(() => new CountriesClient(Settings.Default.TreatiesServiceUrl));
            container.Register<ITarget>(() => new Target(downloadUrls, container.GetInstance<ICountries>(),
                Settings.Default.DbReconnectAfter));
            container.Register<IHarmony>(() => new Harmony(
                container.GetInstance<ISource>(),
                container.GetInstance<ITarget>(),
                Settings.Default.IsDeleteUnprocessed,
                Settings.Default.Interval));

            var app = container.GetInstance<IHarmony>();
  

            HostFactory.Run(x =>                                 //1
            {
                x.Service<IHarmony>(s =>                        //2
                {
                    s.ConstructUsing(name => app);     //3
                    s.WhenStarted(tc => tc.Start());              //4
                    s.WhenStopped(tc => tc.Stop());               //5
                });
                x.RunAsLocalSystem();                            //6

                x.SetDescription("M-Files to database syncronization tool");        //7
                x.SetDisplayName("HarmonyApp");                       //8
                x.SetServiceName("HarmonyApp");                       //9
                x.AfterInstall(PressKey);
                x.AfterUninstall(PressKey);

            });   

        }
    }
}