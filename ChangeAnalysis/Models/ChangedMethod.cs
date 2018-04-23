namespace ChangeAnalysis.Models
{
    using System.Collections.Generic;
    public struct ChangedMethod
    {
        public string Symbol { get; }
        public IEnumerable<string> Locations { get; }

        public ChangedMethod(string symbol, IEnumerable<string> locations)
        {
            this.Symbol = symbol;
            this.Locations = locations;
        }
    }
}
