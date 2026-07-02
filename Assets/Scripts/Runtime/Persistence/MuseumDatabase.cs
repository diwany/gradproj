using System;
using System.IO;
using SQLite;
using UnityEngine;

namespace Museum.Persistence
{
    /// <summary>
    /// Thin wrapper over sqlite-net for visitor records. The DB file lives at
    /// %APPDATA%/MuseumVR/museum.db on Windows (Application.persistentDataPath fallback elsewhere).
    /// </summary>
    public sealed class MuseumDatabase : IDisposable
    {
        readonly SQLiteConnection _conn;
        public string DbPath { get; }

        public MuseumDatabase(string path = null)
        {
            DbPath = path ?? DefaultDbPath();
            Directory.CreateDirectory(Path.GetDirectoryName(DbPath));
            _conn = new SQLiteConnection(DbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
            _conn.CreateTable<VisitorRecord>();
        }

        public static string DefaultDbPath()
        {
            // Portable path first: <exe folder>/MuseumVR/museum.db. Lets the build be self-contained
            // on a USB drive — visitor history follows the executable, not the host machine's user.
            // We use the portable path if its parent folder already exists, even if museum.db doesn't
            // yet; that's the signal that the operator wants portable mode.
            try
            {
                var dataPath = Application.dataPath;
                if (!string.IsNullOrEmpty(dataPath))
                {
                    var parent = Directory.GetParent(dataPath)?.FullName;
                    if (!string.IsNullOrEmpty(parent))
                    {
                        var portableDir = Path.Combine(parent, "MuseumVR");
                        if (Directory.Exists(portableDir))
                            return Path.Combine(portableDir, "museum.db");
                    }
                }
            }
            catch { /* fall through to AppData */ }

            // %APPDATA% on Windows: e.g. C:\Users\K\AppData\Roaming. Fallback to persistentDataPath on other OS.
            string baseDir;
            try
            {
                baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (string.IsNullOrEmpty(baseDir)) baseDir = Application.persistentDataPath;
            }
            catch
            {
                baseDir = Application.persistentDataPath;
            }
            return Path.Combine(baseDir, "MuseumVR", "museum.db");
        }

        public int InsertVisitor(VisitorRecord record)
        {
            if (record.StartedAtUnixSeconds == 0)
                record.StartedAtUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _conn.Insert(record);
            return record.Id;
        }

        public bool UpdateVisitor(VisitorRecord record)
        {
            return _conn.Update(record) > 0;
        }

        public VisitorRecord GetVisitor(int id) => _conn.Find<VisitorRecord>(id);

        public int VisitorCount() => _conn.Table<VisitorRecord>().Count();

        public void Dispose() => _conn?.Dispose();
    }
}
