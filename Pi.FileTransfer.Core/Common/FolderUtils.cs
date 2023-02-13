namespace Pi.FileTransfer.Core.Common;

public static class FolderUtils
{
    public static string GetIncomingFolderPath(string folderPath,string fileName = "") => Path.Combine(folderPath,Constants.RootDirectoryName, "Data", "Incoming",fileName??"");
    public static string GetIncomingFolderTempPath(string folderPath,string fileName = "") => Path.Combine(folderPath,Constants.RootDirectoryName, "Data", "Incoming","Temp",fileName??"");
    public static string GetDeltasFolderPath(string folderPath,string fileName = "") => Path.Combine(folderPath,Constants.RootDirectoryName, "Data", "Deltas",fileName??"");
    public static string GetSignaturesFolderPath(string folderPath,string fileName = "") => Path.Combine(folderPath,Constants.RootDirectoryName, "Data", "Signatures",fileName??"");
}
