namespace Game.Gameplay
{
    /// <summary>A group of a ship's guns that bear the same way and fire together.</summary>
    public enum Battery
    {
        /// <summary>The broadside to starboard.</summary>
        Starboard,

        /// <summary>The broadside to port.</summary>
        Port,

        /// <summary>The chasers, firing ahead.</summary>
        Bow,

        /// <summary>The stern guns, firing astern.</summary>
        Stern,
    }
}
