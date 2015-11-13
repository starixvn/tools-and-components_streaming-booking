using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StreamingBooking
{
    public static class Helper
    {
        public static IList<string> ReadFile(string filePath)
        {
            IList<string> lines = new List<string>();

            if (File.Exists(filePath))
            {
                lines = File.ReadAllLines(filePath).ToList();
            }
            else
            {
                File.WriteAllText(filePath, string.Empty);
            }

            return lines;
        }

        public static Dictionary<int, string> ReadRemarkFile(string filePath)
        {
            var data = new Dictionary<int, string>();

            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);

                foreach (var line in lines)
                {
                    var streamId = line.Substring(0, line.IndexOf(':'));
                    var remark = line.Substring(line.IndexOf(':') + 1, line.Length - line.IndexOf(':') - 1);

                    if (!data.Keys.Contains(int.Parse(streamId)))
                    {
                        data.Add(int.Parse(streamId), remark);
                    }
                }
            }
            else
            {
                File.WriteAllText(filePath, string.Empty);
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