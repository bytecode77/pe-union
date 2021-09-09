// Tests with fake API calls (currently unused)

public sealed class FakeApi
{
	public string Dll { get; private set; }
	public string Function { get; private set; }
	public FakeApiParameter[] Parameters { get; private set; }

	public FakeApi(string dll, string function, FakeApiParameter[] parameters)
	{
		Dll = dll;
		Function = function;
		Parameters = parameters;
	}
}

public enum FakeApiParameter
{
	[Description("ptr")]
	Ptr0,
	[Description("ptr$")]
	PtrRandom,
	[Description("ptr!")]
	PtrAllocated,
	[Description("dword")]
	Dword0,
	[Description("dword$")]
	DwordRandom,
	[Description("dword!")]
	DwordAllocated
}

private static FakeApi[] ReadApiFile(string path)
{
	//CURRENT: Refactor...
	List<FakeApi> apis = new List<FakeApi>();

	foreach (string str in ReadLineFile(path))
	{
		string line = str.Replace('\t', ' ');

		string dll;
		string function;
		FakeApiParameter[] parameters;

		if (!line.Contains(" ")) FormatError();
		dll = line.SubstringUntil(" ").Trim();
		line = line.SubstringFrom(" ").Trim();

		function = line.SubstringUntil(" ").Trim();

		if (line.Contains(" "))
		{
			parameters = line
				.SubstringFrom(" ")
				.Trim()
				.Split(',')
				.Select(param =>
				{
					if (EnumEx.FindValueByDescription<FakeApiParameter>(param.Trim()) is FakeApiParameter parameter)
					{
						return parameter;
					}
					else
					{
						FormatError();
						return default;
					}
				})
				.ToArray();
		}
		else
		{
			parameters = new FakeApiParameter[0];
		}

		apis.Add(new FakeApi(dll, function, parameters));
	}

	return apis.ToArray();

	void FormatError()
	{
		throw new FormatException("Failed to parse " + Path.GetFileName(path) + ".");
	}
}

public void UpdateImportTable(string path)
{
	string[] lines = File
		.ReadAllText(path)
		.Replace("\\\n", " ")
		.Replace("\\\r\n", " ")
		.SplitToLines()
		.Select(line => line.Trim())
		.ToArray();

	if (lines.Length < 3 ||
		!lines[0].StartsWith("section ") ||
		!lines[1].StartsWith("library ") ||
		!lines.Skip(2).All(line => line.StartsWith("import "))) FormatError();

	using (AssemblyStream assembly = new AssemblyStream(path))
	{
		Dictionary<string, string> libraryDictionary = ReadDefinition(lines[1]);
		Dictionary<string, Dictionary<string, string>> importDictionary = lines
			.Skip(2)
			.Select(str => Tuple.Create(str.SubstringFrom(" ").SubstringUntil(","), ReadDefinition(str.SubstringFrom(","))))
			.ToDictionary();

		assembly.Indent = 4;
		assembly.WriteLine(lines[0]);
		assembly.EmitDefinition("library", libraryDictionary.Select(entry => entry.Key + ", '" + entry.Value + "'"));

		foreach (KeyValuePair<string, Dictionary<string, string>> import in importDictionary)
		{
			assembly.EmitDefinition("import " + import.Key + ",", import.Value.Select(entry => entry.Key + ", '" + entry.Value + "'"));
		}

		Dictionary<string, string> ReadDefinition(string str)
		{
			string[] entries = str.SubstringFrom(" ").Split(',');
			if (entries.Length % 2 != 0) FormatError();

			Dictionary<string, string> definition = new Dictionary<string, string>();
			for (int i = 0; i < entries.Length / 2; i++)
			{
				definition.Add(entries[i * 2].Trim(), entries[i * 2 + 1].Trim().Trim('\''));
			}

			return definition;
		}
	}

	void FormatError()
	{
		throw new ErrorException("Import table could not be parsed.");
	}
}