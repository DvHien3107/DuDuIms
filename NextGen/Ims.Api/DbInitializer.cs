using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.RegularExpressions;

namespace Promotion_API
{
    public static partial class DbInitializer
    {
        public static async Task InitializeAsync(string connectionString)
        {
            try
            {
                DirectoryInfo d = new(@"wwwroot//sqlFiles");
                FileInfo[] files = d.GetFiles("*.sql").OrderBy(f => f.FullName).ToArray();

                using IDbConnection db = new SqlConnection(connectionString);
                db.Open();
                try
                {
                    foreach (FileInfo file in files)
                    {
                        string script = File.ReadAllText(Path.Combine("wwwroot", "sqlFiles", file.FullName));

                        // split script on GO command
                        IEnumerable<string> commandStrings = Regex.Split(script, "^\\s*GO\\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        foreach (string commandString in commandStrings)
                        {
                            if (!string.IsNullOrWhiteSpace(commandString.Trim()))
                            {
                                try
                                {
                                    await db.ExecuteAsync(commandString.Trim());
                                }
                                catch (Exception) { }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    db.Close();
                }
            }
            catch (System.Exception)
            {
            }
        }
    }
}
