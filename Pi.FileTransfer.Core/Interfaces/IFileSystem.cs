﻿namespace Pi.FileTransfer.Core.Interfaces;
public interface IFileSystem
{
    void CreateDirectory(string path);
    void DeleteFile(string file);
    Task<TE> GetContentOfFile<TE>(string transferFile);
    IEnumerable<(string file, DateTime lastModified)> GetFiles(Core.Entities.Folder folder);
    IEnumerable<string> GetFiles(string path, string pattern, bool recursive = false);
    FileStream GetReadFileStream(string file, int bufferSize = 2048);
    IEnumerable<string> GetFoldersFromBasePath();
    Task WritetoFile<TE>(string transferFile, TE value);
    FileStream GetWriteFileStream(string file, int bufferSize = 2048);
    void MoveFile(string source, string destination);
    void DeleteDirectory(string incomingDataFolder);
}
