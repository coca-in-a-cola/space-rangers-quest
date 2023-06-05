using System.Collections;
using System.Collections.Generic;
using SRQ;

namespace SRQ {
    public class Jump : Media, IParamsChanger, IShowingOrder {
        public class JumpParameterCondition {
            public int MustFrom { get; set; }
            public int MustTo { get; set; }
            public List<int> MustEqualValues { get; set; }
            public bool MustEqualValuesEqual { get; set; }
            public List<int> MustModValues { get; set; }
            public bool MustModValuesMod { get; set; }
        }

        public List<ParameterChange> ParamsChanges { get; set; }

        public double Priority { get; set; }
        public bool DayPassed { get; set; }
        public int Id { get; set; }
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public bool AlwaysShow { get; set; }
        public int? JumpingCountLimit { get; set; }
        public int ShowingOrder { get; set; }

        public List<JumpParameterCondition> ParamsConditions { get; set; }
        public string FormulaToPass { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }

        public static Jump ParseJump(Reader r, int paramsCount) {
            double priority = r.Float64();
            bool dayPassed = r.Int32() != 0;
            int id = r.Int32();
            int fromLocationId = r.Int32();
            int toLocationId = r.Int32();
            r.Seek(1);
            bool alwaysShow = r.Byte() != 0;
            int jumpingCountLimit = r.Int32();
            int showingOrder = r.Int32();

            List<ParameterChange> paramsChanges = new List<ParameterChange>();
            List<JumpParameterCondition> paramsConditions = new List<JumpParameterCondition>();
            for (int i = 0; i < paramsCount; i++) {
                r.Seek(4);
                int mustFrom = r.Int32();
                int mustTo = r.Int32();
                int change = r.Int32();
                // Assuming ParameterShowingType is an enum
                ParameterShowingType showingType = (ParameterShowingType)r.Int32();
                r.Seek(1);
                bool isChangePercentage = r.Byte() != 0;
                bool isChangeValue = r.Byte() != 0;
                bool isChangeFormula = r.Byte() != 0;
                string changingFormula = r.ReadString();

                int mustEqualValuesCount = r.Int32();
                bool mustEqualValuesEqual = r.Byte() != 0;
                List<int> mustEqualValues = new List<int>();
                for (int ii = 0; ii < mustEqualValuesCount; ii++) {
                    mustEqualValues.Add(r.Int32());
                }
                int mustModValuesCount = r.Int32();
                bool mustModValuesMod = r.Byte() != 0;
                List<int> mustModValues = new List<int>();
                for (int ii = 0; ii < mustModValuesCount; ii++) {
                    mustModValues.Add(r.Int32());
                }

                string critText = r.ReadString();
                paramsChanges.Add(new ParameterChange {
                    Change = change,
                    ShowingType = showingType,
                    IsChangeFormula = isChangeFormula,
                    IsChangePercentage = isChangePercentage,
                    IsChangeValue = isChangeValue,
                    ChangingFormula = changingFormula,
                    CritText = critText,
                    Img = null,
                    Track = null,
                    Sound = null
                });
                paramsConditions.Add(new JumpParameterCondition {
                    MustFrom = mustFrom,
                    MustTo = mustTo,
                    MustEqualValues = mustEqualValues,
                    MustEqualValuesEqual = mustEqualValuesEqual,
                    MustModValues = mustModValues,
                    MustModValuesMod = mustModValuesMod
                });
            }

            string formulaToPass = r.ReadString();

            string text = r.ReadString();

            string description = r.ReadString();

            return new Jump {
                Priority = priority,
                DayPassed = dayPassed,
                Id = id,
                FromLocationId = fromLocationId,
                ToLocationId = toLocationId,
                AlwaysShow = alwaysShow,
                JumpingCountLimit = jumpingCountLimit,
                ShowingOrder = showingOrder,
                ParamsChanges = paramsChanges,
                ParamsConditions = paramsConditions,
                FormulaToPass = formulaToPass,
                Text = text,
                Description = description,
                Img = null,
                Track = null,
                Sound = null
            };
        }


        public static Jump ParseJumpQmm(Reader r, int paramsCount, List<QMParam> questParams) {
            double priority = r.Float64();
            bool dayPassed = r.Int32() != 0;
            int id = r.Int32();
            int fromLocationId = r.Int32();
            int toLocationId = r.Int32();

            bool alwaysShow = r.Byte() != 0;
            int jumpingCountLimit = r.Int32();
            int showingOrder = r.Int32();

            List<ParameterChange> paramsChanges = new List<ParameterChange>();
            List<JumpParameterCondition> paramsConditions = new List<JumpParameterCondition>();

            for (int i = 0; i < paramsCount; i++) {
                paramsChanges.Add(new ParameterChange {
                    Change = 0,
                    ShowingType = ParameterShowingType.НеТрогать,
                    IsChangeFormula = false,
                    IsChangePercentage = false,
                    IsChangeValue = false,
                    ChangingFormula = string.Empty,
                    CritText = string.Empty,
                    Img = null,
                    Track = null,
                    Sound = null
                });

                paramsConditions.Add(new JumpParameterCondition {
                    MustFrom = questParams[i].Min,
                    MustTo = questParams[i].Max,
                    MustEqualValues = new List<int>(),
                    MustEqualValuesEqual = false,
                    MustModValues = new List<int>(),
                    MustModValuesMod = false
                });
            }

            int affectedConditionsParamsCount = r.Int32();
            for (int i = 0; i < affectedConditionsParamsCount; i++) {
                int paramId = r.Int32();

                int mustFrom = r.Int32();
                int mustTo = r.Int32();

                int mustEqualValuesCount = r.Int32();
                bool mustEqualValuesEqual = r.Byte() != 0;
                List<int> mustEqualValues = new List<int>();

                for (int ii = 0; ii < mustEqualValuesCount; ii++) {
                    mustEqualValues.Add(r.Int32());
                }

                int mustModValuesCount = r.Int32();
                bool mustModValuesMod = r.Byte() != 0;
                List<int> mustModValues = new List<int>();

                for (int ii = 0; ii < mustModValuesCount; ii++) {
                    mustModValues.Add(r.Int32());
                }

                paramsConditions[paramId - 1] = new JumpParameterCondition {
                    MustFrom = mustFrom,
                    MustTo = mustTo,
                    MustEqualValues = mustEqualValues,
                    MustEqualValuesEqual = mustEqualValuesEqual,
                    MustModValues = mustModValues,
                    MustModValuesMod = mustModValuesMod
                };
            }

            int affectedChangeParamsCount = r.Int32();
            for (int i = 0; i < affectedChangeParamsCount; i++) {
                int paramId = r.Int32();
                int change = r.Int32();

                ParameterShowingType showingType = (ParameterShowingType)r.Byte();

                ParameterChangeType changingType = (ParameterChangeType)r.Byte();

                bool isChangePercentage = changingType == ParameterChangeType.Percentage;
                bool isChangeValue = changingType == ParameterChangeType.Value;
                bool isChangeFormula = changingType == ParameterChangeType.Formula;
                string changingFormula = r.ReadString();

                string critText = r.ReadString();

                string bufImg = r.ReadString(true);
                string bufSound = r.ReadString(true);
                string bufTrack = r.ReadString(true);

                paramsChanges[paramId - 1] = new ParameterChange {
                    Change = change,
                    ShowingType = showingType,
                    IsChangeFormula = isChangeFormula,
                    IsChangePercentage = isChangePercentage,
                    IsChangeValue = isChangeValue,
                    ChangingFormula = changingFormula,
                    CritText = critText,
                    Img = bufImg,
                    Track = bufTrack,
                    Sound = bufSound
                };
            }

            string formulaToPass = r.ReadString();

            string text = r.ReadString();

            string description = r.ReadString();
            string img = r.ReadString(true);
            string sound = r.ReadString(true);
            string track = r.ReadString(true);

            return new Jump {
                Priority = priority,
                DayPassed = dayPassed,
                Id = id,
                FromLocationId = fromLocationId,
                ToLocationId = toLocationId,
                AlwaysShow = alwaysShow,
                JumpingCountLimit = jumpingCountLimit,
                ShowingOrder = showingOrder,
                ParamsChanges = paramsChanges,
                ParamsConditions = paramsConditions,
                FormulaToPass = formulaToPass,
                Text = text,
                Description = description,
                Img = img,
                Track = track,
                Sound = sound
            };
        }
    }
}