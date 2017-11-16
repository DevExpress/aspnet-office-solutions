using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DevExpress.Web.DatabaseHibernationProvider {
    public class HibernatedItemsStorage {
        HibernatedItemsStorageSettings settings;
        public HibernatedItemsStorage(HibernatedItemsStorageSettings settings) {
            this.settings = settings;
        }
        public HibernatedItem GetItemByWorkSessionId(Guid workSessionId) {
            HibernatedItem item = null;
            using(SqlConnection connection = new SqlConnection(settings.ConnectionString)) {
                string commandText = "SELECT [#WorkSessionId#], [#DocumentId#], [#HibernationTime#], [#Header#], [#Content#] FROM [#TableName#] WHERE [#WorkSessionId#] = @WorkSessionId";
                commandText = HibernationTableQueryHelper.PatchSQLCommandText(commandText, settings);
                SqlCommand command = new SqlCommand(commandText, connection);
                command.Parameters.Add(new SqlParameter("WorkSessionId", workSessionId));
                connection.Open();
                using(SqlDataReader reader = command.ExecuteReader()) {
                    if(reader.Read()) {
                        item = new HibernatedItem(workSessionId);
                        item.DocumentId = (string)reader[settings.ColumnNames.DocumentId];
                        item.HibernationTime = Convert.ToDateTime(reader[settings.ColumnNames.HibernationTime]);
                        item.Header = (byte[])reader[settings.ColumnNames.Header];
                        item.Content = (byte[])reader[settings.ColumnNames.Content];
                    }
                    connection.Close();
                }
            }
            return item;
        }

        public Guid FindWorkSessionId(string documentId) {
            Guid workSessionId = Guid.Empty;
            using(SqlConnection connection = new SqlConnection(settings.ConnectionString)) {
                string commandText = "SELECT [#WorkSessionId#] FROM [#TableName#] WHERE [#DocumentId#] = @DocumentId";
                commandText = HibernationTableQueryHelper.PatchSQLCommandText(commandText, settings);
                SqlCommand command = new SqlCommand(commandText, connection);
                command.Parameters.Add(new SqlParameter("DocumentId", documentId));
                connection.Open();
                using(SqlDataReader reader = command.ExecuteReader()) {
                    if(reader.Read()) 
                        workSessionId = reader.GetGuid(0);
                }
                connection.Close();
            }
            return workSessionId;
        }

        public bool HasItem(Guid workSessionId) {
            bool hasItem = false;
            using(SqlConnection connection = new SqlConnection(settings.ConnectionString)) {
                string commandText = "SELECT [#WorkSessionId#] FROM [#TableName#] WHERE [#WorkSessionId#] = @WorkSessionId";
                commandText = HibernationTableQueryHelper.PatchSQLCommandText(commandText, settings);
                SqlCommand command = new SqlCommand(commandText, connection);
                command.Parameters.Add(new SqlParameter("WorkSessionId", workSessionId));
                connection.Open();
                using(SqlDataReader reader = command.ExecuteReader()) 
                    hasItem = reader.Read();
                connection.Close();
            }
            return hasItem;
        }

        public void CheckIn(HibernatedItem item) {
            if(HasItem(item.WorkSessionId))
                UpdateItem(item);
            else
                AddItem(item);
        }

        public HibernatedItem CheckOut(Guid workSessionId) {
            if(HasItem(workSessionId))
                return GetItemByWorkSessionId(workSessionId);
            return null;
        }
        void AddItem(HibernatedItem item) {
            using(SqlConnection connection = new SqlConnection(settings.ConnectionString)) {
                string commandText = "INSERT INTO [#TableName#] ([#WorkSessionId#], [#DocumentId#], [#HibernationTime#], [#Header#], [#Content#]) VALUES (@WorkSessionId, @DocumentId, @HibernationTime, @Header, @Content)";
                commandText = HibernationTableQueryHelper.PatchSQLCommandText(commandText, settings);
                SqlCommand command = new SqlCommand(commandText, connection);
                command.Parameters.Add(new SqlParameter("@WorkSessionId", item.WorkSessionId));
                command.Parameters.Add(new SqlParameter("@DocumentId", item.DocumentId));
                command.Parameters.Add(new SqlParameter("@HibernationTime", item.HibernationTime));
                command.Parameters.Add(new SqlParameter("@Header", item.Header));
                command.Parameters.Add(new SqlParameter("@Content", item.Content));
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        void UpdateItem(HibernatedItem item) {
            using(SqlConnection connection = new SqlConnection(settings.ConnectionString)) {
                string commandText = "UPDATE [#TableName#] SET [#DocumentId#]=@DocumentId, [#HibernationTime#]=@HibernationTime, [#Header#]=@Header, [#Content#]=@Content WHERE [#WorkSessionId#]=@WorkSessionId";
                commandText = HibernationTableQueryHelper.PatchSQLCommandText(commandText, settings);
                SqlCommand command = new SqlCommand(commandText, connection);
                command.Parameters.Add(new SqlParameter("@WorkSessionId", item.WorkSessionId));
                command.Parameters.Add(new SqlParameter("@DocumentId", item.DocumentId));
                command.Parameters.Add(new SqlParameter("@HibernationTime", item.HibernationTime));
                command.Parameters.Add(new SqlParameter("@Header", item.Header));
                command.Parameters.Add(new SqlParameter("@Content", item.Content));
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void DeleteItem(Guid workSessionId) {
            using(SqlConnection connection = new SqlConnection(settings.ConnectionString)) {
                string commandText = "DELETE FROM [#TableName#] WHERE [#WorkSessionId#]=@WorkSessionId";
                commandText = HibernationTableQueryHelper.PatchSQLCommandText(commandText, settings);
                SqlCommand command = new SqlCommand(commandText, connection);
                command.Parameters.Add(new SqlParameter("@WorkSessionId", workSessionId));
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void DeleteExpiredItems(DateTime time) {
            using(SqlConnection connection = new SqlConnection(settings.ConnectionString)) {
                string commandText = "DELETE FROM [#TableName#] WHERE [#HibernationTime#]=@HibernationTime";
                commandText = HibernationTableQueryHelper.PatchSQLCommandText(commandText, settings);
                SqlCommand command = new SqlCommand(commandText, connection);
                command.Parameters.Add(new SqlParameter("@HibernationTime", time));
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }
        public bool IsStorageValid() {
            using(SqlConnection connection = new SqlConnection(settings.ConnectionString)) {
                try {
                    connection.Open();
                    connection.Close();
                    return true;
                } catch(SqlException) {
                    return false;
                }
            }
        }
    }

    public static class HibernationTableQueryHelper {
        public static string PatchSQLCommandText(string commandText, HibernatedItemsStorageSettings settings) {
            commandText = commandText.Replace("#WorkSessionId#", settings.ColumnNames.WorkSessionId);
            commandText = commandText.Replace("#DocumentId#", settings.ColumnNames.DocumentId);
            commandText = commandText.Replace("#HibernationTime#", settings.ColumnNames.HibernationTime);
            commandText = commandText.Replace("#Header#", settings.ColumnNames.Header);
            commandText = commandText.Replace("#Content#", settings.ColumnNames.Content);
            commandText = commandText.Replace("#TableName#", settings.TableName);
            return commandText;
        }
    }
}