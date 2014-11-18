using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace StreamingBooking
{
    public static class Helper
    {
        public static IList<string> ReadFile(string filePath)
        {
            IList<string> lines = new List<string>();
            if (File.Exists(filePath))
            {
                FileStream fielStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
                StreamReader sr = new StreamReader(fielStream);
                string line = string.Empty;
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    lines.Add(line);
                }

                sr.Close();
                fielStream.Close();
            }
            else
            {
                File.CreateText(filePath);
            }

            return lines;
        }

        public static Dictionary<int, string> ReadRemarkFile(string filePath)
        {
            var data = new Dictionary<int, string>();

            if (File.Exists(filePath))
            {
                FileStream fielStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);

                StreamReader sr = new StreamReader(fielStream);
                string line = string.Empty;
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    var streamId = line.Substring(0, line.IndexOf(':'));
                    var remark = line.Substring(line.IndexOf(':') + 1, line.Length - line.IndexOf(':') - 1);
                    if (!data.Keys.Contains(int.Parse(streamId)))
                    {
                        data.Add(int.Parse(streamId), remark);
                    }
                }

                sr.Close();
                fielStream.Close();
            }
            else
            {
                File.CreateText(filePath);
            }

            return data;
        }

        public static void WriteRemarkFile(string filePath, int streamId, string remark)
        {
            var data = ReadRemarkFile(filePath);
            if (!data.Keys.Contains(streamId))
            {
                data.Add(streamId, remark);
            }
            else
            {
                data[streamId] = remark;
            }

            var result = data.Select(x => x.Key + ":" + x.Value).ToArray();

            File.WriteAllLines(filePath, result);
        }

        public static void WriteFile(string filePath, int streamId)
        {
            IList<string> streamIds = ReadFile(filePath);
            if (!streamIds.Contains(streamId.ToString()))
            {
                streamIds.Add(streamId.ToString());
            }
            else
            {
                streamIds.Remove(streamId.ToString());
            }

            File.WriteAllLines(filePath, streamIds);
        }
    }
}