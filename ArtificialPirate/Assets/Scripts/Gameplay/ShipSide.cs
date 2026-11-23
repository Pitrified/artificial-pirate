namespace Game.Gameplay
{
    /// <summary>Which side a ship fights on: its guns fire at ships of another side, never its own.</summary>
    public enum ShipSide
    {
        /// <summary>The player's ship and its allies.</summary>
        Player,

        /// <summary>The pirates the player fights.</summary>
        Pirates,
    }
}
