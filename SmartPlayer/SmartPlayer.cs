/* CS3110 Module 8 Group Game
 * Group 1: Ryan Leftridge, Jacob McMahon, Jacob Swoffer
 * This program implements an AI that can play the game of Battleship.
 */


using System;
using System.Data;
using System.Diagnostics; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CS3110_Module_8_Group
{
    internal class SmartPlayer : IPlayer
    {
        //private List<AttackResult> attackLog = new List<AttackResult>(); Unsure about the need of this yet
        private List<Position> previouslyAttackedPositions = new List<Position>();
        private List<Position> positions_to_ignore = new List<Position>();
        private static readonly List<Position> Guesses = new List<Position>();
        private Position previousPosition = null;
        private Position originalPosition = null;
        private Direction direction = Direction.Horizontal;
        private bool shouldReverse = false;

        private int gridSize;
        private int index;
        private int invalidCounter = 0;
        private int battleship_length;
        private static readonly Random Random = new Random();

        public int Index => index;

        private string name;

        public string Name
        {
            get
            {
                return name;
            }
        }

        public SmartPlayer(string name)
        {
            this.name = name;
        }

        /**
         * Initializes all of the AI data and places the ships on the grid
         */
        public void StartNewGame(int playerIndex, int gridSize, Ships ships)
        {

            this.index = playerIndex;
            this.gridSize = gridSize;
            var occupiedPositions = new List<Position>();
            bool firstShipPlaced = false;
            bool canPlace = true;

            GenerateGuesses();

            // This method generates a random direction and x and y coordinate for each ship.
            // If any of that ship's generated Positions overlap with one that was previously generated successfully,
            // then it starts over and generates a new direction and x and y coordinate. It loops until all the Positions are unique.

            foreach (var ship in ships._ships)
            {

                if (ship.IsBattleShip)
                {
                    this.battleship_length = ship.Length;
                }

                // when an overlap is detected, loops back here to generate another set of random Positions
                while (true)
                {
                    // randomly decide if ship should be placed horizontally or vertically
                    Direction direction = new Direction();
                    int randomDirection = Random.Next(2);
                    if (randomDirection == 0)
                    {
                        direction = Direction.Horizontal;
                    }
                    else
                    {
                        direction = Direction.Vertical;
                    }

                    // choose an X and Y based on the ship length and grid size so it always fits
                    var x = Random.Next(gridSize - ship.Length);
                    var y = Random.Next(gridSize - ship.Length);

                    // use Place to generate the ship's Positions.
                    ship.Place(new Position(x, y), direction);

                    // skip overlap validation the first time a ship is placed
                    if (firstShipPlaced == false)
                    {
                        firstShipPlaced = true;
                        break;
                    }

                    // validate that none of ship's Positions overlap existing ones
                    foreach (Position shipPos in ship.Positions)
                    {
                        foreach (Position occupiedPos in occupiedPositions)
                        {
                            if (shipPos.X == occupiedPos.X && shipPos.Y == occupiedPos.Y)
                            {
                                canPlace = false;
                                break;
                            }
                            else
                                canPlace = true;
                        }
                        if (canPlace == false)
                            break;
                    }
                    if (canPlace == true)
                        break;
                    else
                        continue;
                }

                // no overlap detected, so add all the ship's Positions to the list of occupiedPositions
                foreach (Position position in ship.Positions)
                {
                    occupiedPositions.Add(position);
                }
            }
        }

        private void GenerateGuesses()
        {
            //We want all instances of RandomPlayer to share the same pool of guesses
            //So they don't repeat each other.

            //We need to populate the guesses list, but not for every instance - so we only do it if the set is missing some guesses
            if (Guesses.Count < gridSize * gridSize)
            {
                Guesses.Clear();
                for (int x = 0; x < gridSize; x++)
                {
                    for (int y = 0; y < gridSize; y++)
                    {
                        Guesses.Add(new Position(x, y));
                    }
                }
            }
        }

        /**
         * Returns the next attack position that is calculated by the AI
         */
        public Position GetAttackPosition()
        {
            Position attackTarget = null;

            if (previousPosition != null)
            {
                if (originalPosition == null)
                {
                    originalPosition = previousPosition; // This check will only happen when the AI hits a target for the first time
                }
                while (attackTarget == null)
                {
                    //While the attackTarget is null keep trying to acquire a new attack position
                    attackTarget = GetNextAttackPosition(previousPosition, direction);
                    if (invalidCounter > 4)
                    {
                        attackTarget = RandomPosition();
                        invalidCounter = 0;
                    }
                }
            }
            else
            {
                attackTarget = RandomPosition();
            }
            if (IsHit(attackTarget)) {
                attackTarget = RandomPosition();
            }
            if (previousPosition == null)
            {
                previousPosition = attackTarget;
            }
            previouslyAttackedPositions.Add(attackTarget);
            return attackTarget;
        }

        /**
         * Handles all of the possible results of the previous attack. Switches the AI targeting state accordingly for each possible
         * attack result
         */
        public void SetAttackResults(List<AttackResult> results)
        {
            //Defaulting lastAttack to a generic value in order to create a default variable
            AttackResult lastAttack = new AttackResult(-1, null, AttackResultType.Miss, ShipTypes.None);

            foreach (AttackResult result in results)
            {
                if (result.PlayerIndex == index)
                {
                    continue; // Ignores attacks it lands on itself
                }
                if (previousPosition != null)
                {
                    if (result.Position.X - previousPosition.X == 0 && result.Position.Y - previousPosition.Y == 0)
                    {
                        lastAttack = result; // Finds the resulting attack that is the same as the previousAttack variable
                    }
                }
            }

            if (lastAttack.PlayerIndex == -1)
            {
                //Something went wrong if this happens
                return;
            }
            else
            {
                if (lastAttack.ResultType == AttackResultType.Hit)
                {
                    previousPosition = lastAttack.Position; // If the attack hits, move the previousPosition to the new attack
                }
                else if (lastAttack.ResultType == AttackResultType.Miss && !shouldReverse && originalPosition != null)
                {
                    previousPosition = originalPosition; // If the attack misses and is not already set to reverse, return the pointing position to the original position and reverse
                    shouldReverse = true;
                }
                else if (lastAttack.ResultType == AttackResultType.Miss && !shouldReverse && originalPosition == null)
                {
                    previousPosition = null; // If the attack misses and it hasn't already hit something, set previous position to null
                }
                else if (lastAttack.ResultType == AttackResultType.Miss && shouldReverse)
                {
                    // If the attack misses and it is already set to reverse this means we have checked both sides of the original point and the only possible answer is that the other ship positions are along the other axis
                    direction = (direction == Direction.Horizontal) ? Direction.Vertical : Direction.Horizontal;
                    previousPosition = originalPosition;
                    shouldReverse = false;
                }
                else if (lastAttack.ResultType == AttackResultType.Sank)
                {
                    // If the attack sank the target, reset the targeting state to the default values
                    previousPosition = null;
                    originalPosition = null;
                    shouldReverse = false;
                    direction = Direction.Horizontal;
                }
            }
        }

        /**
         * Returns an offset position based on the position passed in, the direction, and the shouldReverse variable
         */
        private Position GetNextAttackPosition(Position position, Direction direction)
        {
            int offset = (shouldReverse) ? -1 : 1;
            Position newPos = position;
            do 
            {
                if (direction == Direction.Horizontal)
                {
                    newPos = new Position(newPos.X + offset, newPos.Y);
                }
                else
                {
                    newPos = new Position(newPos.X, newPos.Y + offset);
                }
                if (!IsValid(newPos))
                {
                    previousPosition = originalPosition;
                    shouldReverse = true;
                    return null;
                }
                else
                {
                    previousPosition = newPos;
                }
            }while (!IsValid(newPos) || IsHit(newPos));
            return newPos;
        }

        /**
         * This method returns if the position is out of bounds. It will also increment a counter that will act as a failsafe for an
         * infinite loop
         */
        private bool IsValid(Position position)
        {
            if (position.X > gridSize - 1 || position.Y > gridSize - 1 || position.X < 0 || position.Y < 0)
            {
                invalidCounter++;
                return false;
            }
            return true;
        }

        /**
         * This method returns if the position has already been hit by this AI. It is not able to detect what positions
         * are hit by other AI
         */
        private bool IsHit(Position position)
        {
            foreach (Position pos in previouslyAttackedPositions)
            {
                if (pos.X == position.X && pos.Y == position.Y)
                {
                    return true;
                }
            }
            return false;
        }

        private enum check_dir
        {
            Left, 
            Right,
            Up,
            Down
        }

        private Boolean check_position(Position pos, Boolean attack_mode = false) //Will check if a position is a good choice based on if a battle ship can fit in adjancent spots
        {
            int count = 1;
            check_dir check_direction = check_dir.Left; 
            Position temp_pos = pos; 
            while (count < this.battleship_length)
            {
                if (check_direction == check_dir.Left)
                {
                    temp_pos = new Position(temp_pos.X - 1,temp_pos.Y); 
                    if (IsValid(temp_pos) && !(previouslyAttackedPositions.Contains(temp_pos) || positions_to_ignore.Contains(temp_pos))) //Check if position is valid and it is not part of previous attacks or position to ignore 
                    {
                        count++;
                    }
                    else
                    {
                        temp_pos = pos; 
                        check_direction = check_dir.Right; 
                    }
                }
                else if (check_direction == check_dir.Right)
                {
                    temp_pos = new Position(temp_pos.X + 1, temp_pos.Y);
                    if (IsValid(temp_pos) && !(previouslyAttackedPositions.Contains(temp_pos) || positions_to_ignore.Contains(temp_pos))) //Check if position is valid and it is not part of previous attacks or position to ignore 
                    {
                        count++;
                    }
                    else
                    {
                        temp_pos = pos;
                        check_direction = check_dir.Up;
                        count = 1; //Have to disregard count from horizontal and reset since ship can't bend 
                    }
                }
                else if (check_direction == check_dir.Up)
                {
                    temp_pos = new Position(temp_pos.X, temp_pos.Y + 1);
                    if (IsValid(temp_pos) && !(previouslyAttackedPositions.Contains(temp_pos) || positions_to_ignore.Contains(temp_pos))) //Check if position is valid and it is not part of previous attacks or position to ignore 
                    {
                        count++;
                    }
                    else
                    {
                        temp_pos = pos;
                        check_direction = check_dir.Down;
                    }
                }
                else if (check_direction == check_dir.Down)
                {
                    temp_pos = new Position(temp_pos.X, temp_pos.Y - 1);
                    if (IsValid(temp_pos) && !(previouslyAttackedPositions.Contains(temp_pos) || positions_to_ignore.Contains(temp_pos))) //Check if position is valid and it is not part of previous attacks or position to ignore 
                    {
                        count++;
                    }
                    else
                    {
                        positions_to_ignore.Add(pos);
                        return false; 
                    }
                } 


            }
            return true;

        }

        /**
         * Returns a random position that has not already been attacked by this AI yet
         */
        private Position RandomPosition()
        {
            Random random = new Random();
            Position pos = new Position(random.Next(gridSize), random.Next(gridSize));

            while (previouslyAttackedPositions.Contains(pos) || positions_to_ignore.Contains(pos))
            {

                pos = new Position(random.Next(gridSize), random.Next(gridSize));
                check_position(pos); 

            }
            return pos;
        }
    }

}
