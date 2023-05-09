namespace Interpretarr.Clients.FlareSolver.Model
{
    public class FlareSolverResponse
    {
        public string Status;
        public string Message;
        public long StartTimestamp;
        public long EndTimestamp;
        public string Version;
        public Solution Solution;
        public string Session;
        public string[] Sessions;
    }
}
