using LiteDB;
using Rebyu.Interfaces;
using Rebyu.Models;

namespace Rebyu.Services;

public class SessionRepository(ILiteDatabase db) : BaseRepository<Session>(db), ISessionRepository
{
}