namespace DataLayer;

public interface IDataLayer
{
    void InitializeProcessedFilesDatabase();
    void InitializeReplacementsDatabase();
    int SaveProcessedFile(string fileName);
    bool IsFileProcessed(string fileName);
}