using LibGit2Sharp;

File.WriteAllText("date.txt", DateTimeOffset.UtcNow.ToString("O"));

using var repo = new Repository("../");
Commands.Stage(repo, "*");
// Create the committer's signature and commit
var author = new Signature("GitHub Actions Bot", "actions@githib.com", DateTime.Now);
var committer = author;

// Commit to the repository
Commit commit = repo.Commit("Commit from code", author, committer);

var remote = repo.Network.Remotes["origin"];
var options = new PushOptions();
repo.Network.Push(repo.Branches["main"]);
