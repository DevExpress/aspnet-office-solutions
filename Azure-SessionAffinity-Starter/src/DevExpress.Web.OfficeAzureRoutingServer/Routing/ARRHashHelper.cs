using System.Security.Cryptography;
using System.Text;

namespace DevExpress.Web.OfficeAzureRoutingServer {
    static class ARRHashHelper {
        public static string CalculateHash(string value) {
            int num2;
            SHA256 sha = SHA256.Create();
            byte[] bytes = Encoding.Unicode.GetBytes(value.ToUpper());
            byte[] buffer = sha.ComputeHash(bytes);
            string str = string.Empty;
            for(int i = 0; i < buffer.Length; i = num2 + 1) {
                str += buffer[i].ToString("X2");
                num2 = i;
            }
            return str.ToLower();
        }
    }
}
