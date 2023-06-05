namespace SRQ {
    public enum GameStateType {
        Starting,
        Location,
        Jump, // ���� ������� � ��������� � ��������� ������� �� ������
        JumpAndNextCrit, // ���� ������� � ��������� � ��������� ������� �� ������, � ��������� �������� ����������
        CritOnLocation, // �������� ���� ��������� �� �������, �������� ������ ���� ������� �����
        CritOnLocationLastMessage, // �������� ���� ��������� �� �������, ������������ ��������� ���������
        CritOnJump,  // �������� ���� ��������� �� �������� ��� ��������
        ReturnedEnding
    }

    public enum PlayerGameStateType {
        Running,
        Fail,
        Win,
        Dead
    }
}