using System.Diagnostics;
using System.Text.Json;
using SimpleSearchEngine;

var searchEngine = new SearchEngine<Movie>();

var movies = GetMovies();
searchEngine.Index(movies, x => x.Fields.Title);

var linqIndex = movies.Where(x => !string.IsNullOrWhiteSpace(x.Fields.Title))
	.Select(x => new LinqIndexEntry()
	{
		Words = x.Fields.Title.Split(new[] { ' ', '\n', '\r', '\t', '-' }, StringSplitOptions.RemoveEmptyEntries),
		Movie = x
	})
	.ToList();

while (true)
{
	Console.Write("Enter search term: ");
	var searchTerm = Console.ReadLine();

	if (string.IsNullOrEmpty(searchTerm))
		break;

	// ---------- SEARCH ENGINE SEARCH ----------------
	Console.WriteLine("Using search engine");
	Console.WriteLine("===================");

	var searchResults = searchEngine.Search(searchTerm);

	foreach (var result in searchResults)
		Console.WriteLine("*** {0}", result.Fields.Title);

	Console.WriteLine("{0} results ({1} seconds)", searchResults.Count(), searchResults.Elapsed.TotalSeconds.ToString("0.00####################"));
	Console.WriteLine();

	// ---------- LINQ SEARCH ----------------
	Console.WriteLine("Using LINQ");
	Console.WriteLine("==========");

	var stopwatch = Stopwatch.StartNew();
	var results = linqIndex.Where(x => x.Words.Any(e => e.StartsWith(searchTerm, StringComparison.InvariantCultureIgnoreCase))).ToList();
	stopwatch.Stop();

	foreach (var result in results)
		Console.WriteLine("*** {0}", result.Movie.Fields.Title);

	Console.WriteLine("{0} results ({1} seconds)", results.Count(), stopwatch.Elapsed.TotalSeconds.ToString("0.00####################"));
	Console.WriteLine();
}

static Movie[] GetMovies()
{
	var json = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "moviedata.json"));
	return JsonSerializer.Deserialize<Movie[]>(json, new JsonSerializerOptions()
	{
		PropertyNameCaseInsensitive = true
	}) ?? Array.Empty<Movie>();
}

class LinqIndexEntry
{
	public string[] Words { get; set; }
	public Movie Movie { get; set; }
}

class Movie
{
	public string Type { get; set; }
	public string Id { get; set; }
	public int Version { get; set; }
	public string Lang { get; set; }
	public Fields Fields { get; set; }
}

class Fields
{
	public string Title { get; set; }
	public int? Year { get; set; }
	public string Director { get; set; }
	public string[] Genre { get; set; }
	public string[] Actor { get; set; }
}