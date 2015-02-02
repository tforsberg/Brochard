using System.Collections.Generic;

namespace OrchardVNext.Setup.Services {
    public class SetupContext {
        public string SiteName { get; set; }
        public IEnumerable<string> EnabledFeatures { get; set; }
    }
}