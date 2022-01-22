using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace DFPay.Application.Services
{
    public class SecurityService
    {

        public string Hash(string invoiceNo, decimal amount, string hash)
        {
            return SHA256_Hash(string.Format("{0}_{1}_{2}", invoiceNo, Convert.ToDouble(amount), hash));
        }

        private string SHA256_Hash(string value)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(value));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString().ToUpper();
            }
        }

        public bool HashMatch(string Hashing, string InvoiceNo, decimal Amount, string Key)
        {
            if (string.Compare(Hashing, Hash(InvoiceNo, Amount, Key)) == 0)
                return true;
            else
                return false;
        }
    }
}
