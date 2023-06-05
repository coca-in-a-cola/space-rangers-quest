using System.Collections;
using System.Collections.Generic;

namespace SRQ {
    public class Location : IParamsChanger {
        public bool DayPassed { get; set; }
        public int Id { get; set; }
        public bool IsStarting { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsFaily { get; set; }
        public bool IsFailyDeadly { get; set; }
        public bool IsEmpty { get; set; }
        public List<string> Texts { get; set; }
        public List<Media> Media { get; set; }
        public bool IsTextByFormula { get; set; }
        public string TextSelectFormula { get; set; }
        public int? MaxVisits { get; set; }
        public int LocX { get; set; }
        public int LocY { get; set; }

        public List<ParameterChange> ParamsChanges { get; set; }

        public static Location ParseLocation(Reader r, int paramsCount) {
            bool dayPassed = r.Int32() != 0;
            int locX = r.Int32();
            int locY = r.Int32();
            int id = r.Int32();
            bool isStarting = r.Byte() != 0;
            bool isSuccess = r.Byte() != 0;
            bool isFaily = r.Byte() != 0;
            bool isFailyDeadly = r.Byte() != 0;
            bool isEmpty = r.Byte() != 0;

            List<ParameterChange> paramsChanges = new List<ParameterChange>();
            for (int i = 0; i < paramsCount; i++) {
                r.Seek(12);
                int change = r.Int32();
                ParameterShowingType showingType = (ParameterShowingType)r.Byte();
                r.Seek(4);
                bool isChangePercentage = r.Byte() != 0;
                bool isChangeValue = r.Byte() != 0;
                bool isChangeFormula = r.Byte() != 0;
                string changingFormula = r.ReadString();
                r.Seek(10);
                string critText = r.ReadString();
                paramsChanges.Add(new ParameterChange {
                    Change = change,
                    ShowingType = showingType,
                    IsChangePercentage = isChangePercentage,
                    IsChangeValue = isChangeValue,
                    IsChangeFormula = isChangeFormula,
                    ChangingFormula = changingFormula,
                    CritText = critText,
                    Img = null,
                    Track = null,
                    Sound = null
                });
            }
            List<string> texts = new List<string>();
            List<Media> media = new List<Media>();
            int locationTexts = r.Int32();
            for (int i = 0; i < locationTexts; i++) {
                texts.Add(r.ReadString());
                string img = r.ReadString(true);
                string sound = r.ReadString(true);
                string track = r.ReadString(true);
                media.Add(new Media { Img = img, Sound = sound, Track = track });
            }
            bool isTextByFormula = r.Byte() != 0;
            r.Seek(4);
            r.ReadString();
            r.ReadString();
            string textSelectFormula = r.ReadString();

            return new Location {
                DayPassed = dayPassed,
                Id = id,
                IsEmpty = isEmpty,
                IsFaily = isFaily,
                IsFailyDeadly = isFailyDeadly,
                IsStarting = isStarting,
                IsSuccess = isSuccess,
                ParamsChanges = paramsChanges,
                Texts = texts,
                Media = media,
                IsTextByFormula = isTextByFormula,
                TextSelectFormula = textSelectFormula,
                MaxVisits = 0,

                LocX = locX,
                LocY = locY
            };
        }

        public static Location ParseLocationQmm(Reader r, int paramsCount) {
            bool dayPassed = r.Int32() != 0;

            int locX = r.Int32();
            int locY = r.Int32();

            int id = r.Int32();
            int maxVisits = r.Int32();

            LocationType type = (LocationType)r.Byte();
            bool isStarting = type == LocationType.Starting;
            bool isSuccess = type == LocationType.Success;
            bool isFaily = type == LocationType.Faily;
            bool isFailyDeadly = type == LocationType.Deadly;
            bool isEmpty = type == LocationType.Empty;

            List<ParameterChange> paramsChanges = new List<ParameterChange>();

            for (int i = 0; i < paramsCount; i++) {
                paramsChanges.Add(new ParameterChange {
                    Change = 0,
                    ShowingType = ParameterShowingType.НеТрогать,
                    IsChangePercentage = false,
                    IsChangeValue = false,
                    IsChangeFormula = false,
                    ChangingFormula = string.Empty,
                    CritText = string.Empty,
                    Img = null,
                    Track = null,
                    Sound = null
                });
            }

            int affectedParamsCount = r.Int32();
            for (int i = 0; i < affectedParamsCount; i++) {
                int paramN = r.Int32();

                int change = r.Int32();
                ParameterShowingType showingType = (ParameterShowingType)r.Byte();

                ParameterChangeType changeType = (ParameterChangeType)r.Byte();
                bool isChangePercentage = changeType == ParameterChangeType.Percentage;
                bool isChangeValue = changeType == ParameterChangeType.Value;
                bool isChangeFormula = changeType == ParameterChangeType.Formula;
                string changingFormula = r.ReadString();
                string critText = r.ReadString();
                string img = r.ReadString(true);
                string sound = r.ReadString(true);
                string track = r.ReadString(true);
                paramsChanges[paramN - 1] = new ParameterChange {
                    Change = change,
                    ShowingType = showingType,
                    IsChangePercentage = isChangePercentage,
                    IsChangeFormula = isChangeFormula,
                    IsChangeValue = isChangeValue,
                    ChangingFormula = changingFormula,
                    CritText = critText,
                    Img = img,
                    Track = track,
                    Sound = sound
                };
            }

            List<string> texts = new List<string>();
            List<Media> media = new List<Media>();
            int locationTexts = r.Int32();
            for (int i = 0; i < locationTexts; i++) {
                string text = r.ReadString();
                texts.Add(text);
                string img = r.ReadString(true);
                string sound = r.ReadString(true);
                string track = r.ReadString(true);
                media.Add(new Media { Img = img, Track = track, Sound = sound });
            }

            bool isTextByFormula = r.Byte() != 0;
            string textSelectFormula = r.ReadString();

            return new Location {
                DayPassed = dayPassed,
                Id = id,
                IsEmpty = isEmpty,
                IsFaily = isFaily,
                IsFailyDeadly = isFailyDeadly,
                IsStarting = isStarting,
                IsSuccess = isSuccess,
                ParamsChanges = paramsChanges,
                Texts = texts,
                Media = media,
                IsTextByFormula = isTextByFormula,
                TextSelectFormula = textSelectFormula,
                MaxVisits = maxVisits,
                LocX = locX,
                LocY = locY
            };
        }
    }
}