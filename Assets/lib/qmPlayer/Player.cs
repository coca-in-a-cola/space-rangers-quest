namespace SRQ {
    public class Player {
        public string Ranger { get; set; }
        public string PlayerName { get; set; }
        public string Money { get; set; }
        public string FromPlanet { get; set; }
        public string FromStar { get; set; }
        public string ToPlanet { get; set; }
        public string ToStar { get; set; }
        public string Lang { get; set; }
        public bool? AllowBackButton { get; set; }

        /** ���� �������� */
        public string Date { get; set; }
        /**  ���-�� ���� */
        public string Day { get; set; }
        /**  ������� ���� */
        public string CurDate {get; set;}



        public static Player DEFAULT_RUS_PLAYER => new Player {
            Ranger = "����",
            PlayerName = "����",
            FromPlanet = "�����",
            FromStar = "���������", 
            ToPlanet = "��������",
            ToStar = "�������",
            Money = "10000",
            Lang = "rus",
            AllowBackButton = false,

        };

        public static Player DEFAULT_ENG_PLAYER => new Player {
            Ranger = "Ranger",
            PlayerName = "Player",
            FromPlanet = "FromPlanet",
            FromStar = "FromStar",
            ToPlanet = "ToPlanet",
            ToStar = "ToStar",
            Money = "10000",
            Lang = "eng",
            AllowBackButton = false
        };
    }
}