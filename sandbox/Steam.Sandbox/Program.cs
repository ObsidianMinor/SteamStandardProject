/* README
 * This project is an untracked directory,
 * it serves as a sandbox to test features
 * you've added to the Steam Standard projects.
 * Changes made here skip the working tree and
 * will not be commited or pushed to the upstream repository
 */

using System;

namespace Steam.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(SteamId.FromCommunityId(76561198092222042));
        }
    }
}
