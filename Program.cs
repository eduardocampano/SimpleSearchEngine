using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace SimpleSearchEngine
{
	class Program
	{
		static void Main(string[] args)
		{
			var searchEngine = new SearchEngine<Movie>();

			var documents = GetDocuments();
			
			searchEngine.Index(documents, x => x.Fields.Title);
			var documentsForSimpleSearch = documents.Where(x => !string.IsNullOrWhiteSpace(x.Fields.Title))
													.Select(x => new { Index = x.Fields.Title.Split(new [] { ' ', '\n', '\r', '\t', '-' }, StringSplitOptions.RemoveEmptyEntries), Movie = x })
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
					Console.WriteLine("*** Matched {0}", result.Fields.Title);

				Console.WriteLine("{0} results ({1} seconds)", searchResults.Count(), searchResults.Elapsed.TotalSeconds.ToString("0.00####################"));
				Console.WriteLine();
				
				// ---------- LINQ SEARCH ----------------
				Console.WriteLine("Using LINQ");
				Console.WriteLine("==========");

				var stopwatch = Stopwatch.StartNew();
				var results = documentsForSimpleSearch.Where(x => x.Index.Any(e => e.StartsWith(searchTerm, StringComparison.InvariantCultureIgnoreCase))).ToList();
				stopwatch.Stop();

				foreach (var result in results)
					Console.WriteLine("*** Matched {0}", result.Movie.Fields.Title);

				Console.WriteLine("{0} results ({1} seconds)", results.Count(), stopwatch.Elapsed.TotalSeconds.ToString("0.00####################"));
				Console.WriteLine();
			}
		}

		static Movie[] GetDocuments()
		{
			var json = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "moviedata.json"));
			return JsonConvert.DeserializeObject<Movie[]>(json);
		}

		class Movie
		{
			public string Type { get; set; }
			public string Id { get; set; }
			public int Version { get; set; }
			public string Lang { get; set; }
			public Field Fields { get; set; }
		}

		class Field
		{
			public string Title { get; set; }
			public int? Year { get; set; }
			public string Director { get; set; }
			public string[] Genere { get; set; }
			public string[] Actor { get; set; }
		}
	}
}
