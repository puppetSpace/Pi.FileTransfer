namespace Pi.FileTransfer.Core.Common;

public static class FolderUtils
{
    public static string GetIncomingFolderPath(string folderPath) => Path.Combine(folderPath,Constants.RootDirectoryName, "Data", "Incoming");
    public static string GetIncomingFolderPathForFile(string folderPath, Guid fileId) => Path.Combine(folderPath,Constants.RootDirectoryName, "Data", "Incoming",fileId.ToString());

}
