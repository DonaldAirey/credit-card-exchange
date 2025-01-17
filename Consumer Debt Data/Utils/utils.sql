
select * FROM [Guardian].[dbo].[CsvImport] where RealCcHasFuzzyTrustSide = 'true'
 
select * FROM [Guardian].[dbo].[CsvImport] where TrustCCN <> DebtCCN
  
select * FROM [Guardian].[dbo].[CsvImport] where TrustSSN <> DebtSSN
  

/* Matching Statistics */  
declare @runningTime decimal
declare @count decimal
declare @countTrust decimal
declare @countDebt decimal
declare @duration decimal
declare @time1 Datetime
declare @time2 Datetime
declare @predictedPeriod decimal

select @count = COUNT(*) from Match
select @time1 = MAX(Match.MatchedTime) from Match
select @time2 = MIN(Match.MatchedTime) from Match
select @runningTime = DateDiff(SECOND, @time2, @time1)
select @countTrust = COUNT(*) from ConsumerTrust
select @countDebt = COUNT(*) from ConsumerDebt

select
      'RunTime' = Round(@runningTime / (60.0 * 60.0), 2),
      'Matches' = @count,
      'Matches per Minute' = @count / (@runningTime / 60),
      'ETC' = (40000 - @count) / (@count * 60.0 * 60.0 / @runningTime),
      'Loop Time' = Round(@runningTime / (@countTrust * @countDebt) * 1000000, 1)


/* Matching Statistics */ 
select M.HeatIndex, C.LastName, C.SocialSecurityNumber, C_C.LastName, C_C.SocialSecurityNumber,
            CC.OriginalAccountNumber, C_CC.OriginalAccountNumber
from match M
join WorkingOrder WO on M.WorkingOrderId = WO.WorkingOrderId
join Security S on WO.SecurityId = S.SecurityId
join ConsumerDebt CD on S.SecurityId = CD.ConsumerDebtId
Join Consumer C on C.ConsumerId = CD.ConsumerId
Join CreditCard CC on CD.CreditCardId = CC.CreditCardId
join WorkingOrder C_WO on M.ContraOrderId = C_WO.WorkingOrderId
join Security C_S on C_WO.SecurityId = C_S.SecurityId
join ConsumerTrust C_CT on C_S.SecurityId = C_CT.ConsumerTrustId
Join Consumer C_C on C_C.ConsumerId = C_CT.ConsumerId
JOIN ConsumerTrustNegotiation CTN on CTN.MatchId = M.ContraMatchId
JOIN CreditCard C_CC ON C_CC.CreditCardId = CTN.CreditCardId
order by C.LastName


/* Find 'Duplicates' */
SELECT OriginalAccountNumber,
COUNT(OriginalAccountNumber) AS NumOccurrences
FROM CreditCard
GROUP BY OriginalAccountNumber
HAVING ( COUNT(OriginalAccountNumber) > 1 )


SELECT SocialSecurityNumber,
COUNT(SocialSecurityNumber) AS NumOccurrences
FROM Consumer
GROUP BY SocialSecurityNumber
HAVING ( COUNT(SocialSecurityNumber) > 1 )


/* Don-supplied debugging assistants */ 

select * from Consumer where 'Verderpol' = "LastName"
select * from Consumer where 'Vanderpol' = "FirstName"
select * from ConsumerDebtNegotiation
 
select * from Match

where WorkingOrderId in

(
      select WorkingOrderId from WorkingOrder where SecurityId in

      (
            select SecurityId from Security

            where SecurityId in

            (
                  select ConsumerDebtId from ConsumerDebt where ConsumerId in

                  (
                        select ConsumerId from Consumer where 'Verderpol' = "LastName"
                  )
            )
      )
)

 

select * from Consumer

where ConsumerId in
(
      select ConsumerId from ConsumerTrust

      where ConsumerTrust.ConsumerTrustId in
      (
            select SecurityId from WorkingOrder

            where WorkingOrderId in
            (
                  select ContraOrderId from Match

                  where WorkingOrderId in
                  (
                        select WorkingOrderId from WorkingOrder where SecurityId in
                        (
                              select SecurityId from Security

                              where SecurityId in
                              (
                                    select ConsumerDebtId from ConsumerDebt where ConsumerId in
                                    (
                                          select ConsumerId from Consumer where 'Verderpol' = "LastName"
                                    )
                              )
                        )
                  )
            )
      )
)
 

select * from CreditCard

where ConsumerId in
(
      select ConsumerId from Consumer
      where ConsumerId in
      (
            select ConsumerId from ConsumerTrust

            where ConsumerTrust.ConsumerTrustId in
            (
                  select SecurityId from WorkingOrder

                  where WorkingOrderId in ('FA133BE5-5A99-42DD-9624-114F1BF73FE1', 'BFE9FA0E-8CDA-47C9-9AD7-AF956EB10BBE')
            )
      )
)

 