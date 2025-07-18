public interface IUnitRepository
{
    UnitAggregate Get(int unitId, int starLevel);
    bool Exists(int unitId);
    int GetMaxId();
}
