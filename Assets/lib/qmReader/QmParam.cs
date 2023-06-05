using System.Collections;
using System.Collections.Generic;
using SRQ;
using UnityEngine;

namespace SRQ {
    public class QMParam : Media, IQMParam {
        public int Min { get; set; }
        public int Max { get; set; }
        public ParamType Type { get; set; }
        public bool ShowWhenZero { get; set; }
        public ParamCritType CritType { get; set; }
        // int showingRangesCount { get; set; }
        public bool Active { get; set; }

        public bool IsMoney { get; set; }
        public string Name { get; set; }

        public List<QMParamShowInfoPart> ShowingInfo { get; set; }
        public string CritValueString { get; set; }
        public string Starting { get; set; }

        public static QMParam ParseParam(Reader r) {
            int min = r.Int32();
            int max = r.Int32();
            r.Int32();
            ParamType type = (ParamType)r.Byte();
            r.Int32();
            bool showWhenZero = r.Byte() != 0;
            ParamCritType critType = (ParamCritType)r.Byte();
            bool active = r.Byte() != 0;
            int showingRangesCount = r.Int32();
            bool isMoney = r.Byte() != 0;
            string name = r.ReadString();
            QMParam param = new QMParam {
                Min = min,
                Max = max,
                Type = type,
                ShowWhenZero = showWhenZero,
                CritType = critType,
                Active = active,
                IsMoney = isMoney,
                Name = name,
                ShowingInfo = new List<QMParamShowInfoPart>(),
                Starting = "",
                CritValueString = "",
                Img = null,
                Sound = null,
                Track = null,
            };

            for (int i = 0; i < showingRangesCount; i++) {
                int from = r.Int32();
                int to = r.Int32();
                string str = r.ReadString();
                param.ShowingInfo.Add(new QMParamShowInfoPart(from, to, str));
            }

            param.CritValueString = r.ReadString();
            param.Starting = r.ReadString();
            return param;
        }

        public static QMParam ParseParamQmm(Reader r) {
            int min = r.Int32();
            int max = r.Int32();
            ParamType type = (ParamType)r.Byte();
            byte unknown1 = r.Byte();
            byte unknown2 = r.Byte();
            byte unknown3 = r.Byte();
            if (unknown1 != 0) {
                Shared.Warn("Unknown1 is params is not zero");
            }
            if (unknown2 != 0) {
                Shared.Warn("Unknown2 is params is not zero");
            }
            if (unknown3 != 0) {
                Shared.Warn("Unknown3 is params is not zero");
            }
            bool showWhenZero = r.Byte() != 0;
            ParamCritType critType = (ParamCritType)r.Byte();
            bool active = r.Byte() != 0;

            int showingRangesCount = r.Int32();
            bool isMoney = r.Byte() != 0;

            string name = r.ReadString();
            QMParam param = new QMParam {
                Min = min,
                Max = max,
                Type = type,
                ShowWhenZero = showWhenZero,
                CritType = critType,
                Active = active,
                IsMoney = isMoney,
                Name = name,
                ShowingInfo = new List<QMParamShowInfoPart>(),
                Starting = "",
                CritValueString = "",
                Img = null,
                Sound = null,
                Track = null,
            };
            for (int i = 0; i < showingRangesCount; i++) {
                int from = r.Int32();
                int to = r.Int32();
                string str = r.ReadString();
                param.ShowingInfo.Add(new QMParamShowInfoPart(from, to, str));
            }
            param.CritValueString = r.ReadString();
            param.Img = r.ReadString(true);
            param.Sound = r.ReadString(true);
            param.Track = r.ReadString(true);
            param.Starting = r.ReadString();
            return param;
        }
    }

    public class QMParamShowInfoPart {
        public int From { get; set; }
        public int To { get; set; }
        public string Str { get; set; }

        public QMParamShowInfoPart(int from, int to, string str) {
            From = from;
            To = to;
            Str = str;
        }
    }

    public class Media : IMedia {

        public string Img { get; set; }
        public string Sound { get; set; }
        public string Track { get; set; }
    }
}