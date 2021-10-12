create database Equities
go

use Equities
go

create table spy (date varchar(255), open_price float, high_price float, low_price float, close_price float, adj_close_price float, volume float, )

BULK INSERT spy FROM '/Historical_Data/SPY.csv' with (format = 'CSV', firstrow=2, fieldterminator = ',', rowterminator = '\n')

create table qqq (date varchar(255), open_price float, high_price float, low_price float, close_price float, adj_close_price float, volume float, )

BULK INSERT qqq FROM '/Historical_Data/QQQ.csv' with (format = 'CSV', firstrow=2, fieldterminator = ',', rowterminator = '\n')

create table spy_missingrows (date varchar(255), open_price float, high_price float, low_price float, close_price float, adj_close_price float, volume float)
bulk insert spy_missingrows from '/Historical_Data/SPY_missingrows.csv' with (format='CSV', firstrow=2, fieldterminator=',', rowterminator='\n')

create table qqq_missingrows (date varchar(255), open_price float, high_price float, low_price float, close_price float, adj_close_price float, volume float)
bulk insert qqq_missingrows from '/Historical_data/QQQ_missingrows.csv' with (format='CSV', firstrow=2, fieldterminator=',', rowterminator='\n')
