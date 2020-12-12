I have seperated the solution into two applications as "DataFetcher"  and "TodaysBestExchangeRates".
1.The "DataFetcher" is a batch process console application. It should be scheduled to run daily.
It will load the data into database from the data source urls. I have used NLog for logging. 
2.The "TodaysBestExchangeRates" application is the output application. It will just load the
data from Database and show to the user.

Set Up the Code:
Step 1
Create a databse "CurrencyExchange" in SqlServer

Step 2
Run the "CurrencyExchange.sql" on that DB.

Only "DataSource" table will have two records. Now DB is ready to load data. 

Step 3. Change the connection string in the App.config of "DataFetcher" application.

Step 4. Now Run the "DataFetcher" application once. Data will be loaded. Please confirm it in DB. 
If not please check the error log in the bin folder. 

Step 5. Now Run the "TodaysBestExchangeRates" application. 
You can change the "PrintMode" to "Console" or "MessageBox" and check the output.

Note: 
I have used visual studio 2019.
.Net 4.6
Sql Server Management studio 17
