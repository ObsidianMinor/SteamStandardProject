namespace Steam
{
    /// <summary>
    /// Specifies Steam user profile visibilities
    /// </summary>
    public enum ProfileVisibility
    {
        /// <summary>
        /// The profile is only visible to its owner
        /// </summary>
        Private = 1,
        /// <summary>
        /// The profile is visible to friends only
        /// </summary>
        FriendsOnly,
        /// <summary>
        /// The profile is visible to everyone
        /// </summary>
        Public
    }
}