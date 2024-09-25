namespace Vaetech.Shell
{
    public partial class PShellResult
    {
        public int ExitCode { get; set; }
        public int Line { get; set; } = 0;
        public string? Output { get; set; }        
        public bool? IbException { get; set; }               
        public string[]? Causes { get; set; }
        
        public PShellResult() { }
        public PShellResult(string output)
        {
            Output = output;
            IbException = false;
        }        
        public static PShellResult Fail(int line, string output, params string[] causes) => new PShellResult { Line = line, IbException = true, Output = output, Causes = causes };        
        public static PShellResult Success(int line, string output) => new PShellResult(output) { Line = line };
    }    
}
