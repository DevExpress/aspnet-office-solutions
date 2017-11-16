using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB.Documents.DAL {
    public class DocumentsDbContext : DbContext {
        public DocumentsDbContext(string connectionString) : base(connectionString) { }

        public DbSet<Item> Items { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<BinaryContent> BinaryContentSet { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            var itemConfig = modelBuilder.Entity<Item>();
            itemConfig.HasKey(i => i.Id);
            itemConfig.Property(i => i.Name).IsRequired();
            itemConfig.Property(i => i.IsFolder).IsRequired();
            itemConfig.HasOptional(i => i.Content);
            itemConfig.HasOptional(i => i.Owner);
            itemConfig.HasOptional(i => i.ParentItem)
                .WithMany(pi => pi.ChildItems);

            var userConfig = modelBuilder.Entity<User>();
            userConfig.HasKey(u => u.Id);
            userConfig.Property(u => u.Name).IsRequired();
            userConfig.Property(u => u.AccountName).IsRequired();
            userConfig.HasOptional(u => u.Avatar);

            var binaryContentConfig = modelBuilder.Entity<BinaryContent>();
            binaryContentConfig.HasKey(bc => bc.Id);

            base.OnModelCreating(modelBuilder);
        }
    }
}
