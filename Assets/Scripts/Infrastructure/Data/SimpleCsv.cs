using System.Collections.Generic;

namespace CivClone.Infrastructure.Data
{
    public static class SimpleCsv
    {
        public static List<string[]> Parse(string csvText)
        {
            var rows = new List<string[]>();
            if (string.IsNullOrWhiteSpace(csvText))
            {
                return rows;
            }

            var lines = csvText.Replace("", string.Empty).Split('
');
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    continue;
                }

                rows.Add(line.Split(','));
            }

            return rows;
        }
    }
}
