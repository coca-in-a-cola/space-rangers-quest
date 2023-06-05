using System.Collections;
using System.Collections.Generic;

namespace SRQ {
    public partial class QM {
        public Dictionary<string, string> Strings { get; set; }
        public int LocationsCount { get; set; }
        public int JumpsCount { get; set; }
        public string SuccessText { get; set; }
        public string TaskText { get; set; }

        public static QM ParseBase2(QM base1, Reader r, bool isQmm) {
            string ToStar = r.ReadString();

            string Parsec = isQmm ? null : r.ReadString(true);
            string Artefact = isQmm ? null : r.ReadString(true);

            string ToPlanet = r.ReadString();
            string Date = r.ReadString();
            string Money = r.ReadString();
            string FromPlanet = r.ReadString();
            string FromStar = r.ReadString();
            string Ranger = r.ReadString();

            int locationsCount = r.Int32();
            int jumpsCount = r.Int32();

            string successText = r.ReadString();

            string taskText = r.ReadString();

            string unknownText = isQmm ? null : r.ReadString();


            base1.Strings = new Dictionary<string, string> {
                ["ToStar"] = ToStar,
                ["Parsec"] = Parsec,
                ["Artefact"] = Artefact,
                ["ToPlanet"] = ToPlanet,
                ["Date"] = Date,
                ["Money"] = Money,
                ["FromPlanet"] = FromPlanet,
                ["FromStar"] = FromStar,
                ["Ranger"] = Ranger,
            };
            base1.LocationsCount = locationsCount;
            base1.JumpsCount = jumpsCount;
            base1.SuccessText = successText;
            base1.TaskText = taskText;

            return base1;
        }
    }
}