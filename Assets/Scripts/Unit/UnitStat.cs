using UnityEngine;

public class UnitStat
{
    private int _unitID;
    private int _starLevel;
    private UnitStatData _data;

    public UnitStat(int unitID, int starLevel)
    {
        _unitID = unitID;
        _starLevel = starLevel;
        LoadUnitStatData(_unitID, _starLevel);
        Debug.Log($"UnitId={_data.unitID}, ★{_data.starLevel}, HP={_data.health}, ATK={_data.attack:F1}");
    }

    private void LoadUnitStatData(int unitID, int starLevel)
    {
        var entity = DB_UnitStats.FindEntity((f) => f.f_UnitId == unitID && f.f_StarLevel == starLevel);
        _data = new UnitStatData(entity);
    }
}
