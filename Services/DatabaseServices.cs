using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Nest;
using Microsoft.Extensions.Configuration;

namespace TrackBot4.Services
{
    public class DatabaseServices
    {
        private IConfiguration _configuration;
        public DatabaseServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public int GetProcedureSingleData(long imei, string procedureName)
        {
            try
            {
                using (SqlConnection connection = new(_configuration["ConnectionStrings:DefaultConnection"]))
                {
                    var param = new DynamicParameters();
                    param.Add("@IMEI", imei, DbType.Int64, ParameterDirection.Input);

                    int result = connection.QueryFirst<int>(procedureName, param, commandType: CommandType.StoredProcedure);

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Dictionary<DateTime, int> GetTop10(long imei)
        {
            try
            {
                using (SqlConnection connection = new(connectionString: _configuration["ConnectionStrings:DefaultConnection"]))
                {
                    var param = new DynamicParameters();
                    string spName = "top10";
                    param.Add("@IMEI", imei, DbType.Int64, ParameterDirection.Input);

                    var resultDb = connection.QueryMultiple(spName, param, commandType: CommandType.StoredProcedure).Read();

                    Dictionary<DateTime, int> resultDictionary = new();

                    foreach (IDictionary<string, object> row in resultDb)
                    {
                        if (row.Last().Value is not null) resultDictionary.Add((DateTime)row.First().Value, (int)row.Last().Value);
                    }

                    return resultDictionary;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public float GetTotalDistance(long imei)
        {
            try
            {
                using (SqlConnection connection = new(connectionString: "Data Source=ARCHIBASE\\SQLEXPRESS;Initial Catalog=Track;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
                {
                    var param = new DynamicParameters();
                    string spName = "fullDistance";
                    param.Add("@IMEI", imei, DbType.Int64, ParameterDirection.Input);

                    var resultDb = connection.QueryMultiple(spName, param, commandType: CommandType.StoredProcedure).Read();

                    List<decimal> listDistance = new();

                    foreach (IDictionary<string, object> row in resultDb)
                    {
                        foreach (var pair in row)
                        {
                            listDistance.Add((decimal)pair.Value);
                        }
                    }

                    GeoCoordinate geoCoordinate = new((double)listDistance.First(), (double)listDistance.Last());
                    GeoCoordinate zero = new(0, 0);

                    var totalDistance = (float)GeoServices.CalculateDistance(geoCoordinate, zero);

                    return totalDistance;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public int GetTimeOfWalkings(long imei)
        {
            string spName = "timeWalkings";
            return GetProcedureSingleData(imei, spName);
        }

        public int GetNumberOfWalkings(long imei)
        {
            string spName = "countWalkings";
            return GetProcedureSingleData(imei, spName);
        }
    }
}
