using System.Data;
using DataLayer.Models;

namespace DataLayer.Implementations;
using Microsoft.Data.Sqlite;

public class SqLiteDataLayer : IDataLayer
{
    private SqliteConnection _connection;

    public SqLiteDataLayer(WorkerConfiguration workerConfiguration)
    {
        var sqlFilePath = Path.Combine(workerConfiguration.ConfigurationDirectory, workerConfiguration.DatabaseFile);
        _connection = new SqliteConnection("Data Source=" + sqlFilePath);

        if (!File.Exists(sqlFilePath))
        {
            InitializeProcessedFilesDatabase();
        }
    }

    public void InitializeProcessedFilesDatabase()
    {
        try
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            using var command = _connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS ProcessedFiles (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FileName TEXT NOT NULL UNIQUE,
                    ProcessedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                )";

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Initialization of ProcessedFiles database failed: {ex.Message}");
            throw;
        }
        finally
        {
            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
        }
    }
    
    public void InitializeReplacementsDatabase()
    {
        throw new NotImplementedException();
    }

    public int SaveProcessedFile(string fileName)
    {
        try
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            
            using var command = _connection.CreateCommand();
            command.CommandText = "INSERT INTO ProcessedFiles (FileName) VALUES (@FileName)";
            command.Parameters.AddWithValue("@FileName", fileName);
            return command.ExecuteNonQuery();
        }
        catch (SqliteException ex) when (ex.ErrorCode == 19) // SQLITE_CONSTRAINT
        {
            Console.WriteLine($"File was already processed: {fileName}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving file: {ex.Message}");
            return 0;
        }
        finally
        {
            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
        }
    }

    public bool IsFileProcessed(string fileName)
    {
        try
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT COUNT(1) FROM ProcessedFiles WHERE FileName = @FileName";
            command.Parameters.AddWithValue("@FileName", fileName);
            var count = command.ExecuteScalar();
            return Convert.ToInt32(count) > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking file: {ex.Message}");
            return false;
        }
        finally
        {
            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
        }
    }
}