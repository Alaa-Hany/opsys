using System;
using System.Text;

namespace MiniFatFs
{
    public static class Converter
    {
        private static readonly Encoding Encoding = Encoding.ASCII; 

        public static byte[] StringToBytes(string s)
        {
            return Encoding.GetBytes(s);
        }

        public static string BytesToString(byte[] bytes, int length)
        {
            if (length > bytes.Length)
                length = bytes.Length;
            
            return Encoding.GetString(bytes, 0, length);
        }
    }
}