using Steam.Common;
using System;

namespace Steam.Community
{
    /// <summary>
    /// Contains the basic information about a community member such as a user or clan
    /// </summary>
    public interface IMember
    {
        SteamId Id { get; }
        string Name { get; }
        Uri AvatarUri { get; }
        Uri MediumAvatarUri { get; }
        Uri FullAvatarUri { get; }
    }
}
