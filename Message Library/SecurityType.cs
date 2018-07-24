namespace FluidTrade.Core
{

	using System;

	/// <summary>SecurityTypes for a FIX Message</summary>
	[Serializable()]
	public enum SecurityType
	{
		Future = 0,
		Option = 1

		/* Here are the rest from the FIX spec if they're ever needed:
		 BA = BankersAcceptance 
		 CB = ConvertibleBond 
		 CD = CertificateOfDeposit 
		 CMO = CollateralizeMortgageObligation 
		 CORP = CorporateBond 
		 CP = CommercialPaper 
		 CPP = CorporatePrivatePlacement 
		 CS = CommonStock
		 FHA = FederalHousingAuthority 
		 FHL = FederalHomeLoan 
		 FN = FederalNationalMortgageAssociation
		 FOR = ForeignExchangeContract 
		 FUT = Future 
		 GN = GovernmentNationalMortgageAssociation
		 GOVT = TreasuriesAndAgencyDebenture 
		 IET = Mortgage IOETTE 
		 MF = Mutual Fund 
		 MIO = Mortgage Interest Only 
		 MPO = MortgagePrincipalOnly 
		 MPP = MortgagePrivatePlacement 
		 MPT = MiscellaneousPassThru
		 MUNI = MunicipalBond  
		 NONE = No ISITC Security Type 
		 OPT = Option 
		 PS = PreferredStock 
		 RP = RepurchaseAgreement 
		 RVRP = ReverseRepurchaseAgreement
		 SL = StudentLoanMarketingAssociation 
		 TD = TimeDeposit   
		 USTB = UsTreasuryBill
		 WAR = Warrant    
		 ZOO = CatsTigersAndLions (a real code: US Treasury Receipts) 
		 */
	}

}
