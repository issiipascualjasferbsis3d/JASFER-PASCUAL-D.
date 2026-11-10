using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Security.Cryptography;

namespace Activity3
{
    public static class Database
    {
        public static readonly string DBPATH = ResolveDatabasePath();

        private static string ResolveDatabasePath()
        {
            string projectDatabase = Path.GetFullPath(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "..", "..", "..",
                    "sampledb.accdb"));

            if (File.Exists(projectDatabase))
                return projectDatabase;


            string besideApp = Path.Combine(AppContext.BaseDirectory, "sampledb.accdb");

            if (File.Exists(besideApp))
                return besideApp;

            throw new FileNotFoundException(
                "The Access database was not found.",
                projectDatabase);
        }

        private static string ConnectionString =>
            $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={DBPATH};Persist Security Info=False;";

        public static OleDbConnection GetConnection() =>
            new OleDbConnection(ConnectionString);

        public static void Initialize()
        {
            string? folder = Path.GetDirectoryName(DBPATH);
            if (!string.IsNullOrEmpty(folder))
                Directory.CreateDirectory(folder);

            if (!File.Exists(DBPATH))
                throw new FileNotFoundException(
                    "The Access database was not found. Place sampledb.accdb in the project folder.",
                    DBPATH);

            using OleDbConnection connection = GetConnection();
            connection.Open();

            if (!TableExists(connection, "Users"))
            {
                using OleDbCommand create = connection.CreateCommand();
                create.CommandText =
                    "CREATE TABLE Users (" +
                    "ID COUNTER CONSTRAINT PK_Users PRIMARY KEY, " +
                    "Username TEXT(50) NOT NULL, " +
                    "PasswordHash TEXT(255) NOT NULL, " +
                    "FullName TEXT(100) NOT NULL, " +
                    "UserRole TEXT(30), " +
                    "isActive YESNO)";
                create.ExecuteNonQuery();
            }

        }

        private static bool TableExists(OleDbConnection connection, string tableName)
        {
            DataTable tables = connection.GetSchema("Tables");
            foreach (DataRow row in tables.Rows)
            {
                if (string.Equals(Convert.ToString(row["TABLE_NAME"]), tableName,
                    StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public static bool UsernameExists(string username)
        {
            using OleDbConnection connection = GetConnection();
            connection.Open();

            using OleDbCommand command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Users WHERE Username = ?";
            command.Parameters.Add("?", OleDbType.VarWChar, 50).Value = username.Trim();
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public static void RegisterUser(string fullName, string username, string password)
        {
            string passwordHash = HashPassword(password);

            using OleDbConnection connection = GetConnection();
            connection.Open();

            using OleDbCommand command = connection.CreateCommand();
            command.CommandText =
                "INSERT INTO Users (Username, PasswordHash, FullName, UserRole, isActive) " +
                "VALUES (?, ?, ?, ?, ?)";
          
            command.Parameters.Add("?", OleDbType.VarWChar, 50).Value = username.Trim();
            command.Parameters.Add("?", OleDbType.VarWChar, 255).Value = passwordHash;
            command.Parameters.Add("?", OleDbType.VarWChar, 100).Value = fullName.Trim();
            command.Parameters.Add("?", OleDbType.VarWChar, 30).Value = "User";
            command.Parameters.Add("?", OleDbType.Boolean).Value = true;

            int rows = command.ExecuteNonQuery();
            if (rows != 1)
                throw new InvalidOperationException("The account was not inserted into the Users table.");
        }

        public static bool ValidateLogin(string username, string password)
        {
            using OleDbConnection connection = GetConnection();
            connection.Open();

            using OleDbCommand command = connection.CreateCommand();
            command.CommandText =
                "SELECT PasswordHash, isActive FROM Users WHERE Username = ?";
            command.Parameters.Add("?", OleDbType.VarWChar, 50).Value = username.Trim();

            using OleDbDataReader reader = command.ExecuteReader();
            if (!reader.Read())
                return false;

            bool active = reader["isActive"] != DBNull.Value &&
                          Convert.ToBoolean(reader["isActive"]);
            if (!active)
                return false;

            string storedHash = Convert.ToString(reader["PasswordHash"]) ?? "";
            return VerifyPassword(password, storedHash);
        }

        private static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, 100_000, HashAlgorithmName.SHA256, 32);
            return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
        }

        private static bool VerifyPassword(string password, string storedValue)
        {
            try
            {
                string[] parts = storedValue.Split('.');
                if (parts.Length != 2)
                    return false;

                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] expected = Convert.FromBase64String(parts[1]);
                byte[] actual = Rfc2898DeriveBytes.Pbkdf2(
                    password, salt, 100_000, HashAlgorithmName.SHA256, expected.Length);

                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
