using System;

namespace CS3110_Module_8_Group
{
    class Battleship : Ship
    {
        public Battleship() : base(4, ConsoleColor.DarkGreen, ShipTypes.Battleship)
        {
        }

        public override bool IsBattleShip => true;
    }
}
