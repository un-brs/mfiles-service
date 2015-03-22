using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using HarmonyInterfaces;

// ReSharper disable once CheckNamespace

namespace Conventions.MFiles.Models
{
    public class DocumentsContext : DbContext
    {
        public DocumentsContext() : base("DocumentsContext")
        {
        }

        public DbSet<Document> Documents { get; set; }
        public DbSet<ListProperty> Values { get; set; }
        public DbSet<Title> Titles { get; set; }
        public DbSet<Description> Descriptions { get; set; }
        public DbSet<File> Files { get; set; }

        public DbSet<MFilesDocument> MFilesDocuments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
           //modelBuilder.Entity<SingleProperty>()
           //    .Map<Title>(m => m.Requires("Discriminator").HasValue("Title"))
           //    .Map<Description>(m => m.Requires("Discriminator").HasValue("Description"));

            //modelBuilder.Entity<MFilesDocument>().HasOptional(d => d.Title).WithRequired(t => t.MFilesDocument);
            //modelBuilder.Entity<MFilesDocument>().HasOptional(d => d.Description).WithRequired(t => t.MFilesDocument);
            //modelBuilder.Entity<MFilesDocument>()
        }
    }

    public class MFilesDocument : ITargetDocument
    {
        [Key]
        public Guid Guid { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateTime ModifiedDate { get; set; }

        public virtual Document Document { get; set; }

        public virtual Title Title { get; set; } 
        public virtual Description Description { get; set; }

    
    }

    public class Document
    {
        public Document()
        {
            // ReSharper disable  DoNotCallOverridableMethodsInConstructor
            Titles = new HashSet<Title>();
            Descriptions = new HashSet<Description>();
            Chemicals = new HashSet<ChemicalValue>();
            Terms = new HashSet<TermValue>();
            Tags = new HashSet<TagValue>();
            Programs = new HashSet<ProgramValue>();
            Types = new HashSet<TypeValue>();
            Meetings = new HashSet<MeetingValue>();
            MeetingsTypes = new HashSet<MeetingTypeValue>();
            Files = new HashSet<File>();
        }

        [Key, ForeignKey("MFilesDocument")]
        public Guid DocumentId { get; set; }

        [Required]
        [StringLength(255)]
        [DefaultValue("")]
        public String Vault { get; set; }

        [Required]
        [StringLength(255)]
        [Index(IsUnique = true)]
        public String UnNumber { get; set; }

        [DefaultValue("")]
        public String Copyright { get; set; }

        [DefaultValue("")]
        public String Author { get; set; }

        [StringLength(3)]
        [DefaultValue("")]
        public String Country { get; set; }
        
        [StringLength(255)]
        [DefaultValue("")]
        public String CountryFull { get; set; }


        public DateTime PublicationDate { get; set; }


        public DateTime? PeriodStartDate { get; set; }

        public DateTime? PeriodEndDate { get; set; }


        public virtual ICollection<Title> Titles { get; set; }
        public virtual ICollection<Description> Descriptions { get; set; }
        public virtual ICollection<ChemicalValue> Chemicals { get; set; }
        public virtual ICollection<TermValue> Terms { get; set; }
        public virtual ICollection<TagValue> Tags { get; set; }
        public virtual ICollection<ProgramValue> Programs { get; set; }
        public virtual ICollection<TypeValue> Types { get; set; }
        public virtual ICollection<MeetingValue> Meetings { get; set; }
        public virtual ICollection<MeetingTypeValue> MeetingsTypes { get; set; }

        public virtual ICollection<File> Files { get; set; }

     
        public virtual MFilesDocument MFilesDocument { get; set; }
    }


    public class File
    {
        [Key, ForeignKey("MFilesDocument")]
        public Guid FileId { get; set; }

        [Required]
        public Document Document { get; set; }

        [Required]
        [StringLength(3)]
        public String Language { get; set; }
        [Required]
        [StringLength(255)]
        public String LanguageFull { get; set; }

        [Required]
        [StringLength(512)]
        public String Name { get; set; }

        [Required]
        [StringLength(10)]
        public String Extension { get; set; }

        [Required]
        [StringLength(1024)]
        public String Url { get; set; }

        [Required]
        [StringLength(255)]
        public String MimeType { get; set; }

        [Required]
        public long Size { get; set; }

        public MFilesDocument MFilesDocument { get; set; }
    }

    public abstract class ListProperty
    {
        [Key]
        public Guid ListPropertyId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [DefaultValue("")]
        public String Value { get; set; }


       
    }

    public class Title
    {
        [Key, ForeignKey("MFilesDocument")]
        public Guid TitleId { get; set; }
        public MFilesDocument MFilesDocument { get; set; }
        [Required]
        [InverseProperty("Titles")]
        public Document Document { get; set; }

        [Required]
        [StringLength(3)]
        public String Language { get; set; }
        
        [Required]
        [StringLength(255)]
        public String LanguageFull { get; set; }

        [Required(AllowEmptyStrings = true)]
        [DefaultValue("")]
        public String Value { get; set; }

    }


    public class Description
    {
        [Key, ForeignKey("MFilesDocument")]
        public Guid DescriptionId { get; set; }
        public MFilesDocument MFilesDocument { get; set; }

        [Required]
        [InverseProperty("Descriptions")]
        public Document Document { get; set; }

        [Required]
        [StringLength(3)]
        public String Language { get; set; }

        [Required]
        [StringLength(255)]
        public String LanguageFull { get; set;  }

        [Required(AllowEmptyStrings = true)]
        [DefaultValue("")]
        public String Value { get; set; }
        
  
    }
    


    public class ChemicalValue : ListProperty
    {
        public virtual ICollection<Document> Documents { get; set; }
    }

    public class ProgramValue : ListProperty
    {
        public virtual ICollection<Document> Documents { get; set; }
    }


    public class TermValue : ListProperty
    {
        public virtual ICollection<Document> Documents { get; set; }
    }

    public class TagValue : ListProperty
    {
        public virtual ICollection<Document> Documents { get; set; }
    }

    public class TypeValue : ListProperty
    {
        public virtual ICollection<Document> Documents { get; set; }
    }

    public class MeetingValue : ListProperty
    {
        public virtual ICollection<Document> Documents { get; set; }
    }

    public class MeetingTypeValue : ListProperty
    {
        public virtual ICollection<Document> Documents { get; set; }
    }
}