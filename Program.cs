// Multiplayer Battleship Game with AI - Partial Solution


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS3110_Module_8_Group
{
    class Program
    {
        static void Main(string[] args)
        {
            List<IPlayer> players = new List<IPlayer>();
            //players.Add(new DumbPlayer("Dumb 1"));
            //players.Add(new DumbPlayer("Dumb 2"));
            //players.Add(new DumbPlayer("Dumb 3"));
            //..players.Add(new RandomPlayer("Random 1"));
            //players.Add(new RandomPlayer("Random 2"));
            //players.Add(new RandomPlayer("Random 3"));
            //players.Add(new RandomPlayer("Random 4"));
            players.Add(new RandomPlayer("Random 5"));

            players.Add(new SmartPlayer("Smart 1"));

            //Your code here
            //players.Add(new GroupNPlayer());

            MultiPlayerBattleShip game = new MultiPlayerBattleShip(players);
            game.Play(PlayMode.Pause);
        }
    }
}
