using System;

namespace SRQ {
    public class QMPlayer {
        private Player player;
        private GameState state;
        private readonly QM quest;
        private readonly string lang;

        public QMPlayer(QM quest, string lang) {
            this.quest = quest;
            this.lang = lang;
            this.player = lang == "rus" ? Player.DEFAULT_RUS_PLAYER : Player.DEFAULT_ENG_PLAYER;
        }

        public void Start() {
            this.state = new GameState(this.quest, new Random().Next().ToString("x"), player);
        }

        public PlayerState GetState() {
            return state.GetUIState();
        }

        public void PerformJump(int jumpId) {
            state.PerformJump(jumpId);
        }

        public GameState GetSaving() {
            return this.state;
        }

        public void LoadSaving(GameState state) {
            this.state = state;
        }
    }
}