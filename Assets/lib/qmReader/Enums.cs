namespace SRQ {

    public enum ParameterChangeType {
        Value = 0x00,
        Summ = 0x01,
        Percentage = 0x02,
        Formula = 0x03,
    }

    public enum ParameterShowingType {
        ��������� = 0x00,
        �������� = 0x01,
        ������ = 0x02,
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
        ������� = 0,
        ���������� = 1,
        �������� = 2,
        ����������� = 3,
    }

    public enum ParamCritType {
        �������� = 0,
        ������� = 1,
    }

    public enum PlayerRace {
        ������ = 1,
        ������� = 2,
        ���� = 4,
        ����� = 8,
        ������� = 16,
    }

    public enum PlanetRace {
        ������ = 1,
        ������� = 2,
        ���� = 4,
        ����� = 8,
        ������� = 16,
        ������������ = 64,
    }

    public enum WhenDone {
        OnReturn = 0,
        OnFinish = 1,
    }

    public enum PlayerCareer {
        �������� = 1,
        ����� = 2,
        ���� = 4,
    }
}