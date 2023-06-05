namespace SRQ {
    static partial class Constants {
        public const int JUMP_I_AGREE = -1;
        public const int JUMP_NEXT = -2;
        public const int JUMP_GO_BACK_TO_SHIP = -3;

        public const int DEFAULT_DAYS_TO_PASS_QUEST = 35;
        public const string TRACK_NAME_RESET_DEFAULT_MUSIC = "Quest";

        public const string CRL = "<clr>";
        public const string CRLEND = "<clrEnd>";

        public static string[] PLAYER_KEYS_TO_REPLACE = {
          "Ranger",
          "PlayerName",
          "FromPlanet",
          "FromStar",
          "ToPlanet",
          "ToStar",
          "Money",
          "Date",
          "Day",
          "CurDate"
        }; // TODO: Maybe move from here

    }
}
