using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TripToPrint.Core
{
    public static class TestingEnvCore
    {
        public static string GetUrlString(Uri url)
        {
            var filename = MakeFileName(url.PathAndQuery);
            var fullpath = Path.Combine(GetCachePath(), filename);
            if (File.Exists(fullpath))
                return File.ReadAllText(fullpath);
            return null;
        }

        public static byte[] GetUrlBytes(Uri url, string args = null)
        {
            var filename = MakeFileName(url + args);
            var fullpath = Path.Combine(GetCachePath(), filename);
            if (File.Exists(fullpath))
                return File.ReadAllBytes(fullpath);
            return null;
        }

        public static void StoreUrlBytes(Uri url, string parameters, byte[] bytes)
        {
            PrepareCacheFolder();
            var filename = MakeFileName(url + parameters);
            File.WriteAllBytes(Path.Combine(GetCachePath(), filename), bytes);
        }

        public static void StoreUrlString(Uri url, string content)
        {
            PrepareCacheFolder();
            var filename = MakeFileName(url.PathAndQuery);
            File.WriteAllText(Path.Combine(GetCachePath(), filename), content);
        }

        private static string GetCachePath()
        {
            return Path.Combine(Environment.CurrentDirectory, "cache");
        }

        private static void PrepareCacheFolder()
        {
            Directory.CreateDirectory(GetCachePath());
        }

        private static string MakeFileName(string str)
        {
            return CalculateMd5Hash(str);
            //return Regex.Replace(str, @"[^\w]", string.Empty);
        }

        public static string CalculateMd5Hash(string input)
        {
            MD5 md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            foreach (var t in hash)
            {
                sb.Append(t.ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
