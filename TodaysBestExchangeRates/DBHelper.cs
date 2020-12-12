using System.Configuration;
using System.Data.SqlClient;
using System.Collections.Generic;
using System;

namespace TodaysBestExchangeRates
{
    struct BestExchangeRate
    {
        public string ResourceName { get; set; }
        public string Symbol { get; set; }
        public decimal Rate { get; set; }
    }
    class DBHelper
    {
        readonly string _connectionstring;
        public DBHelper()
        {
            _connectionstring = ConfigurationManager.ConnectionStrings["CurrencyExchangeDB"].ConnectionString; 
        }

        /// <summary>
        /// To get the best exchange rates from DB
        /// </summary>
        /// <returns></returns>
        public List<BestExchangeRate> GetBestExchangeRates()
        {
            List<BestExchangeRate> bestExchangeRates = new List<BestExchangeRate>();
            SqlConnection sqlConnection = new SqlConnection(_connectionstring);
            try 
            {
                using (sqlConnection)
                {
                    string commandstring = "GetBestRates";
                    SqlCommand command = new SqlCommand(commandstring, sqlConnection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlConnection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            BestExchangeRate bestExchangeRate = new BestExchangeRate();
                            bestExchangeRate.ResourceName = reader["datasource"].ToString();
                            bestExchangeRate.Symbol = reader["Symbol"].ToString();
                            bestExchangeRate.Rate = decimal.Parse(reader["Rate"].ToString());
                            bestExchangeRates.Add(bestExchangeRate);
                        }
                    }
                    reader.Close();
                }
            }
            catch(Exception ex)
            {
                //Do nothing. bestExchangeRates count wiil be 0. It means error 
            }
            return bestExchangeRates;
        }
        
    }
}
