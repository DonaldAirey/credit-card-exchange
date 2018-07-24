using System;
namespace FluidTrade.Core
{
	public interface IDataModelTransaction
	{
		void AddLock(IRow iRow);
		void AddRecord(IRow iRow);
		void Commit(System.Transactions.Enlistment enlistment);
		void InDoubt(System.Transactions.Enlistment enlistment);
		void Prepare(System.Transactions.PreparingEnlistment preparingEnlistment);
		void Rollback(System.Transactions.Enlistment enlistment);
		System.Data.SqlClient.SqlConnection SqlConnection { get; }
		Guid TransactionId { get; }
	}
}
