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
        public DbSet<StringValue> Values { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
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

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateTime ModifiedDate { get; set; }

        private DateTime? _publicationDate;
        public DateTime PublicationDate
        {
            get { return _publicationDate.HasValue? _publicationDate.Value:CreatedDate;}
            set { _publicationDate = value; }
        }

        private DateTime? _publicationUpdateDate;
        public DateTime PublicationUpdateDate
        {
            get { return _publicationUpdateDate.HasValue ? _publicationUpdateDate.Value : ModifiedDate; }
            set { _publicationUpdateDate = value; }
        }


        public DateTime? PeriodStartDate { get; set; }

        public DateTime? PeriodEndDate { get; set; }


        public virtual ICollection<TitleValue> Titles { get; set; }
        public virtual ICollection<DescriptionValue> Descriptions { get; set; }
        public virtual ICollection<ChemicalValue> Chemicals { get; set; }
        public virtual ICollection<TermValue> Terms { get; set; }
        public virtual ICollection<ProgramValue> Programs { get; set; }
        public virtual ICollection<TypeValue> Types { get; set; }

        public virtual ICollection<File> Files { get; set; }
    }


    public class File
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FileId { get; set; }

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
        public int Size { get; set; }

        [Required]
        public Document Document { get; set; }
    }

    public class StringValue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StringValueId { get; set; }

        [Required]
        [StringLength(3)]
        public String Language { get; set; }

        [Required(AllowEmptyStrings = true)]
        [DefaultValue("")]
        public String Value { get; set; }

        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        
        public StringValue Parent { get; set; }
    }

    public class TitleValue : StringValue
    {
        public virtual ICollection<Document> Documents { get; set; }
    }

    public class DescriptionValue : StringValue
    {
        public virtual ICollection<Document> Documents { get; set; }
    }

    public class ChemicalValue : StringValue
    {
        public virtual ICollection<Document> Documents { get; set; }
    }

    public class ProgramValue : StringValue
    {
        public virtual ICollection<Document> Documents { get; set; }
    }


    public class TermValue : StringValue
    {
        public virtual ICollection<Document> Documents { get; set; }
    }

    public class TypeValue : StringValue
    {
        public virtual ICollection<Document> Documents { get; set; }
    }
}