namespace VegaExpress.Agent.Extentions
{
    public static class PathExtention
    {
        public static string GetAbbreviatedPath(string fullFilePath)
        {
            if (string.IsNullOrWhiteSpace(fullFilePath)) return null!;
            string drive = Path.GetPathRoot(fullFilePath)!.TrimEnd(Path.DirectorySeparatorChar);
            string fileName = Path.GetFileName(fullFilePath);
            string abbreviatedPath = $"{drive}{Path.DirectorySeparatorChar}...{Path.DirectorySeparatorChar}{fileName}";
            return abbreviatedPath;
        }
    }
}
