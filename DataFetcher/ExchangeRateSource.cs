using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFetcher
{
    /// <summary>
    /// Interface to enforce datasource implementation rules
    /// </summary>
    interface iDataResource
    {
        List<string> FetchNewSymbols();
        List<ExchangeRate> FetchTodayExchangeRates();
    }
    /// <summary>
    /// Defferent datasource type implementations
    /// </summary>
    class RateSource : iDataResource
    {
        public Dictionary<string, decimal> Rates { get; set; }
        readonly string _dataElementName;

        public RateSource()
        {
            _dataElementName = "RATES";
        }

        /// <summary>
        /// To Fetch New Symbols
        /// </summary>
        /// <returns>New Symbols</returns>
        public List<string> FetchNewSymbols()
        {
            List<string> newSymbols = new List<string>();
            try
            {
                CurrencyExchangeEntities currencyExchangeEntities = new CurrencyExchangeEntities();
                List<string> oldSymbols = currencyExchangeEntities.Currencies.Select(x => x.Code).ToList();
                //Fetching Quote resource data
                var dataSources = currencyExchangeEntities.DataSources.Where(x => x.IsNew == true && x.DataElementName == _dataElementName);
                foreach (var dataSource in dataSources)
                {
                    //Fetching data from resource
                    RateSource currencyRates = ExchangeRateProcessor.GetDataFromSource<RateSource>(dataSource.Url);
                    if (currencyRates.Rates == null)
                    {
                        Program.logger.Error("Unable to fetch data from resource Url.");
                        return newSymbols;
                    }
                    else 
                    {
                        //Taking symbols alone
                        List<string> CurrencySymbols = currencyRates.Rates.Select(x => x.Key.Trim().ToUpper()).ToList();
                        newSymbols.AddRange(CurrencySymbols);
                    }
                }
                Program.logger.Info("RateSource: new symbols fetched successfully!");
            }
            catch (Exception ex)
            {
                Program.logger.Error("RateSource: Error while fetching new symbols.");
                Program.logger.Error(ex.Message);
            }

            return newSymbols;
        }

        /// <summary>
        /// To Fetch Today Exchange Rates
        /// </summary>
        /// <returns>Today Exchange Rates</returns>
        public List<ExchangeRate> FetchTodayExchangeRates()
        {
            CurrencyExchangeEntities currencyExchangeEntities = new CurrencyExchangeEntities();
            List<ExchangeRate> todayExchangeRates = new List<ExchangeRate>();
            try
            {
                //Fetching Quote resource data
                var dataSources = currencyExchangeEntities.DataSources.Where(x => x.DataElementName.Trim().ToUpper() == _dataElementName);
                foreach (var datasource in dataSources)
                {
                    //Fetching data from resource
                    RateSource currencyRates = ExchangeRateProcessor.GetDataFromSource<RateSource>(datasource.Url);
                    if (currencyRates.Rates == null)
                    {
                        Program.logger.Error("Unable fetch data from resource Url.");
                        return todayExchangeRates;
                    }
                    else
                    {
                        //Processing and taking rates
                        foreach (var rate in currencyRates.Rates)
                        {
                            ExchangeRate exchangeRate = new ExchangeRate();
                            exchangeRate.DataSourceId = datasource.Id;
                            //Fetching corresponding symbol code from DB
                            var cid = currencyExchangeEntities.Currencies.Where(x => x.Code == rate.Key.Trim().ToUpper()).FirstOrDefault();
                            exchangeRate.CurrencyId = cid.Id;
                            exchangeRate.Rate = rate.Value;
                            exchangeRate.Date = DateTime.Today;
                            todayExchangeRates.Add(exchangeRate);
                        }
                    }
                }
                Program.logger.Info("RateSource: Today exchange rates fetched successfully!");
            }
            catch (Exception ex)
            {
                Program.logger.Error("RateSource: Error while fetch today exchange rates.");
                Program.logger.Error(ex.Message);
            }
            return todayExchangeRates;
        }

    }
    class QuoteSource : iDataResource
    {
        public Dictionary<string, decimal> Quotes { get; set; }
        readonly string _dataElementName;

        public QuoteSource()
        {
            _dataElementName = "QUOTES";
        }

        /// <summary>
        /// To fetch new symbols from new resources
        /// </summary>
        /// <returns>New symbols</returns>
        public List<string> FetchNewSymbols()
        {
            List<string> newSymbols = new List<string>();
            CurrencyExchangeEntities currencyExchangeEntities = new CurrencyExchangeEntities();
            try
            {
                List<string> oldSymbols = currencyExchangeEntities.Currencies.Select(x => x.Code).ToList();
                //Fetching Quote resource data
                var dataSources = currencyExchangeEntities.DataSources.Where(x => x.IsNew == true && x.DataElementName == _dataElementName);
                foreach (var dataSource in dataSources)
                {
                    //Fetching data from resource
                    QuoteSource currencyQuotes = ExchangeRateProcessor.GetDataFromSource<QuoteSource>(dataSource.Url);
                    if (currencyQuotes.Quotes == null)
                    {
                        Program.logger.Error("Unable to fetch data from resource Url.");
                        return newSymbols;
                    }
                    else
                    {
                        //Processing and taking symbols alone
                        List<string> CurrencySymbols = currencyQuotes.Quotes.Select(x => x.Key.Replace("USD", "").Trim().ToUpper()).ToList();
                        newSymbols.AddRange(CurrencySymbols);
                    }
                }
                Program.logger.Info("QuoteSource: New symbols fetched succesfully!");
            }
            catch (Exception ex)
            {
                Program.logger.Error("QuoteSource: Error while fetching new symbols.");
                Program.logger.Error(ex.Message);
            }
            return newSymbols;
        }

        /// <summary>
        /// To Fetch Today Exchange Rates
        /// </summary>
        /// <returns>Today Exchange Rates</returns>
        public List<ExchangeRate> FetchTodayExchangeRates()
        {
            CurrencyExchangeEntities currencyExchangeEntities = new CurrencyExchangeEntities();
            List<ExchangeRate> todayExchangeRates = new List<ExchangeRate>();
            try
            {
                //Fetching Quote resource data
                var QuoteDataSources = currencyExchangeEntities.DataSources.Where(x => x.DataElementName.Trim().ToUpper() == _dataElementName);
                foreach (var datasource in QuoteDataSources)
                {
                    //Fetching data from resource
                    QuoteSource currencyQuotes = ExchangeRateProcessor.GetDataFromSource<QuoteSource>(datasource.Url);
                    if (currencyQuotes.Quotes == null)
                    {
                        Program.logger.Error("Unable to fetch data from resource Url.");
                        return todayExchangeRates;
                    }
                    else
                    {
                        //Processing and taking rates
                        foreach (var rate in currencyQuotes.Quotes)
                        {
                            ExchangeRate exchangeRate = new ExchangeRate();
                            exchangeRate.DataSourceId = datasource.Id;
                            //Fetching corresponding symbol code from DB
                            var cid = currencyExchangeEntities.Currencies.Where(x => x.Code == rate.Key.Replace("USD", "").Trim().ToUpper()).FirstOrDefault();
                            exchangeRate.CurrencyId = cid.Id;
                            exchangeRate.Rate = rate.Value;
                            exchangeRate.Date = DateTime.Today;
                            todayExchangeRates.Add(exchangeRate);
                        }
                    }
                }
                Program.logger.Info("QuoteSource: Today exchange rates fetched successfully");
            }
            catch (Exception ex)
            {
                Program.logger.Error("QuoteSource: Error while fetching today exchange rates.");
                Program.logger.Error(ex.Message);
            }
            return todayExchangeRates;
        }
    }
}
