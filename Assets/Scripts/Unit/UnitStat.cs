using UnityEngine;

public class UnitStat
{
    private int _unitID;
    private int _starLevel;
    private UnitStatData _data;

    public UnitStatData Data => _data;

    public UnitStat(int unitID, int starLevel)
    {
        _unitID = unitID;
        _starLevel = starLevel;
        LoadUnitStatData(_unitID, _starLevel);
    }

    public void LevelUpUnitStat(int starLevel)
    {
        _starLevel = starLevel;
        LoadUnitStatData(_unitID, _starLevel);
    }

    private void LoadUnitStatData(int unitID, int starLevel)
    {
        var entity = DB_UnitStats.FindEntity((f) => f.f_UnitId == unitID && f.f_StarLevel == starLevel);
        _data = new UnitStatData(entity);
        Debug.Log($"UnitId={_data.UnitId}, ★{_data.StarLevel}, HP={_data.Health}, ATK={_data.Attack:F1}");
    }
}
