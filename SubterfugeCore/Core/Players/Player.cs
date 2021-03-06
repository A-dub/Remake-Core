﻿using System.Collections.Generic;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.Network;

namespace SubterfugeCore.Core.Players
{
    /// <summary>
    /// An instance of a player
    /// </summary>
    public class Player
    {
        /// <summary>
        /// The name or alias of the player
        /// </summary>
        private string PlayerName { get;  }
        
        /// <summary>
        /// The player's id
        /// </summary>
        private int PlayerId { get; }

        /// <summary>
        /// Constructor to create an instance of a player based off of their player Id
        /// </summary>
        /// <param name="playerId">The player's ID in the database</param>
        public Player(int playerId)
        {
            this.PlayerId = playerId;
        }
        
        /// <summary>
        /// Constructor to create an instance of a player based off of their player Id and name
        /// </summary>
        /// <param name="playerId">The player's ID in the database</param>
        /// <param name="name">The player's name</param>
        public Player(int playerId, string name)
        {
            this.PlayerId = playerId;
            this.PlayerName = name;
        }

        public Player(NetworkUser networkUser)
        {
            this.PlayerId = networkUser.Id;
            this.PlayerName = networkUser.Name;
        }

        /// <summary>
        /// Checks if the player's queen is alive at the current game tick.
        /// </summary>
        /// <returns>If the player's queen is alive</returns>
        public bool IsAlive()
        {
            List<Specialist> playerSpecs = Game.TimeMachine.GetState().GetPlayerSpecialists(this);
            
            // Find the player's queen.
            foreach (Specialist spec in playerSpecs)
            {
                Queen playerQueen = spec as Queen;
                if (playerQueen != null)
                {
                    return playerQueen.IsCaptured;
                }
            }

            // Player doesn't have a queen. Odd but possible if stolen.
            return false;
        }

        /// <summary>
        /// Gets the player's id
        /// </summary>
        /// <returns>The player's database ID</returns>
        public int GetId()
        {
            return this.PlayerId;
        }

        /// <summary>
        /// Get the player's username
        /// </summary>
        /// <returns>The player's username</returns>
        public string GetPlayerName()
        {
            return this.PlayerName;
        }
    }
}
