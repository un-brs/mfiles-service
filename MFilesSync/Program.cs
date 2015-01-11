using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Conventions.MFiles.Models;


namespace MFilesSync
{
    class Program
    {
        static void Main(string[] args)
        {
            var ctx = new DocumentsContext();
            ctx.Database.CreateIfNotExists();
            ctx.Database.Connection.Open();

            var testDocument = ctx.Documents.Create();
            testDocument.UnNumber = "Hello";
            testDocument.Vault = "My Vault";
            
            testDocument.CreatedDate = DateTime.Now;
            testDocument.ModifiedDate = DateTime.Now;

            var title = new TitleValue {Language = "en", Value = "Test"};


            var descr = new DescriptionValue {Language = "en", Value = "Test"};

            testDocument.Titles.Add(title);
            testDocument.Descriptions.Add(descr);
            
            ctx.Documents.Add(testDocument);
            try
            {
                ctx.SaveChanges();
            }
            catch (Exception)
            {
                foreach (var e in ctx.GetValidationErrors())
                {
                    foreach (var e1 in e.ValidationErrors)
                    {


                        Console.WriteLine(e1.PropertyName+" "+e1.ErrorMessage);
                    }
                }
            }


        }
    }
}
