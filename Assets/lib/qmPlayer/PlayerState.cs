using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SRQ.Formulas;

namespace SRQ {
    public class PlayerState {
        public string Text { get; set; }
        public string ImageName { get; set; }
        public string TrackName { get; set; }
        public string SoundName { get; set; }
        public List<string> ParamsState { get; set; }
        public List<PlayerChoice> Choices { get; set; }
        public PlayerGameStateType GameState { get; set; }
    }

    public partial class GameState : GameLog {
        public Player CurrentPlayer { get; }
        public PlayerState CurrentPlayerState { get; private set; }

        public PlayerState GetUIState() {
            Alea alea = new Alea(AleaState.data.ToString());
            random = alea.Random;
            var texts = CurrentPlayer.Lang == "rus"
                ? new {
                    IAgree = "Я берусь за это задание",
                    Next = "Далее",
                    GoBackToShip = "Вернуться на корабль",
                }
                : new {
                    IAgree = "I agree",
                    Next = "Next",
                    GoBackToShip = "Go back to ship",
                };

            if (State == GameStateType.Starting) {
                CurrentPlayerState.Text = Replace(Quest.TaskText);
                CurrentPlayerState.ParamsState = new List<string>();
                CurrentPlayerState.Choices = new List<PlayerChoice>
                {
                    new PlayerChoice{ JumpId = Constants.JUMP_I_AGREE, Text = texts.IAgree, Active = true },
                };
                CurrentPlayerState.GameState = PlayerGameStateType.Running;
                CurrentPlayerState.ImageName = null;
                CurrentPlayerState.TrackName = null;
                CurrentPlayerState.SoundName = null;

                return CurrentPlayerState;
            }
             else if (State == GameStateType.Jump) {
                var jump = Quest.Jumps.FirstOrDefault(x => x.Id == LastJumpId);
                if (jump == null) {
                    throw new Exception($"Internal error: no last jump id={LastJumpId}");
                }
                CurrentPlayerState.Text = Replace(jump.Description);
                CurrentPlayerState.ParamsState = GetParamsState();
                CurrentPlayerState.Choices = new List<PlayerChoice>
                {
                    new PlayerChoice { JumpId = Constants.JUMP_NEXT, Text = texts.Next, Active = true },
                };
                CurrentPlayerState.GameState = PlayerGameStateType.Running;
                CurrentPlayerState.ImageName = ImageName;
                CurrentPlayerState.TrackName = ReplaceSpecialTrackName(TrackName);
                CurrentPlayerState.SoundName = SoundName;

                return CurrentPlayerState;
            }
            else if (State == GameStateType.Location || State == GameStateType.CritOnLocation) {
                var location = Quest.Locations.FirstOrDefault(x => x.Id == LocationId);
                if (location == null) {
                    throw new Exception($"Internal error: no state loc id={LocationId}");
                }

                int locTextId = CalculateLocationShowingTextId(location);
                string locationOwnText = location.Texts.ElementAtOrDefault(locTextId) ?? string.Empty;

                var lastJump = Quest.Jumps.FirstOrDefault(x => x.Id == LastJumpId);
                if (lastJump != null) {
                    Shared.Log("-----------------------\n Прыжок: " + lastJump.Description);
                    Shared.Log(string.IsNullOrEmpty(lastJump.Description));
                }
                string text = (location.IsEmpty && lastJump != null && !string.IsNullOrEmpty(lastJump.Description)) ? lastJump.Description : locationOwnText;
                CurrentPlayerState.Text = Replace(text);
                CurrentPlayerState.ParamsState = GetParamsState();
                CurrentPlayerState.Choices = State == GameStateType.Location
                    ? location.IsFaily || location.IsFailyDeadly
                        ? new List<PlayerChoice>()
                        : location.IsSuccess
                            ? new List<PlayerChoice>
                            {
                                new PlayerChoice
                                {
                                    JumpId = Constants.JUMP_GO_BACK_TO_SHIP,
                                    Text = texts.GoBackToShip,
                                    Active = true,
                                },
                            }
                            : PossibleJumps.Select(x => {
                                var jump = Quest.Jumps.FirstOrDefault(y => y.Id == x.Id);
                                if (jump == null) {
                                    throw new Exception($"Internal error: no jump {x.Id} in possible jumps");
                                }
                                return new PlayerChoice {
                                    Text = Replace(jump.Text) ?? texts.Next,
                                    JumpId = (int)x.Id,
                                    Active = (bool)x.active,
                                };
                            }).ToList()
                    : new List<PlayerChoice>
                    {
                        new PlayerChoice
                        {
                            // critonlocation
                            JumpId = Constants.JUMP_NEXT,
                            Text = texts.Next,
                            Active = true,
                        },
                    };
                CurrentPlayerState.GameState = location.IsFailyDeadly
                    ? PlayerGameStateType.Dead
                        : location.IsFaily
                                ? PlayerGameStateType.Fail
                                    : PlayerGameStateType.Running;
                CurrentPlayerState.ImageName = ImageName;
                CurrentPlayerState.TrackName = ReplaceSpecialTrackName(TrackName);
                CurrentPlayerState.SoundName = SoundName;
                return CurrentPlayerState;
            }
            else if (State == GameStateType.CritOnJump) {
                int critId = (int)CritParamId;
                var jump = Quest.Jumps.FirstOrDefault(x => x.Id == LastJumpId);

                if (critId == 0 || jump == null) {
                    throw new Exception($"Internal error: crit={critId} lastjump={LastJumpId}");
                }
                var param = Quest.QMParams[critId];
                CurrentPlayerState.Text = Replace(jump.ParamsChanges[critId].CritText ?? Quest.QMParams[critId].CritValueString);
                CurrentPlayerState.ParamsState = GetParamsState();
                CurrentPlayerState.Choices = param.Type == ParamType.Успешный
                        ? new List<PlayerChoice>
                        {
                            new PlayerChoice
                            {
                                JumpId = Constants.JUMP_GO_BACK_TO_SHIP,
                                Text = texts.GoBackToShip,
                                Active = true,
                            },
                        }
                        : new List<PlayerChoice>();
                CurrentPlayerState.GameState = param.Type == ParamType.Успешный
                    ? PlayerGameStateType.Running
                    : param.Type == ParamType.Провальный
                        ? PlayerGameStateType.Fail
                        : PlayerGameStateType.Dead;
                CurrentPlayerState.ImageName = ImageName;
                CurrentPlayerState.TrackName = ReplaceSpecialTrackName(TrackName);
                CurrentPlayerState.SoundName = SoundName;
                return CurrentPlayerState;
            }
            else if (State == GameStateType.JumpAndNextCrit) {
                int critId = (int)CritParamId;
                var jump = Quest.Jumps.FirstOrDefault(x => x.Id == LastJumpId);
                if (critId == 0 || jump == null) {
                    throw new Exception($"Internal error: crit={critId} lastjump={LastJumpId}");
                }

                CurrentPlayerState.Text = Replace(jump.Description);
                CurrentPlayerState.ParamsState = GetParamsState();
                CurrentPlayerState.Choices = new List<PlayerChoice>
                {
                new PlayerChoice
                    {
                        JumpId = Constants.JUMP_NEXT,
                        Text = texts.Next,
                        Active = true,
                    },
                };
                CurrentPlayerState.GameState = PlayerGameStateType.Running;
                CurrentPlayerState.ImageName = ImageName;
                CurrentPlayerState.TrackName = ReplaceSpecialTrackName(TrackName);
                CurrentPlayerState.SoundName = SoundName;
                return CurrentPlayerState;
            }
            else if (State == GameStateType.CritOnLocationLastMessage) {
                int critId = (int)CritParamId;
                var location = Quest.Locations.FirstOrDefault(x => x.Id == LocationId);

                if (critId == 0) {
                    throw new Exception("Internal error: no critId");
                }
                if (location == null) {
                    throw new Exception($"Internal error: no crit state location {LocationId}");
                }
                var param = Quest.QMParams[critId];

                CurrentPlayerState.Text = Replace(location.ParamsChanges[critId].CritText ?? Quest.QMParams[critId].CritValueString);
                CurrentPlayerState.ParamsState = GetParamsState();
                CurrentPlayerState.Choices = param.Type == ParamType.Успешный
                    ? new List<PlayerChoice>
                    {
                        new PlayerChoice
                        {
                            JumpId = Constants.JUMP_GO_BACK_TO_SHIP,
                            Text = texts.GoBackToShip,
                            Active = true,
                        },
                    }
                    : new List<PlayerChoice>();
                CurrentPlayerState.GameState = param.Type == ParamType.Успешный
                    ? PlayerGameStateType.Running
                    : param.Type == ParamType.Провальный
                        ? PlayerGameStateType.Fail
                        : PlayerGameStateType.Dead;
                CurrentPlayerState.ImageName = ImageName;
                CurrentPlayerState.TrackName = ReplaceSpecialTrackName(TrackName);
                CurrentPlayerState.SoundName = SoundName;
                return CurrentPlayerState;
            }
            else if (State == GameStateType.ReturnedEnding) {
                CurrentPlayerState.Text = Replace(Quest.SuccessText);
                CurrentPlayerState.ParamsState = new List<string>();
                CurrentPlayerState.Choices = new List<PlayerChoice>();
                CurrentPlayerState.GameState = PlayerGameStateType.Win;
                CurrentPlayerState.ImageName = (string)null;
                CurrentPlayerState.TrackName = (string)null;
                CurrentPlayerState.SoundName = (string)null;
                return CurrentPlayerState;
            }
            else {
                throw new Exception($"Unexpected object: {State}");
            }
        }

        public string Replace(string str, int? diamondIndex = null) {
            CurrentPlayer.Day = $"{Constants.DEFAULT_DAYS_TO_PASS_QUEST - DaysPassed}";
            CurrentPlayer.Date = SRDateToString(Constants.DEFAULT_DAYS_TO_PASS_QUEST, CurrentPlayer.Lang);
            CurrentPlayer.CurDate = SRDateToString(DaysPassed, CurrentPlayer.Lang);

            return Substitute(str, diamondIndex);
        }

        public string Substitute(string str,int? diamondIndex = null) {
            if (diamondIndex.HasValue) {
                str = str.Replace("<>", $"[p{diamondIndex.Value + 1}]");
            }

            int searchPosition = 0;
            while (true) {
                int dIndex = str.IndexOf("[d", searchPosition);
                if (dIndex == -1) {
                    break;
                }
                string paramIndexStr = string.Empty;
                int scanIndex = dIndex + 2;

                while (true) {
                    char currentChar = str[scanIndex];
                    if ("0123456789".IndexOf(currentChar) == -1) {
                        break;
                    }
                    paramIndexStr += currentChar;
                    scanIndex++;
                }

                if (string.IsNullOrEmpty(paramIndexStr)) {
                    Shared.Warn($"No param index found in '{str}' at {dIndex}");
                    searchPosition = scanIndex;
                    continue;
                }
                int paramIndex = int.Parse(paramIndexStr) - 1;

                int? paramValue = ParamValues[paramIndex];

                if (paramValue == null) {
                    scanIndex++;
                    str = str.Substring(0, dIndex) + Constants.CRL + "UNKNOWN_PARAM" + Constants.CRLEND + str.Substring(scanIndex);
                    continue;
                }

                if (str[scanIndex] == ']') {
                    // Just keep param value as is
                    scanIndex++;
                }
                else if (str[scanIndex] == ':') {
                    // Replace param value with formula
                    scanIndex++;
                    int formulaStartIndex = scanIndex;
                    int formulaEndIndex = formulaStartIndex;
                    while (str[scanIndex] == ' ') {
                        scanIndex++;
                    }

                    // And here goes the formula parsing
                    // TODO: Use Parse() method without throwing errors
                    // So, Parse() should read the expression and return the index where it ends
                    // Now we're just using naive implementation and counting square brackets
                    int squareBracketsCount = 0;
                    while (true) {
                        if (str[scanIndex] == '[') {
                            squareBracketsCount++;
                        }
                        else if (str[scanIndex] == ']') {
                            if (squareBracketsCount == 0) {
                                formulaEndIndex = scanIndex;
                                scanIndex++;
                                break;
                            }
                            else {
                                squareBracketsCount--;
                            }
                        }
                        scanIndex++;
                        if (scanIndex > str.Length) {
                            Console.WriteLine($"No closing bracket found in '{str}' at {formulaStartIndex}");
                            break;
                        }
                    }
                    string formulaWithMaybeCurlyBrackets = str.Substring(formulaStartIndex, formulaEndIndex - formulaStartIndex);

                    string formula = formulaWithMaybeCurlyBrackets;
                    Match insideCurlyBracketsMatch = Regex.Match(
                        formulaWithMaybeCurlyBrackets, @"\s*\{(.*)\}\s*");
                    if (insideCurlyBracketsMatch.Success) {
                        formula = insideCurlyBracketsMatch.Groups[1].Value;
                    }

                    paramValue = Formula.Calculate(formula, ParamValues, random);
                }
                else {
                    Console.WriteLine($"Unknown symbol in '{str}' at {scanIndex}");
                    break;
                }

                // TODO: This is very similar to GetParamsState function, maybe better to refactor
                foreach (var range in Quest.QMParams[paramIndex].ShowingInfo) {
                    if (paramValue >= range.From && paramValue <= range.To) {
                        string paramString = Substitute(range.Str, diamondIndex);
                        str = str.Substring(0, dIndex) + Constants.CRL + paramString + Constants.CRLEND + str.Substring(scanIndex);
                        break;
                    }
                }

                searchPosition = scanIndex;
            }

            while (true) {
                Match m = Regex.Match(str, @"\{[^}]*\}");
                if (!m.Success) {
                    break;
                }
                string formulaWithBrackets = m.Value;
                int result = Formula.Calculate(
                    formulaWithBrackets.Substring(1, formulaWithBrackets.Length - 2), ParamValues, random
                );

                str = str.Replace(formulaWithBrackets, $"{Constants.CRL}{result}{Constants.CRLEND}");
            }

            foreach (string k in Constants.PLAYER_KEYS_TO_REPLACE) {
                var prop = CurrentPlayer.GetType().GetProperty(k).GetValue(CurrentPlayer, null);
                Shared.Log(prop);
                str = str.Replace($"<{k}>", Constants.CRL
                    + CurrentPlayer.GetType().GetProperty(k).GetValue(CurrentPlayer, null).ToString()
                    + Constants.CRLEND);
            }

            for (int ii = 0; ii < ParamValues.Count(); ii++) {
                while (str.IndexOf($"[p{ii + 1}]") > -1) {
                    str = str.Replace($"[p{ii + 1}]", $"{Constants.CRL}{ParamValues[ii]}{Constants.CRLEND}");
                }
            }

            return str;
        }

        public List<string> GetParamsState() {
            List<string> paramsState = new List<string>();
            for (int i = 0; i < Quest.ParamsCount; i++) {
                if (ParamShow[i] && Quest.QMParams[i].Active) {
                    int val = ParamValues[i];
                    var param = Quest.QMParams[i];
                    if (val != 0 || param.ShowWhenZero) {
                        foreach (var range in param.ShowingInfo) {
                            if (val >= range.From && val <= range.To) {
                                string str = Replace(range.Str, i);
                                paramsState.Add(str);
                                break;
                            }
                        }
                    }
                }
            }
            return paramsState;
        }
    }
}