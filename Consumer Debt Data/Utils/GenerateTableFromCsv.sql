
USE Guardian
Go


/* Create the table */
Create Table CsvImport
(
RealCCN varchar(16),
RealSSN varchar(11),
RealFN	varchar(40),
RealLN	varchar(40),
RealCcHasFuzzyTrustSide varchar(5),
RealCcHasFuzzyDebtSide varchar(5),
TrustCCN varchar(16),
TrustSSN varchar(11),
TrustFN	varchar(40),
TrustLN	varchar(40),
TrustCreditCardFuzzFields int,
TrustCreditCardFuzzMethod int,
TrustConsumerFuzzFields	int,
TrustConsumerFuzzMethod	int,
DebtCCN	varchar(16),
DebtSSN	varchar(11),
DebtFN	varchar(40),
DebtLN varchar(40),
DebtCreditCardFuzzFields int,
DebtCreditCardFuzzMethod int,
DebtConsumerFuzzFields int,
DebtConsumerFuzzMethod int
)

BULK

INSERT CsvImport

FROM 'C:\Users\Stephen Torchia\Documents\Visual Studio 2008\Projects\Consumer Debt Data\Generated Data\MatchDump.csv'

WITH
(
FIRSTROW = 2,
FIELDTERMINATOR = ',',
ROWTERMINATOR = '\n'
)
GO
