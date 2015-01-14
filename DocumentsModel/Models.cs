using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

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

        public DbSet<MFilesDocument> MFilesDocuments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
           modelBuilder.Entity<SingleProperty>()
               .Map<TitleValue>(m => m.Requires("Discriminator").HasValue("Title"))
               .Map<DescriptionValue>(m => m.Requires("Discriminator").HasValue("Description"));
           
        
        }
    }

    public class MFilesDocument
    {
        [Key]
        public Guid MFilesDocumentGuid { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateTime ModifiedDate { get; set; }

        public virtual ICollection<Document> Documents { get; set; }

        public virtual ICollection<TitleValue> Titles { get; set; } 
        public virtual ICollection<DescriptionValue> Descriptions { get; set; } 
    }

    public class Document
    {
        public Document()
        {
            // ReSharper disable  DoNotCallOverridableMethodsInConstructor
            Titles = new HashSet<TitleValue>();
            Descriptions = new HashSet<DescriptionValue>();
            Chemicals = new HashSet<ChemicalValue>();
            Terms = new HashSet<TermValue>();
            Programs = new HashSet<ProgramValue>();
            Types = new HashSet<TypeValue>();
            Files = new HashSet<File>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DocumentId { get; set; }

        [Required]
        [StringLength(255)]
        [DefaultValue("")]
        public String Vault { get; set; }

        [Required]
        [StringLength(255)]
        [Index(IsUnique = true)]
        public String UnNumber { get; set; }


        [StringLength(255)]
        [DefaultValue("")]
        public String Copyright { get; set; }

        [StringLength(255)]
        [DefaultValue("")]
        public String Author { get; set; }

        [StringLength(255)]
        [DefaultValue("")]
        public String Meeting { get; set; }

        [StringLength(255)]
        [DefaultValue("")]
        public String Country { get; set; }


        public DateTime PublicationDate { get; set; }
        public DateTime PublicationUpdateDate { get; set; }


        public DateTime? PeriodStartDate { get; set; }

        public DateTime? PeriodEndDate { get; set; }


        public virtual ICollection<TitleValue> Titles { get; set; }
        public virtual ICollection<DescriptionValue> Descriptions { get; set; }
        public virtual ICollection<ChemicalValue> Chemicals { get; set; }
        public virtual ICollection<TermValue> Terms { get; set; }
        public virtual ICollection<ProgramValue> Programs { get; set; }
        public virtual ICollection<TypeValue> Types { get; set; }

        public virtual ICollection<File> Files { get; set; }

        [InverseProperty("Documents")]
        public virtual MFilesDocument InternalDocument { get; set; }
    }


    public class File
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FileId { get; set; }

        [Required]
        public Document Document { get; set; }

        [Required]
        [StringLength(3)]
        public String Language { get; set; }

        [Required]
        [StringLength(512)]
        public String Name { get; set; }

        [Required]
        [StringLength(255)]
        public String MimeType { get; set; }

        [Required]
        public long Size { get; set; }

        [Required]
        public Guid MFilesDocumentGuid { get; set; }
        [ForeignKey("MFilesDocumentGuid")]
        public MFilesDocument MFilesDocument { get; set; }
    }

    public abstract class SingleProperty
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SinglePropertyId { get; set; }

        [Required]
        public int DocumentId { get; set; }

        [Required]
        [StringLength(3)]
        public String Language { get; set; }

        [Required(AllowEmptyStrings = true)]
        [DefaultValue("")]
        public String Value { get; set; }

        [Required]
        public Guid MFilesDocumentGuid { get; set; }
    }

    public abstract class ListProperty
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ListPropertyId { get; set; }


        public int? ParentId { get; set; }
        
        [ForeignKey("ParentId")]
        public ListProperty Parent { get; set; }

        [Required]
        [StringLength(3)]
        public String Language { get; set; }

        [Required(AllowEmptyStrings = true)]
        [DefaultValue("")]
        public String Value { get; set; }


       
    }

    public class TitleValue : SingleProperty
    {
        [ForeignKey("DocumentId")]
        [InverseProperty("Titles")]
        public Document Document { get; set; }

        [ForeignKey("MFilesDocumentGuid")]
        [InverseProperty("Titles")]
        public MFilesDocument MFilesDocument { get; set; }
    }


    public class DescriptionValue : SingleProperty
    {
        [ForeignKey("DocumentId")]
        [InverseProperty("Descriptions")]
        public Document Document { get; set; }

        [ForeignKey("MFilesDocumentGuid")]
        [InverseProperty("Descriptions")]
        public MFilesDocument MFilesDocument { get; set; }
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

    public class TypeValue : ListProperty
    {
        public virtual ICollection<Document> Documents { get; set; }
    }
}