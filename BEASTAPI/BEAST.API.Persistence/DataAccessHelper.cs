
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using BEASTAPI.Core.Contract.Persistence;

namespace BEASTAPI.Persistence;

public class DataAccessHelper : IDataAccessHelper
{
    private readonly IConfiguration _config;

    public DataAccessHelper(IConfiguration config)
    {
        this._config = config;
    }

    public async Task<List<T>> QueryData<T, U>(string storedProcedure, U parameters)
    {
        using (IDbConnection connection = new SqlConnection(_config.GetConnectionString("MSSQL")))
        {
            var rows = await connection.QueryAsync<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            return rows.ToList();
        }
    }

    public async Task<int> ExecuteData<T>(string storedProcedure, T parameters)
    {
        using (IDbConnection connection = new SqlConnection(_config.GetConnectionString("MSSQL")))
        {
            return await connection.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
        }
    }
}