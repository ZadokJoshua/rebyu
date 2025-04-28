using LiteDB;
using Rebyu.Interfaces;
using System;

namespace Rebyu.Services;

public class LiteDbDataService : ILiteDbDataService, IDisposable
{
    private LiteDatabase _db;
    private bool _disposed = false;
    private readonly string _connectionStrings;
    private ISessionRepository _sessionRepository;

    public LiteDbDataService(string connectionStrings)
    {
        if (string.IsNullOrEmpty(connectionStrings))
            throw new ArgumentNullException(nameof(connectionStrings), "Connection string cannot be null or empty.");

        _connectionStrings = connectionStrings;
    }

    private ILiteDatabase DB
    {
        get
        {
            return _db ??= new LiteDatabase(_connectionStrings); 
        }
    }

    public ISessionRepository SessionRepository
    {
        get { return _sessionRepository ??= new SessionRepository(DB); }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing && _db != null)
            {
                _db.Dispose();
            }
            _disposed = true;
        }
    }

    ~LiteDbDataService()
    {
        Dispose(false);
    }
}
