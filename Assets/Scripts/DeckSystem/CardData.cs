namespace DeckSystem
{
    public struct CardData
    {
        public UnitTier tier;
        public int price;
        public int unitID;

        public CardData(UnitTier tier, int price, int unitID)
        {
            this.tier = tier;
            this.price = price;
            this.unitID = unitID;
        }
    }
}