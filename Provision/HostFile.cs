using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Provision
{
    public partial class HostFile
    {
        private const string etcFolder = @"C:\Windows\System32\drivers\etc";
        public static string Location { get { return Path.Combine(etcFolder, "hosts"); } }

        public readonly Entry[] Entries;

        public static void AddEntry(string ip, string hostname)
        {
            using (var file = new FileStream(Location, FileMode.Append, FileAccess.Write, FileShare.Read))
            using (var w = new StreamWriter(file))
            {
                w.WriteLine("{0}    {1}", ip, hostname);
            }
        }
        public HostFile(Entry[] entries)
        {
            Entries = entries;
        }

        /// <summary>
        /// sample:
        /// 102.54.94.97     rhino.acme.com
        /// 38.25.63.10     x.acme.com
        /// 127.0.0.1       localhost
        /// ::1             localhost
        /// </summary>
        private static readonly Regex ipAndHost = new Regex(@"(?<ip>[^ \t]+)\s+(?<host>[^ \t]+)",
            RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);

        private static readonly Regex newLines = new Regex(@"[\r\n]+");
        private static readonly Regex comment = new Regex(@"(#.*)");

        public static HostFile Parse(string content)
        {
            return new HostFile(newLines.Split(content)
                .Select(l => comment.Replace(l, string.Empty))
                .Select(l => ipAndHost.Match(l))
                .Where(m => m.Success)
                .Select(m => new HostFile.Entry(m.Groups["ip"].Value, m.Groups["host"].Value))
                .ToArray());
        }

        public static HostFile Parse(Stream content)
        {
            var r = new StreamReader(content);
            return Parse(r.ReadToEnd());
        }

        public static HostFile Parse()
        {
            using (var file = new FileStream(Location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                return Parse(file);
        }
        public class Entry : IEquatable<Entry>
        {
            public readonly string Ip;
            public readonly string Host;

            public Entry(string ip, string host)
            {
                Ip = ip;
                Host = host;
            }
            public override bool Equals(object obj)
            {
                return Equals(obj as Entry);
            }
            public bool Equals(Entry other)
            {
                if (ReferenceEquals(null, other)) { return false; }
                return Ip.Equals(other.Ip) && Host.Equals(other.Host);
            }

            public override int GetHashCode()
            {
                int hash = 17;
                hash = hash * 23 + Ip.GetHashCode();
                hash = hash * 23 + Host.GetHashCode();
                return hash;
            }
            public override string ToString()
            {
                return string.Format("{0}\t{1}", Ip, Host);
            }
        }
    }
}
