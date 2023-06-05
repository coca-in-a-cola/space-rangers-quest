using System.Collections;
using System.Collections.Generic;

namespace SRQ {
    public partial class QM {
        public int Header { get; set; }
        public List<QMParam> QMParams {get; set;}

        public List<Location> Locations { get; set; }
        public List<Jump> Jumps { get; set; }
    }
}
