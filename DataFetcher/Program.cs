using System;
using System.Linq;
using System.Collections.Generic;
using NLog;
namespace DataFetcher
{
    class Program
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            //NLog setup
            NLog.LogManager.LoadConfiguration("NLog.config"); 
            Logger logger = LogManager.GetCurrentClassLogger();

            logger.Info("");
            logger.Info("Process Started");
            CurrencyExchangeEntities currencyExchangeEntities = new CurrencyExchangeEntities();

            try
            {
                var bestRates = currencyExchangeEntities.BestRates.FirstOrDefault();
                List<iDataResource> dataSources = new List<iDataResource>();
                dataSources.Add(new RateSource());
                dataSources.Add(new QuoteSource());
                //Add more datasources if any in the above list 

                //If no data (may be first time) then fetch and add
                ExchangeRateProcessor dataExchangeProcessor = new ExchangeRateProcessor(dataSources);
                bool isDone = true;
                //If old best rates exists then delete them
                if (bestRates != null && bestRates.Date != DateTime.Today)
                {
                    isDone = dataExchangeProcessor.DeleteOldBestRates();
                }

                //If no more bestrates then process 
                if (isDone && bestRates == null)
                {
                    //Add new symbols from new resources
                    isDone = dataExchangeProcessor.FetchAndAddNewSymbols();
                    if (isDone)
                    {
                        //Add today rates from all resources
                        isDone = dataExchangeProcessor.FetchAndInsertRates();
                        if (isDone)
                        {
                            //Find and add new best rates
                            isDone = dataExchangeProcessor.FindBestRatesAndAdd();
                            if (isDone)
                            {
                                logger.Info("Process Completed Successfully!");
                            }
                        }
                    }
                }
                else
                {
                    logger.Info("Data is uptodate!");
                    logger.Info("Process Completed Successfully!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error while doing the process.");
                logger.Info(ex.Message);
                logger.Error("Process not completed.");
            }
            
        }
    }
}
