using System;

[Serializable]
public class UnitData
{
    public int UnitId { get; }
    public string UnitCode { get; }
    public int NameKey { get; }
    public UnitTier UnitTier { get; }

    public UnitData(DB_Units e)
    {
        if (e == null)
            throw new ArgumentNullException(nameof(e));

        UnitId = e.f_UnitId;
        UnitCode = e.f_UnitCode;
        NameKey = e.f_NameKey;
        UnitTier = e.f_UnitTier;
    }
}
