using System.Collections;
using System.Collections.Generic;


namespace SRQ {
    public class ParameterChange : IMedia {
        public int Change { get; set; }
        public bool IsChangePercentage { get; set; }
        public bool IsChangeValue { get; set; }
        public bool IsChangeFormula { get; set; }
        public string ChangingFormula { get; set; }
        public ParameterShowingType ShowingType { get; set; }
        public string CritText { get; set; }
        public string Img { get; set; }
        public string Sound { get; set; }
        public string Track { get; set; }
    }
}