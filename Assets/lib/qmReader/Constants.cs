namespace SRQ {

    // Gladiator: ......C8
    // Ivan:      ......D0
    // FullRing   ......CA
    // Jump       00000000
    //

    static partial class Constants {
        public const int LOCATION_TEXTS = 10;
        public const int HEADER_QM_2 = 0x423a35d2; // 24 parameters
        public const int HEADER_QM_3 = 0x423a35d3; // 48 parameters
        public const int HEADER_QM_4 = 0x423a35d4; // 96 parameters
        public const int HEADER_QMM_6 = 0x423a35d6;
        public const int HEADER_QMM_7 = 0x423a35d7;

        /**
         *
         * This is a workaround to tell player to keep old TGE behavior if quest is
         * resaved as new version.
         *
         *
         * 0x423a35d7 = 1111111127
         * 0x69f6bd7  = 0111111127
         */

        public const int HEADER_QMM_7_WITH_OLD_TGE_BEHAVIOUR = 0x69f6bd7;
    }
}
