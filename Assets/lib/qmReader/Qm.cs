using System;
using System.Collections.Generic;
using SRQ;

namespace SRQ {
    public partial class QM {

        public int GivingRace { get; protected set; }
        public int WhenDone { get; protected set; }
        public int PlanetRace { get; protected set; }
        public int PlayerCareer { get; protected set; }
        public int PlayerRace { get; protected set; }
        public int DefaultJumpCountLimit { get; protected set; }
        public int Hardness { get; protected set; }
        public int ParamsCount { get; protected set; }

        public string ChangeLogString { get; protected set; }
        public int? MajorVersion { get; protected set; }
        public int? MinorVersion { get; protected set; }

        public int ScreenSizeX { get; protected set; }
        public int ScreenSizeY { get; protected set; }
        public int ReputationChange { get; protected set; }
        public int WidthSize { get; protected set; }
        public int HeightSize { get; protected set; }

        public static QM ParseBase(Reader r, int header) {
            if (
                header == Constants.HEADER_QMM_6 ||
                header == Constants.HEADER_QMM_7 ||
                header == Constants.HEADER_QMM_7_WITH_OLD_TGE_BEHAVIOUR
            ) {
                int? majorVersion = (header == Constants.HEADER_QMM_7 || header == Constants.HEADER_QMM_7_WITH_OLD_TGE_BEHAVIOUR)
                    ? r.Int32()
                    : (int?)null;
                int? minorVersion = (header == Constants.HEADER_QMM_7 || header == Constants.HEADER_QMM_7_WITH_OLD_TGE_BEHAVIOUR)
                    ? r.Int32()
                    : (int?)null;
                string changeLogString = (header == Constants.HEADER_QMM_7 || header == Constants.HEADER_QMM_7_WITH_OLD_TGE_BEHAVIOUR)
                    ? r.ReadString(true)
                    : null;

                int givingRace = r.Byte();
                int whenDone = r.Byte();
                int planetRace = r.Byte();
                int playerCareer = r.Byte();
                int playerRace = r.Byte();
                int reputationChange = r.Int32();

                int screenSizeX = r.Int32();
                int screenSizeY = r.Int32();
                int widthSize = r.Int32();
                int heightSize = r.Int32();
                int defaultJumpCountLimit = r.Int32();
                int hardness = r.Int32();

                int paramsCount = r.Int32();

                return new QM() {
                    GivingRace = givingRace,
                    WhenDone = whenDone,
                    PlanetRace = planetRace,
                    PlayerCareer = playerCareer,
                    PlayerRace = playerRace,
                    DefaultJumpCountLimit = defaultJumpCountLimit,
                    Hardness = hardness,
                    ParamsCount = paramsCount,
                    ChangeLogString = changeLogString,
                    MajorVersion = majorVersion,
                    MinorVersion = minorVersion,
                    ScreenSizeX = screenSizeX,
                    ScreenSizeY = screenSizeY,
                    ReputationChange = reputationChange,
                    WidthSize = widthSize,
                    HeightSize = heightSize,
                };
            }
            else {
                int? paramsCount =
                    header == Constants.HEADER_QM_3
                        ? 48
                        : header == Constants.HEADER_QM_2
                            ? 24
                            : header == Constants.HEADER_QM_4
                                ? 96
                                : (int?)null;

                if (!paramsCount.HasValue) {
                    throw new Exception($"Unknown header {header}");
                }

                r.DWordFlag();
                int givingRace = r.Byte();
                int whenDone = r.Byte();
                r.DWordFlag();
                int planetRace = r.Byte();
                r.DWordFlag();
                int playerCareer = r.Byte();
                r.DWordFlag();
                int playerRace = r.Byte();
                int reputationChange = r.Byte();

                int screenSizeX = r.Int32();
                int screenSizeY = r.Int32();
                int widthSize = r.Int32();
                int heightSize = r.Int32();
                r.DWordFlag();

                int defaultJumpCountLimit = r.Int32();
                int hardness = r.Int32();

                return new QM() {
                    GivingRace = givingRace,
                    WhenDone = whenDone,
                    PlanetRace = planetRace,
                    PlayerCareer = playerCareer,
                    PlayerRace = playerRace,
                    DefaultJumpCountLimit = defaultJumpCountLimit,
                    Hardness = hardness,
                    ParamsCount = paramsCount.Value,

                    ReputationChange = reputationChange,

                    // TODO
                    ScreenSizeX = screenSizeX,
                    ScreenSizeY = screenSizeY,

                    WidthSize = widthSize,
                    HeightSize = heightSize
                };
            }
        }
    }
}