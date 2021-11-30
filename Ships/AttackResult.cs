using System;

namespace CS3110_Module_8_Group
{
    public struct AttackResult
    {
        public int PlayerIndex;
        public Position Position;
        public AttackResultType ResultType;
        public ShipTypes SunkShip; //Filled in if ResultType is Sunk

        public AttackResult(int playerIndex, Position position, AttackResultType attackResultType= AttackResultType.Miss, ShipTypes sunkShip = ShipTypes.None)
        {
            PlayerIndex = playerIndex;
            Position = position;
            ResultType = attackResultType;
            SunkShip = sunkShip;
        }
    }
}
