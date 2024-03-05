# BooksToScape
 ## Introduction
 This project is a small on Crawling with in C#. It's name choice is resulted from the project crawling the https://books.scrape.com and my missing the letter 'r' while initially creating the project. So I pretend the naming is just a playful change from the original website's name!
 
## Project sctructe
The project is a typical .net 8 Console application solution with .net's default dependency injection container added to it. There are two projects inside the solution. 

- `BooksToScape.App` which contains the main application and its required utilities 
- `BooksToScape.Tests` which contains both Playwreight tests and Nunit tests. Nsubstitude is used for dependency mocking and FluentAssertions is used as the assertion library.

### Patterns, libraries used & general tips
Below you will find information about patterns, methods or tips that are used in the project and knowing them will help you navigate the project a bit easier.

#### Perfomant and Default Crawlers
For demonstration purposes, there are two sets of crawlers in the project. One is a default 'greedy' version in which each tasks are being done one after another. The other one is the 'Perfomant' crawler where tasks are being done in parallel and the crawling happens much faster. By default, the `PerfomantCrawler` is used when you run the application. 
However, you can switch to the non-performant crawlers by simply setting the `DefaultCrawler` and `ResourceCrawler` as implementations of `IBooksToScrapeCrawler` and `IResourceCrawler` respecively in the begennig of the `Program.cs`.

### Result Pattern
This project uses a very simple but robust Result implementation through https://github.com/altmann/FluentResults. The idea is to return `Result` objects for methods so the calling functions can get the state of the result and act on upon its Failure or Success states. It also helps a lot with error aggregation and has a perfomance benefit over rethrowing exceptions.

### MedaitR
The MediatR library is used only for its Nottifications functionality, which is MediatR's take on Pub/Sub pattern. The only thing about it that might appear "magical" is that since the setup for MediatR is done for the whole assmebly, the notification handlers "just work" as long as a class that extends `INotificationHandler<T>` is created in the same assmebly. The Notifications are used to report the progress of how many files has been crawled. 

## Building & Running the project
The project is a standard .net solution, therefore you can easily open the `.sln` file in your IDE of choice, build the project and run the `Program.cs`. 
Alternatively, fromt the terminal, you can build the project by navigating to the solutions folder and `dotnet build`.
To run the project from the solution folder using a powershell terminal, use the 
command `dotnet run --project .\BooksToScape.App\`

### Running th tests
Some of the tests use `Playwreight` and these tests are the only ones that require a bit of work before you can run them.
All you need to do is to build the test project once, and then you should be able to see that a new file called `playwright.ps1` is created in the `bin/debug/net8` directory of the test project. In a powershell terminal, navigate to `bin/debug/net8` and run `pwsh .\playwright.ps1 install` . This will install the drivers required to run the playwrieght  tests. You only need to do this once.
If you encounter the error message `The term 'pwsh' is not recognized as the name of a cmdlet, function, script file, or operable program`, make sure you are on the latest version of powershell. You can also read more about the installation steps of Playwreight at https://playwright.dev/dotnet/docs/intro
