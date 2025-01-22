// See https://aka.ms/new-console-template for more information

using Zx;
using static Zx.Env;


// // `await string` execute process like shell
// await "cat package.json | grep name";
//
// // receive result msg of stdout
// var branch = await "git branch --show-current";
// await $"dep deploy --branch={branch}";

// parallel request (similar as Task.WhenAll)
await new[]
{
    "echo 1",
    "echo 2",
    "echo 3",
};

await "ls";

// you can also use cd(chdir)
await "cd ../../../";

await "ls";

// run with $"" automatically escaped and quoted
// var dir = "foo/foo bar";
// await Run($"mkdir {dir}"); // mkdir "/foo/foo bar"

// helper for Console.WriteLine and colorize
Log("red log.", ConsoleColor.Red);
using (Color(ConsoleColor.Blue))
{
    Log("blue log");
    Console.WriteLine("also blue");
    await Run($"echo {"blue blue blue"}");
}

// helper for web request
var text = await FetchText("http://wttr.in");
Log(text);

// helper for ReadLine(stdin)
var bear = await Question("What kind of bear is best?");
Log($"You answered: {bear}");

// run has some variant(run2, runl, withTimeout, withCancellation)
// runl returns string[](runlist -> runl)
var sdks = await RunLine($"dotnet --list-sdks");