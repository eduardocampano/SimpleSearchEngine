using System.Collections;
using System.Diagnostics;

namespace SimpleSearchEngine;

public class SearchEngine<T>
{
	private Node? _root;

	public SearchEngine()
	{
	}

	public SearchEngine(IEnumerable<T> elementsToIndex, Func<T, string> indexer)
	{
		Index(elementsToIndex, indexer);
	}

	public SearchResult Search(string text)
	{
		var stopwatch = Stopwatch.StartNew();
		var node = FindNode(text.ToLowerInvariant());
		stopwatch.Stop();

		return new SearchResult(node.Elements, stopwatch.Elapsed);
	}

	public void Index(IEnumerable<T> elements, Func<T, string> indexer)
	{
		_root = null;
		var tree = new SearchEngine<T>();

		var entries = new Dictionary<string, HashSet<T>>();
		foreach (var element in elements)
		{
			var index = indexer(element);
			if (string.IsNullOrWhiteSpace(index))
				continue;

			var indexEntries = new List<string>();
			indexEntries.Add(index);
			indexEntries.AddRange(index.Split(new[] { ' ', '\n', '\r', '\t', '-' }, StringSplitOptions.RemoveEmptyEntries));

			foreach (var indexEntry in indexEntries)
			{
				var key = indexEntry.ToLowerInvariant();
				if (entries.ContainsKey(key))
					entries[key].Add(element);
				else
					entries.Add(key, new HashSet<T>() { element });
			}
		}

		var indexKeys = entries.Select(x => x.Key).ToArray();

		tree.BuildBalancedTree(indexKeys, 0, indexKeys.Length);

		foreach (var entry in entries)
			tree.AddToLists(entry.Key, entry.Value);

		_root = tree._root;
	}

	private void BuildBalancedTree(string[] suggestions, int left, int right)
	{
		if (left < right)
		{
			int mid = (left + right) / 2;
			_root = Insert(suggestions[mid], 0, _root);
			BuildBalancedTree(suggestions, left, mid);
			BuildBalancedTree(suggestions, mid + 1, right);
		}
	}

	private Node Insert(string suggestion, int i, Node? ls)
	{
		if (ls == null)
			return new Node(suggestion, suggestion.Length, null);
		if (suggestion[i] < ls.First[i])
			ls.Left = Insert(suggestion, i, ls.Left);
		else if (suggestion[i] > ls.First[i])
			ls.Right = Insert(suggestion, i, ls.Right);
		else
		{
			while (++i < ls.CharEnd)
			{
				if (i == suggestion.Length || suggestion[i] != ls.First[i])
				{
					ls.Mid = new Node(ls.First, ls.CharEnd, ls.Mid);
					ls.CharEnd = i;
					break;
				}
			}
			if (i < suggestion.Length)
				ls.Mid = Insert(suggestion, i, ls.Mid);
		}
		return ls;
	}

	private void AddToLists(string suggestion, IEnumerable<T> elements)
	{
		var i = 0;
		var ls = _root;
		while (true)
		{
			if (suggestion[i] < ls.First[i])
				ls = ls.Left;
			else if (suggestion[i] > ls.First[i])
				ls = ls.Right;
			else
			{
				foreach (var el in elements)
					ls.Elements.Add(el);
				i = ls.CharEnd;
				if (i == suggestion.Length)
					return;
				ls = ls.Mid;
			}
		}
	}

	private Node? FindNode(string prefix)
	{
		int i = 0;
		var ls = _root;
		while (ls != null)
		{
			if (prefix[i] < ls.First[i])
				ls = ls.Left;
			else if (prefix[i] > ls.First[i])
				ls = ls.Right;
			else
			{
				while (++i < ls.CharEnd)
				{
					if (i == prefix.Length)
						return ls;
					if (prefix[i] != ls.First[i])
						return null;
				}
				if (i == prefix.Length)
					return ls;
				ls = ls.Mid;
			}
		}
		return null;
	}

	public class SearchResult : IEnumerable<T>
	{
		private readonly IEnumerable<T> _results;

		internal SearchResult(IEnumerable<T> results, TimeSpan elapsed)
		{
			_results = results;
			Elapsed = elapsed;
		}

		public TimeSpan Elapsed { get; private set; }

		public IEnumerator<T> GetEnumerator()
		{
			return _results.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	private class Node
	{
		public string First { get; private set; }
		public HashSet<T> Elements { get; private set; }
		public int CharEnd { get; set; }
		public Node? Left { get; set; }
		public Node? Mid { get; set; }
		public Node? Right { get; set; }

		public Node(string first, int charEnd, Node? mid)
		{
			First = first;
			Elements = new HashSet<T>();
			CharEnd = charEnd;
			Mid = mid;
		}
	}
}
