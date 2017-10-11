namespace Steam
{
    /// <summary>
    /// The account instance an account exists in
    /// </summary>
    public enum Instance
    {
        /// <summary>
        /// The account exists in all instances
        /// </summary>
        All,
        /// <summary>
        /// The account exist on desktop
        /// </summary>
        Desktop,
        /// <summary>
        /// The account exist on console
        /// </summary>
        Console,
        /// <summary>
        /// The account exist on the web
        /// </summary>
        Web = 4,
    }
}
