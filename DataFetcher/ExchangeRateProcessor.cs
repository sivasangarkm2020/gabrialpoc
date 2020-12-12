using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace DataFetcher
{
    /// <summary>
    /// To fetch, Process and store data from data sources
    /// </summary>
    class ExchangeRateProcessor
    {
        List<iDataResource> _dataSources = new List<iDataResource>();
        public ExchangeRateProcessor(List<iDataResource> datasources)
        {
            _dataSources = datasources;
        }

        /// <summary>
        /// To Fetch And Add New Symbols
        /// </summary>
        /// <returns>Success or Fails (True or False)</returns>
        public bool FetchAndAddNewSymbols()
        {
            CurrencyExchangeEntities currencyExchangeEntities = new CurrencyExchangeEntities();
            List<string> symbols = new List<string>();
            List<string> newSymbols = new List<string>();
            try
            {
                List<string> existingCurrencySymbols = currencyExchangeEntities.Currencies.Select(x => x.Code.Trim().ToUpper()).ToList();

                //Fetching symbols from all new resource types
                foreach (iDataResource dataSource in _dataSources)
                {
                    symbols.AddRange(dataSource.FetchNewSymbols());
                }

                if (symbols.Count() > 0)
                {
                    if (existingCurrencySymbols.Count > 0)
                    {
                        //Fetching Distinct symbols from resources as well as symbols not in DB already
                        newSymbols = symbols.Distinct().Except(existingCurrencySymbols).ToList();
                    }
                    else
                    {
                        //Fetching Distinct symbols from resources
                        newSymbols = symbols.Distinct().ToList();
                    }

                    if (newSymbols.Count() > 0)
                    {
                        //Add all the symbols into currency table
                        foreach (var symbol in newSymbols)
                        {
                            Currency currency = new Currency();
                            currency.Code = symbol.Trim().ToUpper();
                            currencyExchangeEntities.Currencies.Add(currency);//DB
                        }
                        var dataSources = currencyExchangeEntities.DataSources;

                        //Marking all the datasources as not new
                        foreach (var dataSource in dataSources)
                        {
                            dataSource.IsNew = false;//DB
                        }
                        currencyExchangeEntities.SaveChanges();//DB
                        Program.logger.Info("New symbols added successfully!");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Program.logger.Error("Error while adding new symbols.");
                Program.logger.Error(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// To Fetch And Insert Rates
        /// </summary>
        /// <returns>Success or Fails (True or False)</returns>
        public bool FetchAndInsertRates()
        {
            CurrencyExchangeEntities currencyExchangeEntities = new CurrencyExchangeEntities();
            try
            {
                //Check data is available for today
                List<ExchangeRate> todayExchangeRates = currencyExchangeEntities.ExchangeRates.Where(x => x.Date == DateTime.Today).ToList();
                if (todayExchangeRates.Count == 0)
                {
                    List<ExchangeRate> exchangeRates = new List<ExchangeRate>();
                    //Fetching rates from all the recources
                    foreach (iDataResource dataSource in _dataSources)
                    {
                        exchangeRates.AddRange(dataSource.FetchTodayExchangeRates());
                    }
                    if (exchangeRates.Count() > 0)
                    {
                        //Adding rates from all the recources into DB
                        foreach (var rate in exchangeRates)
                        {
                            ExchangeRate exchangeRate = new ExchangeRate();
                            exchangeRate.DataSourceId = rate.DataSourceId;
                            exchangeRate.CurrencyId = rate.CurrencyId;
                            exchangeRate.Rate = rate.Rate;
                            exchangeRate.Date = rate.Date;
                            currencyExchangeEntities.ExchangeRates.Add(exchangeRate);//DB
                        }
                        currencyExchangeEntities.SaveChanges();//DB
                        Program.logger.Info("Today exchange rates added successfully!");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Program.logger.Error("Error while adding today rates.");
                Program.logger.Error(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// To Delete Old Best Rates
        /// </summary>
        /// <returns>Success or Fails (True or False)</returns>
        public bool DeleteOldBestRates()
        {
            CurrencyExchangeEntities currencyExchangeEntities = new CurrencyExchangeEntities();
            try
            {
                //Deleting old best rates
                currencyExchangeEntities.BestRates.RemoveRange(currencyExchangeEntities.BestRates);//DB
                currencyExchangeEntities.SaveChanges();//DB
                Program.logger.Info("Old best rates deleted successfully!");
                return true;
            }
            catch (Exception ex)
            {
                Program.logger.Error("Error while deleting old best rates.");
                Program.logger.Error(ex.Message);
                return false;
            }

            
        }

        /// <summary>
        /// To Find Best Rates And Add
        /// </summary>
        /// <returns>Success or Fails (True or False)</returns>
        public bool FindBestRatesAndAdd()
        {
            CurrencyExchangeEntities currencyExchangeEntities = new CurrencyExchangeEntities();
            try
            {
                var todayBestRates = currencyExchangeEntities.BestRates.Where(x => x.Date == DateTime.Today).ToList();
                if (todayBestRates.Count() == 0)
                {
                    //Finding best rate by comparing them by grouping them by symbol
                    var groupedResult = currencyExchangeEntities.ExchangeRates.Where(x => x.Date == DateTime.Today).GroupBy(x => x.CurrencyId);
                    if (groupedResult.Count() > 0)
                    {
                        foreach (var group in groupedResult)
                        {
                            BestRate bestRate = new BestRate();
                            foreach (var item in group)
                            {
                                if (item.Rate > bestRate.Rate)
                                {
                                    bestRate.Rate = item.Rate;
                                    bestRate.CurrencyId = item.CurrencyId;
                                    bestRate.DataSourceId = item.DataSourceId;
                                    bestRate.Date = item.Date;
                                }
                            }
                            currencyExchangeEntities.BestRates.Add(bestRate);//DB
                        }
                        currencyExchangeEntities.SaveChanges();//DB
                        Program.logger.Info("Today best rates added successfully!");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Program.logger.Error("Error while adding today best rates.");
                Program.logger.Error(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// To fetch json data from resource Url into a generic type
        /// </summary>
        /// <returns>Generic type with Json data</returns>
        public static T GetDataFromSource<T>(string url) where T : new()
        {
            string dataString = string.Empty;
            T data = new T();
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    //Fetching raw joson data string from resource Url
                    dataString = webClient.DownloadString(url);
                    //Deserialize the raw json string into generic type
                    data = JsonConvert.DeserializeObject<T>(dataString);
                }
            }
            catch (Exception ex)
            {
                Program.logger.Error("Error while fetching data from source url.");
                Program.logger.Error(ex.Message);
            }
            return data;
        }
    }
}
