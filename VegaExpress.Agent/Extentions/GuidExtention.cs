namespace VegaExpress.Agent.Extentions
{
    public static class GuidExtention
    {
        public static string GetLastBlock(this Guid guid)
        {            
            string guidString = guid.ToString("D");
            string[] blocks = guidString.Split('-');
            string lastBlock = blocks[^1];
            return lastBlock;
        }
    }
}
