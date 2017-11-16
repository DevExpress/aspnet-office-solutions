using System;
using System.Collections.Generic;

namespace DB.Documents.DAL {

    public class Item {
        public Item() {
            ChildItems = new List<Item>();
        }
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsFolder { get; set; }
        public virtual BinaryContent Content { get; set; }
        public virtual User Owner { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastAccessTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public virtual List<Item> ChildItems { get; set; }
        public virtual Item ParentItem { get; set; }
        public bool IsRoot {
            get {
                return ParentItem == null;
            }
        }

        public void UpdateContent(byte[] content) {
            Content.Data = content;
            LastWriteTime = DateTime.Now;
        }
    }

    public class BinaryContent {
        public long Id { get; set; }
        public virtual byte[] Data { get; set; }
    }

    public class User {
        public long Id { get; set; }
        public string AccountName { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public virtual BinaryContent Avatar { get; set; }
    }
}