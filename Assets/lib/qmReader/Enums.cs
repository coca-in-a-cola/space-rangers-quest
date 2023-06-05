namespace SRQ {

    public enum ParameterChangeType {
        Value = 0x00,
        Summ = 0x01,
        Percentage = 0x02,
        Formula = 0x03,
    }

    public enum ParameterShowingType {
        НеТрогать = 0x00,
        Показать = 0x01,
        Скрыть = 0x02,
    }


    public enum LocationType {
        Ordinary = 0x00,
        Starting = 0x01,
        Empty = 0x02,
        Success = 0x03,
        Faily = 0x04,
        Deadly = 0x05,
    }

    public enum ParamType {
        Обычный = 0,
        Провальный = 1,
        Успешный = 2,
        Смертельный = 3,
    }

    public enum ParamCritType {
        Максимум = 0,
        Минимум = 1,
    }

    public enum PlayerRace {
        Малоки = 1,
        Пеленги = 2,
        Люди = 4,
        Феяне = 8,
        Гаальцы = 16,
    }

    public enum PlanetRace {
        Малоки = 1,
        Пеленги = 2,
        Люди = 4,
        Феяне = 8,
        Гаальцы = 16,
        Незаселенная = 64,
    }

    public enum WhenDone {
        OnReturn = 0,
        OnFinish = 1,
    }

    public enum PlayerCareer {
        Торговец = 1,
        Пират = 2,
        Воин = 4,
    }
}