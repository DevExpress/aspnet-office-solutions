using System;
using System.Data.Entity;

namespace DB.Documents.DAL {
    public class UnitOfWork : IDisposable {
        private DocumentsDbContext context;
        private GenericRepository<Item> itemRepository;

        public UnitOfWork(string connectionString) {
            this.context = new DocumentsDbContext(connectionString);
            Database.SetInitializer<DocumentsDbContext>(null);
        }

        public GenericRepository<Item> ItemRepository {
            get {

                if(this.itemRepository == null) {
                    this.itemRepository = new GenericRepository<Item>(context);
                }
                return itemRepository;
            }
        }

        public void Save() {
            context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing) {
            if(!this.disposed) {
                if(disposing) {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
