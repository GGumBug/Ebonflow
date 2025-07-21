namespace DeckSystem
{
    public struct CardData
    {
        public UnitTier tier;
        public int price;
        public int unitID;
        public int starLevel;

        public CardData(UnitTier tier, int price, int unitID, int starLevel)
        {
            this.tier = tier;
            this.price = price;
            this.unitID = unitID;
            this.starLevel = starLevel;
        }
    }
}