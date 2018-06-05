
// TOOLS
public static void ForceDeleteDirectory(string path)
{
    var directory = new System.IO.DirectoryInfo(path) { Attributes = FileAttributes.Normal };

    foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
    {
        info.Attributes = FileAttributes.Normal;
    }

    directory.Delete(true);
}


public static IDictionary<string, string> ReadDictionaryFile(string fileName)
{
    Dictionary<string, string> dictionary = new Dictionary<string, string>();
    foreach (string line in System.IO.File.ReadAllLines(fileName))
    {
        if ((!string.IsNullOrEmpty(line)) &&
            (!line.StartsWith(";")) &&
            (!line.StartsWith("#")) &&
            (!line.StartsWith("'")) &&
            (line.Contains('=')))
        {
            int index = line.IndexOf('=');
            string key = line.Substring(0, index).Trim();
            string value = line.Substring(index + 1).Trim();

            if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                (value.StartsWith("'") && value.EndsWith("'")))
            {
                value = value.Substring(1, value.Length - 2);
            }
            dictionary.Add(key, value);
        }
    }

    return dictionary;
}

public string GetRFCDate(DateTime dateTime)
{
    DateTime UtcDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTime);
    return UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", System.Globalization.DateTimeFormatInfo.InvariantInfo);        
}

