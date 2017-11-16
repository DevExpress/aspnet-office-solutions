using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using DevExpress.Web.Office;

namespace DevExpress.Web.SqlOfficeStateProvider {

    public static class SqlImplementation {

        public static bool AddCheckedOut(string connectionString, string workSessionId, string documentId, string lockerId) {
            using (SqlConnection conn = new SqlConnection(connectionString)) {
                conn.Open();

                SqlCommand cmd = new SqlCommand("AddCheckedOut", conn);
                cmd.CommandTimeout = SqlOfficeStateProvider.CommandTimeout;
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@WorkSessionId", workSessionId));
                cmd.Parameters.Add(new SqlParameter("@DocumentId", documentId));
                cmd.Parameters.Add(new SqlParameter("@lockerId", lockerId));

                SqlParameter success = cmd.Parameters.Add("@Success", SqlDbType.Bit);
                success.Direction = ParameterDirection.Output;

                SqlParameter WorkSessionIdAlreadyExists = cmd.Parameters.Add("@WorkSessionIdAlreadyExists", SqlDbType.Bit);
                WorkSessionIdAlreadyExists.Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                if ((bool)WorkSessionIdAlreadyExists.Value)
                    throw new CannotAddWorkSessionThatAlreadyExistsException();

                return (bool)success.Value;
            }
        }

        public static bool CheckIn(string connectionString, string workSessionId, string documentId, string workSessionState, string lockerId) {
            using (SqlConnection conn = new SqlConnection(connectionString)) {
                conn.Open();

                SqlCommand cmd = new SqlCommand("CheckIn", conn);
                cmd.CommandTimeout = SqlOfficeStateProvider.CommandTimeout;
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@WorkSessionId", workSessionId));
                cmd.Parameters.Add(new SqlParameter("@DocumentId", documentId));
                cmd.Parameters.Add(new SqlParameter("@lockerId", lockerId));
                cmd.Parameters.Add("@state", SqlDbType.VarBinary, -1).Value = System.Text.Encoding.ASCII.GetBytes(workSessionState);

                SqlParameter success = cmd.Parameters.Add("@Success", SqlDbType.Bit);
                success.Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();

                return (bool)success.Value;
            }
        }

        public static string FindWorkSessionId(string connectionString, string documentId) {
            using (SqlConnection conn = new SqlConnection(connectionString)) {
                conn.Open();

                SqlCommand cmd = new SqlCommand("Find", conn);
                cmd.CommandTimeout = SqlOfficeStateProvider.CommandTimeout;
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@DocumentId", documentId));
                //cmd.Parameters.Add("@DocumentId", SqlDbType.NVarChar, -1).Value = documentId;

                SqlParameter workSessionId = cmd.Parameters.Add("@WorkSessionId", SqlDbType.NVarChar, 50);
                workSessionId.Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();

                if (((SqlString)workSessionId.SqlValue).IsNull)
                    return null;

                return (string)workSessionId.Value;
            }
        }

        public static bool HasWorkSessionId(string connectionString, string workSessionId) {
            using (SqlConnection conn = new SqlConnection(connectionString)) {
                conn.Open();

                SqlCommand cmd = new SqlCommand("HasWorkSessionId", conn);
                cmd.CommandTimeout = SqlOfficeStateProvider.CommandTimeout;
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@WorkSessionId", workSessionId));

                SqlParameter found = cmd.Parameters.Add("@Found", SqlDbType.Bit);
                found.Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();

                return (bool)found.Value;
            }
        }

        public static bool Remove(string connectionString, string workSessionId, string lockerId) {
            using (SqlConnection conn = new SqlConnection(connectionString)) {
                conn.Open();

                SqlCommand cmd = new SqlCommand("Remove", conn);
                cmd.CommandTimeout = SqlOfficeStateProvider.CommandTimeout;
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@WorkSessionId", workSessionId));
                cmd.Parameters.Add(new SqlParameter("@lockerId", lockerId));

                SqlParameter wasLockedByAnotherParam = cmd.Parameters.Add("@wasLockedByAnother", SqlDbType.Bit);
                wasLockedByAnotherParam.Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                bool wasLockedByAnother = (bool)wasLockedByAnotherParam.Value;
                if (wasLockedByAnother)
                    throw new CannotRemoveStateCheckedOutByAnotherProcessException();

                return true;
            }
        }

        public static bool CheckOut(string connectionString, string workSessionId, string lockerId, out string workSessionState) {
            using (SqlConnection conn = new SqlConnection(connectionString)) {
                conn.Open();

                SqlCommand cmd = new SqlCommand("CheckOut", conn);
                cmd.CommandTimeout = SqlOfficeStateProvider.CommandTimeout;
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@WorkSessionId", workSessionId));
                cmd.Parameters.Add(new SqlParameter("@lockerId", lockerId));

                SqlParameter state = cmd.Parameters.Add("@State", SqlDbType.Binary, -1);
                state.Direction = ParameterDirection.Output;

                SqlParameter wasLockedByAnotherParam = cmd.Parameters.Add("@wasLockedByAnother", SqlDbType.Bit);
                wasLockedByAnotherParam.Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                if (((SqlBinary)state.SqlValue).IsNull)
                    workSessionState = null;
                else
                    workSessionState = System.Text.Encoding.ASCII.GetString((byte[])state.Value);


                bool wasLockedByAnother = (bool)wasLockedByAnotherParam.Value;
                if (wasLockedByAnother)
                    throw new CannotCheckoutStateCheckedOutByAnotherProcessException();

                return !wasLockedByAnother;
            }
        }

        public static bool UndoCheckOut(string connectionString, string workSessionId, string lockerId) {
            using (SqlConnection conn = new SqlConnection(connectionString)) {
                conn.Open();

                SqlCommand cmd = new SqlCommand("UndoCheckOut", conn);
                cmd.CommandTimeout = SqlOfficeStateProvider.CommandTimeout;
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@WorkSessionId", workSessionId));
                cmd.Parameters.Add(new SqlParameter("@lockerId", lockerId));

                SqlParameter success = cmd.Parameters.Add("@Success", SqlDbType.Bit);
                success.Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                return (bool)success.Value;
            }
        }

        public static void Set(string connectionString, string key, string value) {
            if(value == null) value = "";

            using (SqlConnection conn = new SqlConnection(connectionString)) {
                conn.Open();

                SqlCommand cmd = new SqlCommand("Set", conn);
                cmd.CommandTimeout = SqlOfficeStateProvider.CommandTimeout;
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@Key", key));
                cmd.Parameters.Add("@Value", SqlDbType.VarBinary, -1).Value = System.Text.Encoding.ASCII.GetBytes(value);

                cmd.ExecuteNonQuery();
            }
        }
        public static string Get(string connectionString, string key) {
            string value = null;
            using (SqlConnection conn = new SqlConnection(connectionString)) {
                conn.Open();

                SqlCommand cmd = new SqlCommand("Get", conn);
                cmd.CommandTimeout = SqlOfficeStateProvider.CommandTimeout;
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@Key", key));

                SqlParameter valueParam = cmd.Parameters.Add("@Value", SqlDbType.Binary, -1);
                valueParam.Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                if (!((SqlBinary)valueParam.SqlValue).IsNull)
                    value = System.Text.Encoding.ASCII.GetString((byte[])valueParam.Value);

                return value;
            }
        }
    }

}
