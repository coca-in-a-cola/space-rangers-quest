//using System;
//using System.Linq;
//using System.Collections.Generic;

//namespace SRQ
//{
//    public class Quest : QM
//    {
//        // Implementation here
//    }

//    public class GameLogStep
//    {
//        public int DateUnix { get; set; }
//        public int JumpId { get; set; }
//    }

//    public class GameLog
//    {
//        public string AleaSeed { get; set; }
//        public List<GameLogStep> PerformedJumps { get; set; }
//    }

//    public class GameState
//    {
//        // Properties and constructors here
//    }

//    public class PlayerChoice
//    {
//        public string Text { get; set; }
//        public int JumpId { get; set; }
//        public bool Active { get; set; }
//    }

//    public class PlayerState
//    {
//        public string Text { get; set; }
//        public string ImageName { get; set; }
//        public string TrackName { get; set; }
//        public string SoundName { get; set; }
//        public List<string> ParamsState { get; set; }
//        public List<PlayerChoice> Choices { get; set; }
//        public string GameState { get; set; }
//    }

//    public GameState InitGame(Quest quest, string seed)
//    {
//        Alea alea = new Alea(seed);

//        var startLocation = quest.Locations.FirstOrDefault(x => x.IsStarting);
//        if (startLocation == null)
//        {
//            throw new Exception("No start location!");
//        }

//        var startingParams = quest.Params.Select((param, index) =>
//        {
//            if (!param.Active)
//            {
//                return 0;
//            }
//            if (param.IsMoney)
//            {
//                int giveMoney = 2000;
//                int money = param.Max > giveMoney ? giveMoney : param.Max;
//                string starting = $"[{money}]";
//                return Calculate(starting, new int[0], alea.Random);
//            }
//            return Calculate(param.Starting.Replace("h", ".."), new int[0], alea.Random);
//        }).ToList();

//        var startingShowing = quest.Params.Select(_ => true).ToList();

//        GameState state = new GameState
//        {
//            State = "starting",
//            LocationId = startLocation.Id,
//            LastJumpId = null,
//            CritParamId = null,
//            PossibleJumps = new List<int>(),
//            ParamValues = startingParams,
//            ParamShow = startingShowing,
//            JumpedCount = new Dictionary<int, int>(),
//            LocationVisitCount = new Dictionary<int, int>(),
//            DaysPassed = 0,
//            ImageName = null,
//            TrackName = null,
//            SoundName = null,
//            AleaState = alea.ExportState(),
//            AleaSeed = seed,
//            PerformedJumps = new List<int>()
//        };

//        return state;
//    }

//    public static readonly string TRACK_NAME_RESET_DEFAULT_MUSIC = "Quest";

//    public string SRDateToString(
//        int daysToAdd,
//        Lang lang,
//        DateTime initialDate = default(DateTime)  // TODO: use it
//    )
//    {
//        // Implementation here
//    }

//    public string Replace(
//        string str,
//        Quest quest,
//        GameState state,
//        Player player,
//        int? diamondIndex,
//        Random random
//    )
//    {
//        // Implementation here
//    }

//    public List<string> GetParamsState(Quest quest, GameState state, Player player, Random random)
//    {
//        var paramsState = new List<string>();
//        for (int i = 0; i < quest.ParamsCount; i++)
//        {
//            if (state.ParamShow[i] && quest.Params[i].Active)
//            {
//                int val = state.ParamValues[i];
//                var param = quest.Params[i];
//                if (val != 0 || param.ShowWhenZero)
//                {
//                    foreach (var range in param.ShowingInfo)
//                    {
//                        if (val >= range.From && val <= range.To)
//                        {
//                            var str = Replace(range.Str, quest, state, player, i, random);
//                            paramsState.Add(str);
//                            break;
//                        }
//                    }
//                }
//            }
//        }
//        return paramsState;
//    }

//}