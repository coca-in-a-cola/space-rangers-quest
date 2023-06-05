namespace SRQ {
    public enum GameStateType {
        Starting,
        Location,
        Jump, // Если переход с описанием и следующая локация не пустая
        JumpAndNextCrit, // Если переход с описанием и следующая локация не пустая, и параметры достигли критичного
        CritOnLocation, // Параметр стал критичным на локации, доступен только один переход далее
        CritOnLocationLastMessage, // Параметр стал критичным на локации, показывается сообщение последнее
        CritOnJump,  // Параметр стал критичным на переходе без описания
        ReturnedEnding
    }

    public enum PlayerGameStateType {
        Running,
        Fail,
        Win,
        Dead
    }
}