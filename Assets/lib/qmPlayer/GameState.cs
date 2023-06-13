using System;
using System.Collections.Generic;
using System.Linq;
using SRQ.Formulas;

namespace SRQ {
    public class GameLogStep {
        public DateTime DateUnix { get; set; }
        public int JumpId { get; set; }
    }

    public class GameLog {
        public string AleaSeed { get; set; }
        public List<GameLogStep> PerformedJumps { get; set; }
    }

    public partial class GameState : GameLog {
        public GameStateType State { get; protected set; }
        public int? CritParamId { get; protected set; }
        public int LocationId { get; protected set; }
        public int? LastJumpId { get; protected set; }
        public List<(int? Id, bool? active)> PossibleJumps { get; protected set; }
        public List<int> ParamValues { get; protected set; }
        public List<bool> ParamShow { get; protected set; }
        public Dictionary<int, int> JumpedCount { get; protected set; }
        public Dictionary<int, int> LocationVisitCount { get; protected set; }
        public int DaysPassed { get; protected set; }
        public string ImageName { get; protected set; }
        public string TrackName { get; protected set; }
        public string SoundName { get; protected set; }
        public AleaState AleaState { get; protected set; }

        public QM Quest { get; }

        private bool showDebug = true;
        private Func<int?, double> random = null;

        public GameState (QM quest, string seed, Player player) {
            Alea alea = new Alea(seed);
            random = alea.Random;
            this.CurrentPlayer = player;
            CurrentPlayerState = new PlayerState();

            var startLocation = quest.Locations.Find(x => x.IsStarting);
            if (startLocation == null) {
                throw new Exception("No start location!");
            }

            var startingParams = new List<int>();
            foreach (var param in quest.QMParams) {
                if (!param.Active) {
                    startingParams.Add(0);
                }
                else if (param.IsMoney) {
                    int giveMoney = 2000;
                    int money = param.Max > giveMoney ? giveMoney : param.Max;
                    string starting = $"[{money}]";
                    startingParams.Add(Formula.Calculate(starting, new List<int>(), alea.Random));
                }
                else {
                    startingParams.Add(Formula.Calculate(param.Starting.Replace("h", ".."), new List<int>(), alea.Random));
                }
            }

            var startingShowing = new List<bool>();
            foreach (var _ in quest.QMParams) {
                startingShowing.Add(true);
            }


            State = GameStateType.Starting;
            LocationId = startLocation.Id;
            LastJumpId = null;
            CritParamId = null;
            PossibleJumps = new List<(int? Id, bool? active)>();
            ParamValues = startingParams;
            ParamShow = startingShowing;
            JumpedCount = new Dictionary<int, int>();
            LocationVisitCount = new Dictionary<int, int>();
            DaysPassed = 0;
            ImageName = null;
            TrackName = null;
            SoundName = null;
            AleaSeed = seed;
            PerformedJumps = new List<GameLogStep>();
            AleaState = alea.ExportState();

            Quest = quest;
        }

        public void PerformJump(int jumpId, DateTime? dateUnix = null) {
            PerformedJumps.Add(new GameLogStep { DateUnix = dateUnix ?? DateTime.UtcNow, JumpId = jumpId });

            // Clear sound name before jump
            SoundName = null;

            PerformJumpInternal(jumpId);
        }

        private void PerformJumpInternal(int jumpId) {
            if (jumpId == Constants.JUMP_GO_BACK_TO_SHIP) {
                State = GameStateType.ReturnedEnding;
                return;
            }

            /*
            // Before for unknown reasons it used media from last jump
            // TODO: Test how original game behaves with media

            Jump lastJumpMedia = quest.jumps.FirstOrDefault(x => x.id == state.lastJumpId);
            if (lastJumpMedia != null && lastJumpMedia.img != null)
            {
                state = new GameState
                {
                    stateOriginal = state,
                    imageName = lastJumpMedia.img,
                };
            }
            */

            Jump jumpMedia = Quest.Jumps.FirstOrDefault(x => x.Id == jumpId);
            ImageName = jumpMedia?.Img ?? ImageName;
            TrackName = ReplaceSpecialTrackName(jumpMedia?.Track ?? TrackName);
            SoundName = jumpMedia?.Sound ?? SoundName;

            if (State == GameStateType.Starting) {
                State = GameStateType.Location;
                CalculateLocation();
            }
            else if (State == GameStateType.Jump) {
                Jump currentJump = Quest.Jumps.FirstOrDefault(x => x.Id == LastJumpId);
                if (currentJump == null) {
                    throw new Exception($"Internal error: no jump {LastJumpId}");
                }
                LocationId = currentJump.ToLocationId;
                State = GameStateType.Location;
                CalculateLocation();
            }
            else if (State == GameStateType.Location) {
                if (PossibleJumps.FirstOrDefault(x => x.Id == jumpId) == (null, null)) {
                    throw new Exception($"Jump {jumpId} is not in list in that location. Possible jumps={string.Join(",", PossibleJumps.Select(x => $"{x.Id}({x.active})"))}");
                }

                Jump jump = Quest.Jumps.FirstOrDefault(x => x.Id == jumpId);
                if (jump == null) {
                    throw new Exception($"Internal Error: no jump id={jumpId} from possible jump list");
                }

                LastJumpId = jumpId;

                if (jump.DayPassed) {
                    DaysPassed = DaysPassed + 1;
                }

                JumpedCount[jumpId] = JumpedCount.ContainsKey(jumpId) ? JumpedCount[jumpId] + 1 : 1;

                var critParamsTriggered = CalculateParamsUpdate(jump.ParamsChanges);

                Location nextLocation = Quest.Locations.FirstOrDefault(x => x.Id == jump.ToLocationId);
                if (nextLocation == null) {
                    throw new Exception($"Internal error: no next location {jump.ToLocationId}");
                }
                if (string.IsNullOrEmpty(jump.Description)) {
                    if (critParamsTriggered.Count > 0) {
                        int critParamId = critParamsTriggered[0];
                        State = GameStateType.CritOnJump;
                        CritParamId = critParamId;

                        ImageName = jump.ParamsChanges[critParamId].Img ??
                                    Quest.QMParams[critParamId].Img ??
                                    ImageName;
                        TrackName = ReplaceSpecialTrackName(jump.ParamsChanges[critParamId].Track ??
                                                            Quest.QMParams[critParamId].Track ??
                                                            TrackName);
                        SoundName = jump.ParamsChanges[critParamId].Sound ??
                                    Quest.QMParams[critParamId].Sound ??
                                    SoundName;
                    }
                    else {
                        LocationId = nextLocation.Id;
                        State = GameStateType.Location;
                        CalculateLocation();
                    }
                }
                else {
                    if (critParamsTriggered.Count > 0) {

                        State = GameStateType.JumpAndNextCrit;
                        CritParamId = critParamsTriggered[0];
                    }
                    else if (nextLocation.IsEmpty) {
                        LocationId = nextLocation.Id;
                        State = GameStateType.Location;
                        CalculateLocation();
                    }
                    else {
                        State = GameStateType.Jump;
                    }
                }
            }
            else if (State == GameStateType.JumpAndNextCrit) {
                State = GameStateType.CritOnJump;
                Jump jump = Quest.Jumps.FirstOrDefault(x => x.Id == LastJumpId);

                ImageName = CritParamId != null
                    ? jump?.ParamsChanges[(int)CritParamId]?.Img ?? Quest.QMParams[(int)CritParamId]?.Img ?? ImageName
                    : ImageName;
                TrackName = ReplaceSpecialTrackName(CritParamId != null
                    ? jump?.ParamsChanges[(int)CritParamId]?.Track ?? Quest.QMParams[(int)CritParamId]?.Track ?? TrackName
                    : TrackName);
                SoundName = CritParamId != null
                    ? jump?.ParamsChanges[(int)CritParamId]?.Sound ?? Quest.QMParams[(int)CritParamId]?.Sound ?? SoundName
                    : SoundName;
            }
            else if (State == GameStateType.CritOnLocation) {
                State = GameStateType.CritOnLocationLastMessage;
            }
            else {
                throw new Exception($"Unknown state {State} in performJump");
            }
        }


        public void CalculateLocation() {
            if (State != GameStateType.Location) {
                throw new Exception("Internal error: expecting \"location\" state");
            }

            LocationVisitCount[LocationId] = LocationVisitCount.TryGetValue(LocationId, out var count) ? count + 1 : 0;


            var location = Quest.Locations.FirstOrDefault(x => x.Id == LocationId);
            if (location == null)
            {
                throw new Exception($"Internal error: no state location {LocationId}");
            }

            var locImgId = CalculateLocationShowingTextId(location);

            if (location.Media.Count > locImgId) {
                ImageName = (location.Media[locImgId]?.Img) ?? ImageName;
                TrackName = ReplaceSpecialTrackName((location.Media[locImgId]?.Track) ?? TrackName);
                SoundName = (location.Media[locImgId]?.Sound) ?? SoundName;
            }
            if (location.DayPassed) {
                DaysPassed++;
            }

            var critParamsTriggered = CalculateParamsUpdate(location.ParamsChanges);

            var oldTgeBehaviour =
                Quest.Header == Constants.HEADER_QM_2 ||
                Quest.Header == Constants.HEADER_QM_3 ||
                Quest.Header == Constants.HEADER_QM_4 ||
                Quest.Header == Constants.HEADER_QMM_7_WITH_OLD_TGE_BEHAVIOUR;

            var allJumpsFromThisLocation = Quest.Jumps
                .Where(x => x.FromLocationId == LocationId)
                .Where(jump => {
                    // Сразу выкинуть переходы в локации с превышенным лимитом
                    var toLocation = Quest.Locations.FirstOrDefault(x => x.Id == jump.ToLocationId);
                    if (toLocation != null) {
                        if (toLocation.MaxVisits != 0 &&
                            LocationVisitCount.TryGetValue(jump.ToLocationId, out var count) && count + 1 >= toLocation.MaxVisits) {
                            return false;
                        }
                    }

                    if (oldTgeBehaviour) {
                        // Это какая-то особенность TGE - не учитывать переходы, которые ведут в локацию
                        // где были переходы, а проходимость закончилась.
                        // Это вообще дикость какая-то, потому как там вполне может быть
                        // критичный параметр завершить квест
                        var jumpsFromDestination = Quest.Jumps.Where(x => x.FromLocationId == jump.ToLocationId).ToList();
                        if (jumpsFromDestination.Count == 0) {
                            // Но если там вообще переходов не было, то всё ок
                            return true;
                        }
                        if (jumpsFromDestination.Count(x => x.JumpingCountLimit != 0
                            && JumpedCount.TryGetValue(x.Id, out var jumpedCount)
                            && jumpedCount >= x.JumpingCountLimit) == jumpsFromDestination.Count) {
                            return false;
                        }
                        else {
                            return true;
                        }
                    }
                    else {
                        return true;
                    }
                });
            var allJumpsFromThisLocationSorted = SortJumps(allJumpsFromThisLocation.ToList());

            var allPossibleJumps = allJumpsFromThisLocationSorted
                .Select(jump => new Tuple<Jump, bool>(jump, IsJumpActive(jump)))
                .ToList();
            

            var possibleJumpsWithSameTextGrouped = new List<Tuple<Jump, bool>>();
            var seenTexts = new Dictionary<string, bool>();

            // TODO: здесь ошибка, которая отфильтровывает нужные единственные прыжки из пустой локации.
            foreach (var j in allPossibleJumps) {
                var jump = j.Item1;
                var active = j.Item2;
                if (!seenTexts.ContainsKey(jump.Text)) {
                    seenTexts[jump.Text] = true;
                    var jumpsWithSameText = allPossibleJumps.Where(x => x.Item1.Text == jump.Text).ToList();
                    if (jumpsWithSameText.Count() == 1) {
                        if (jump.Priority < 1 && active) {
                            const int ACCURACY = 1000;
                            active = random(ACCURACY) < j.Item1.Priority * ACCURACY;
                            // Console.WriteLine($"Jump {j.jump.text} is now {j.active} by random");
                        }
                        if (active || jump.AlwaysShow) {
                            possibleJumpsWithSameTextGrouped.Add(j);
                        }
                    }
                    else {
                        var jumpsActiveWithSameText = jumpsWithSameText.Where(x => x.Item2).ToList();
                        if (jumpsActiveWithSameText.Count() > 0) {
                            var maxPrio = jumpsActiveWithSameText.Max(jump => jump.Item1.Priority);
                            var jumpsWithNotSoLowPrio = jumpsActiveWithSameText.Where(x => x.Item1.Priority * 100 >= maxPrio).ToList();
                            var prioSum = jumpsWithNotSoLowPrio.Sum(x => x.Item1.Priority);
                            const int ACCURACY = 1000000;
                            double rnd = (random(ACCURACY) / (double)ACCURACY) * prioSum;
                            foreach (var jj in jumpsWithNotSoLowPrio) {
                                if (jj.Item1.Priority >= rnd || jj == jumpsWithNotSoLowPrio.Last()) {
                                    possibleJumpsWithSameTextGrouped.Add(jj);
                                    break;
                                }
                                else {
                                    rnd = rnd - jj.Item1.Priority;
                                }
                            }
                        }
                        else {
                            var atLeastOneWithAlwaysShow = jumpsWithSameText.FirstOrDefault(x => x.Item1.AlwaysShow);
                            if (atLeastOneWithAlwaysShow != null) {
                                possibleJumpsWithSameTextGrouped.Add(atLeastOneWithAlwaysShow);
                            }
                        }
                    }
                }
            }

            var newJumpsWithoutEmpty = possibleJumpsWithSameTextGrouped.Where(x => !string.IsNullOrEmpty(x.Item1.Text)).ToList();
            var newActiveJumpsOnlyEmpty = possibleJumpsWithSameTextGrouped.Where(x => x.Item2 && string.IsNullOrEmpty(x.Item1.Text)).ToList();
            var newActiveJumpsOnlyOneEmpty = newActiveJumpsOnlyEmpty.Any()
                ? new List<Tuple<Jump, bool>> { newActiveJumpsOnlyEmpty.First() } : new List<Tuple<Jump, bool>>();

            var statePossibleJumps = (newJumpsWithoutEmpty.Any() ? newJumpsWithoutEmpty : newActiveJumpsOnlyOneEmpty).Select(x =>
            {
                (int? id, bool? active) res = (x.Item1.Id, x.Item2);
                return res;
            }).ToList();

            PossibleJumps = statePossibleJumps;

            foreach (int critParam in critParamsTriggered) {
                bool gotFailyCritWithChoices = (Quest.QMParams[critParam].Type == ParamType.Провальный
                    || Quest.QMParams[critParam].Type == ParamType.Смертельный)
                    && PossibleJumps.Exists(x => (bool)x.active);

                if (oldTgeBehaviour && gotFailyCritWithChoices) {
                    // Do nothing because some jumps allows this
                }
                else {
                    var lastJump = Quest.Jumps.Find(x => x.Id == LastJumpId);

                    State = location != null
                        ? LastJumpId != null && lastJump != null && !string.IsNullOrEmpty(lastJump.Description)
                            ? GameStateType.CritOnLocation
                            : GameStateType.CritOnLocationLastMessage
                        : GameStateType.CritOnLocation;

                    CritParamId = critParam;

                    ImageName = location.ParamsChanges[critParam].Img ?? Quest.QMParams[critParam].Img ?? ImageName;
                    TrackName = ReplaceSpecialTrackName(location.ParamsChanges[critParam].Track ?? Quest.QMParams[critParam].Track ?? TrackName);
                    SoundName = location.ParamsChanges[critParam].Sound ?? Quest.QMParams[critParam].Sound ?? SoundName;
                }
            }

            // calculateLocation is always called when state.state === "location", but state.state can change
            /* А это дикий костыль для пустых локаций и переходов */
            if (State == GameStateType.Location && PossibleJumps.Count == 1) {
                var lonenyCurrentJumpInPossible = PossibleJumps[0];
                var lonenyCurrentJump = Quest.Jumps.Find(x => x.Id == lonenyCurrentJumpInPossible.Id);

                if (lonenyCurrentJump == null) {
                    throw new Exception($"Unable to find jump id={lonenyCurrentJumpInPossible.Id}");
                }

                var lastJump = Quest.Jumps.Find(x => x.Id == LastJumpId);
                var locTextId = CalculateLocationShowingTextId(location);
                var locationOwnText = location.Texts.ElementAtOrDefault(locTextId) ?? "";

                Shared.Log($"\n oldTgeBehaviour ={ oldTgeBehaviour}"
                 + $"\n locationOwnText ={ locationOwnText}"
                 + $"\n isEmpty ={ location.IsEmpty}"
                 + $"\n id ={location.Id}"
                 + $"\n lastJump ={lastJump}"
                 + $"\n lastJumpDesc ={lastJump?.Description}");

                var needAutoJump = (bool)lonenyCurrentJumpInPossible.active
                    && string.IsNullOrEmpty(lonenyCurrentJump.Text)
                    && (location.IsEmpty ? (lastJump != null ? string.IsNullOrEmpty(lastJump.Description) : true) : string.IsNullOrEmpty(locationOwnText));

                if (needAutoJump) {
                    if (showDebug) {
                        Console.WriteLine($"Performinig autojump from loc={LocationId} via jump={lonenyCurrentJump.Id}");
                    }
                    PerformJumpInternal(lonenyCurrentJump.Id);
                }
            }
            else if (State == GameStateType.CritOnLocation) {
                // Little bit copy-paste from branch above
                var lastJump = Quest.Jumps.Find(x => x.Id == LastJumpId);
                var locTextId = CalculateLocationShowingTextId(location);
                var locationOwnText = location.Texts[locTextId] ?? "";
                var locationDoNotHaveText = location.IsEmpty ? (lastJump != null ? !string.IsNullOrEmpty(lastJump.Description) : true) : !string.IsNullOrEmpty(locationOwnText);

                if (locationDoNotHaveText) {
                    State = GameStateType.CritOnLocationLastMessage;
                }
            }

            return;
        }

        bool IsJumpActive(Jump jump) {
            for (int i = 0; i < Quest.ParamsCount; i++) {
                if (Quest.QMParams[i].Active) {
                    if (ParamValues[i] > jump.ParamsConditions[i].MustTo
                        || ParamValues[i] < jump.ParamsConditions[i].MustFrom) {
                        return false;
                    }

                    if (jump.ParamsConditions[i].MustEqualValues.Count > 0) {
                        var isEqual = jump.ParamsConditions[i].MustEqualValues
                            .Where(x => x == ParamValues[i])
                            .ToList();

                        if (jump.ParamsConditions[i].MustEqualValuesEqual && isEqual.Count == 0) {
                            return false;
                        }
                        if (!jump.ParamsConditions[i].MustEqualValuesEqual && isEqual.Count != 0) {
                            return false;
                        }
                    }

                    if (jump.ParamsConditions[i].MustModValues.Count > 0) {
                        var isMod = jump.ParamsConditions[i].MustModValues.Where(x => ParamValues[i] % x == 0).ToList();
                        if (jump.ParamsConditions[i].MustModValuesMod && isMod.Count == 0) {
                            return false;
                        }
                        if (!jump.ParamsConditions[i].MustModValuesMod && isMod.Count != 0) {
                            return false;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(jump.FormulaToPass))
            {
                if (Formula.Calculate(jump.FormulaToPass, ParamValues, this.random) == 0)
                {
                    return false;
                }
            }
            
            if (jump.JumpingCountLimit != 0 && JumpedCount.TryGetValue(jump.Id, out var count) && count >= jump.JumpingCountLimit)
            {
                return false;
            }

            return true;
        }

        public string ReplaceSpecialTrackName(string trackName) {
            if (trackName == Constants.TRACK_NAME_RESET_DEFAULT_MUSIC) {
                return null;
            }
            return trackName;
        }

        public int CalculateLocationShowingTextId(Location location) {
            var locationTextsWithText = location.Texts
                .Select((text, i) => new { Text = text, Index = i })
                .Where(x => x.Text != null)
                .ToList();

            if (location.IsTextByFormula) {
                if (!string.IsNullOrEmpty(location.TextSelectFormula)) {
                    int id = Formula.Calculate(location.TextSelectFormula, ParamValues, random) - 1;
                    if (id < location.Texts.Count && !string.IsNullOrEmpty(location.Texts[id])) {
                        return id;
                    }
                    else {
                        if (showDebug) {
                            Console.WriteLine($"Location id={location.Id} formula result textid={id}, but no text");
                        }
                        return 0;
                    }
                }
                else {
                    if (showDebug) {
                        Console.WriteLine($"Location id={location.Id} text by formula is set, but no formula");
                    }
                    int textNum = (int)random(locationTextsWithText.Count());

                    return locationTextsWithText[textNum]?.Index ?? 0;
                }
            }
            else {
                int textNum = locationTextsWithText.Count > 0
                    ? LocationVisitCount[location.Id] % locationTextsWithText.Count()
                    : 0;

                if (locationTextsWithText.Count == 0) {
                    return 0;
                }

                return locationTextsWithText[textNum]?.Index ?? 0;
            }
        }

        public List<int> CalculateParamsUpdate(IReadOnlyList<ParameterChange> paramsChanges) {
            var critParamsTriggered = new List<int>();
            var oldValues = ParamValues.Take(Quest.ParamsCount).ToList();
            var newValues = ParamValues.Take(Quest.ParamsCount).ToList();

            // DEBUG
            var showChanges = paramsChanges.Where((pc) => pc.ShowingType != ParameterShowingType.НеТрогать).ToList();
            if (showChanges.Count > 0 && showDebug) {
                Shared.Log("REAL CHANGE DETECTED");
                var a = 0;
            }

            var valueChanges = paramsChanges.Where((pc) => pc.IsChangeValue || pc.IsChangePercentage || pc.IsChangeFormula).ToList();
            if (valueChanges.Count > 0 && showDebug) {
                Shared.Log("REAL CHANGE DETECTED");
                var a = 0;
            }

            for (int i = 0; i < Quest.ParamsCount; i++) {
                var change = paramsChanges[i];
                if (change.ShowingType == ParameterShowingType.Показать) {
                    ParamShow[i] = true;
                }
                else if (change.ShowingType == ParameterShowingType.Скрыть) {
                    ParamShow[i] = false;
                }

                if (change.IsChangeValue) {
                    newValues[i] = change.Change;
                }
                else if (change.IsChangePercentage) {
                    newValues[i] = (int)Math.Round(oldValues[i] * (100 + change.Change) / 100.0);
                }
                else if (change.IsChangeFormula) {
                    if (!string.IsNullOrEmpty(change.ChangingFormula)) {
                        newValues[i] = Formula.Calculate(change.ChangingFormula, oldValues, random);
                    }
                }
                else {
                    newValues[i] = oldValues[i] + change.Change;
                }

                if (newValues[i] != oldValues[i] && showDebug) {
                    Shared.Log($"Param updated: {oldValues[i]} -> {newValues[i]}");
                    var a = 0;
                }

                var param = Quest.QMParams[i];
                if (newValues[i] > param.Max) {
                    newValues[i] = param.Max;
                }
                if (newValues[i] < param.Min) {
                    newValues[i] = param.Min;
                }

                if (newValues[i] != oldValues[i] && param.Type != ParamType.Обычный) {
                    if ((param.CritType == ParamCritType.Максимум && newValues[i] == param.Max) ||
                        (param.CritType == ParamCritType.Минимум && newValues[i] == param.Min)) {
                        critParamsTriggered.Add(i);
                    }
                }
            }

            ParamValues = newValues;
            return critParamsTriggered;
        }

        private List<T> SortJumps<T>(List<T> input) where T : IShowingOrder {
            var output = input.ToList();
            for (int i = 0; i < output.Count; i++) {
                int? minimumShowingOrder = null;
                List<int> minimumIndexes = new List<int>();
                for (int ii = i; ii < output.Count; ii++) {
                    var curElement = output[ii];
                    if (minimumShowingOrder == null || curElement.ShowingOrder < minimumShowingOrder) {
                        minimumShowingOrder = curElement.ShowingOrder;
                        minimumIndexes = new List<int> { ii };
                    }
                    else if (curElement.ShowingOrder == minimumShowingOrder) {
                        minimumIndexes.Add(ii);
                    }
                }

                int minimumIndex = minimumIndexes.Count == 1
                    ? minimumIndexes[0]
                    : minimumIndexes[(int)random(minimumIndexes.Count)];

                // Console.WriteLine($"i={i} minimumIndex={minimumIndex} minimumIndexes=", minimumIndexes);
                var swap = output[i];
                output[i] = output[minimumIndex];
                output[minimumIndex] = swap;
            }
            return output;
        }

        public static string SRDateToString(int daysToAdd, string lang, DateTime? initialDate = null) {
            DateTime currentDate = initialDate ?? DateTime.Now;
            DateTime d = currentDate.AddDays(daysToAdd);

            string[] months = lang == "eng"
                ? new string[]
                {
            "January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December"
                }
                : new string[]
                {
            "Января", "Февраля", "Марта", "Апреля", "Мая", "Июня",
            "Июля", "Августа", "Сентября", "Октября", "Ноября", "Декабря"
                };

            return $"{d.Day} {months[d.Month - 1]} {d.Year + 1000}";
        }

    }


    public class PlayerChoice {
        public string Text { get; set; }
        public int JumpId { get; set; }
        public bool Active { get; set; }
    }
}