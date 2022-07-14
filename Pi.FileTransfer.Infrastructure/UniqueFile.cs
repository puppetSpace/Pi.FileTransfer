using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Infrastructure;
public class UniqueFile
{
	private readonly string _filePath;
	private readonly Regex _regex = new Regex("_[0-9]*$");

	public UniqueFile(string filePath)
	{
		_filePath = filePath;
	}

	public string GetUniqueFileName()
	{
		var newPath = _filePath;
		while (File.Exists(newPath))
		{
			var sequence = _regex.Match(_filePath)?.Value;
			if(sequence is not null && int.TryParse(sequence,out int isequence))
			{
				newPath = _regex.Replace(newPath, $"{++isequence}");
			}
		}

		return newPath;
	}
}
