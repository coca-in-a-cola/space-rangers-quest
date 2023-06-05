using System.Collections.Generic;

namespace SRQ {
    public interface IShowingOrder {
        int ShowingOrder { get; }
    }
    public interface IParamsChanger {
        List<ParameterChange> ParamsChanges { get; }
    }

    public interface IMedia {
        string Img { get; set; }
        string Sound { get; set; }
        string Track { get; set; }
    }

    public interface IQMParam : IMedia {
        int Min { get; set; }
        int Max { get; set; }
        ParamType Type { get; set; }
        bool ShowWhenZero { get; set; }
        ParamCritType CritType { get; set; }
        // int showingRangesCount { get; set; }
        bool IsMoney { get; set; }
        string Name { get; set; }
        string Starting { get; set; }
        string CritValueString { get; set; }
        bool Active { get; set; }

        List<QMParamShowInfoPart> ShowingInfo { get; set; }
    }
}