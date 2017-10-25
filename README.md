# The Steam Standard Project

A collection of .NET Standard libraries using common types that provide functionality in one or more Steam services.

### Current libraries
| Library             | Description                                                                                                                                               | Progress                     | .NET Standard version |
|---------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------|-----------------------|
| Steam.Common        | Common types shared across multiple Steam libraries                                                                                                       | Shared - Added to as needed | .NET Standard 1.0     |
| Steam.KeyValues     | A library for serializing KeyValues.                                                                                                      | Work in progress             | .NET Standard 2.0     |
| Steam.Rest          | Common types for REST and HTTP requests                                                                                                                   | Shared - Added to as needed  | .NET Standard 1.1     |
| Steam.Net           | A reimagining of the SteamKit built for async events, task-based asynchronous programming, a self-contained reconnect loop, and an abstracted job system. | Working form             | .NET Standard 2.0     |
| Steam.Web           | A statically typed wrapper around the official Steam Web API                                                                                              | Working form             | .NET Standard 2.0     |

### Getting started
If you want to help develop this project, you will need a two things:

 * [.NET Core 2.0](https://www.microsoft.com/net/core#windowscmd)
 * [Visual Studio 2017 15.3](https://www.visualstudio.com/vs/)

#### But why
The Steam Standard Project is built with the goal of providing an easy to use wrapper around any part of Steam, even parts already covered by existing libraries. Existing libraries were built to almost emulate how Steam worked. However writing them that way causes the library to lack what makes C# libraries and code easy to use. Steam was written the way it was because it lacked features other languages had. C++ does not have Tasks or multicast delegates, so they had to write async code and events with callbacks and jobs. So, instead of making these libraries how Steam would make them in C++, I'm making them how they would be written in C#.

#### Navigating this repository
All projects, samples, and tests can be found in the "Steam Standard" solution, documentation for all libraries can be found on [GitHub Pages](https://obsidianminor.github.io/SteamStandardProject) or edited in the docs folder.
