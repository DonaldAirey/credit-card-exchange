namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.ServiceModel;
	using System.Reflection;
	using System.Threading;
	using FluidTrade.Core;
	using System.Collections;
	using FluidTrade.Guardian.TradingSupportReference;

	/// <summary>
	/// Static methods useful for dealing with web service calls.
	/// </summary>
	public static class NetworkHelper
	{

		/// <summary>
		/// The maximum number of times to attempt a call thate fails for innocuous reasons.
		/// </summary>
		public static Int32 MaxCalls = 3;
		/// <summary>
		/// Number of milliseconds of wait to add between attempts.
		/// </summary>
		public static Int32 WaitTimeScale = 100;

		/// <summary>
		/// Attempt to make a call until it either succeeds, or a maximum number of probably innocuous errors occur.
		/// </summary>
		/// <typeParam name="T">The typeof the response. This must have an Errors property of type TradingSupportReference.ErrorInfo[].</typeParam>
		/// <param name="func">The call ot make.</param>
		/// <param name="records">The records to pass to func.</param>
		/// <param name="stopsOnError">If true, dead lock errors will continue from the point of failure, rather than simply retrying the failed
		/// records.</param>
		/// <param name="sentSize">The number of records per call.</param>
		public static T Attempt<T>(Func<TradingSupportClient, Array, T> func, Array records, Boolean stopsOnError, out Int32 sentSize)
		{

			ConstructorInfo recordsConstructor = records.GetType().GetConstructor(new Type[] { typeof(Int32) });
			T response;
			Int32 calls = 0;
			Boolean succeeded = false;
			Random rand = new Random((Int32)DateTime.Now.Ticks);
			
			sentSize = records.Length;

			do
			{

				Thread.Sleep(calls * (NetworkHelper.WaitTimeScale + rand.Next(NetworkHelper.WaitTimeScale)));

				try
				{

					try
					{

						List<object> retryRecords = new List<object>();
						ErrorInfo[] errors;
						TradingSupportClient tradingSupportClient =
							new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);

						calls += 1;
						response = func(tradingSupportClient, records);
						errors = response.GetErrors();

						succeeded = true;

						foreach (ErrorInfo error in errors)
							if (error.ErrorCode == ErrorCode.Deadlock)
							{

								succeeded = false;

								if (stopsOnError)
								{

									Array newArray = recordsConstructor.Invoke(new object[] { records.Length - error.BulkIndex }) as Array;

									Array.Copy(records, error.BulkIndex, newArray, 0, newArray.Length);
									records = newArray;
									System.Diagnostics.Debug.WriteLine("Retrying because of SQL deadlock");

									break;

								}
								else
								{

									retryRecords.Add(records.GetValue(error.BulkIndex));

								}

							}

						if (retryRecords.Count > 0 && stopsOnError)
						{

							records = retryRecords.ToArray();
							System.Diagnostics.Debug.WriteLine("Retrying because of SQL deadlock");

						}

						try
						{

							tradingSupportClient.Close();

						}
						catch (Exception exception)
						{

							EventLog.Information(String.Format(
								"Unable to close client after successful transaction: {0}: {1}\n{2}",
								exception.GetType(), exception.Message,
								exception.StackTrace));

						}

					}
					catch (TargetInvocationException exception)
					{

						throw exception.InnerException;

					}

				}
				catch (CommunicationObjectFaultedException)
				{

					if (records.Length == 1)
						throw;

					Thread.Sleep(NetworkHelper.WaitTimeScale);

					response = NetworkHelper.Reattempt(func, records, stopsOnError, out sentSize);

				}

			} while (calls < NetworkHelper.MaxCalls && !succeeded);

			return response;

		}

		/// <summary>
		/// Get the Errors field of a response object.
		/// </summary>
		/// <param name="response">The response object.</param>
		/// <returns>The errors from the response.</returns>
		private static TradingSupportReference.ErrorInfo[] GetErrors(this object response)
		{

			return response.GetType().GetProperty("Errors", typeof(TradingSupportReference.ErrorInfo[])).GetValue(response, new object[0]) as TradingSupportReference.ErrorInfo[];

		}
		
		/// <summary>
		/// Set the Errors field of a response object.
		/// </summary>
		/// <param name="response">The response object.</param>
		/// <param name="errors">The errors.</param>
		private static void SetErrors(this object response, TradingSupportReference.ErrorInfo[] errors)
		{

			response.GetType().GetProperty("Errors", typeof(TradingSupportReference.ErrorInfo[])).SetValue(response, errors, new object[0]);

		}

		/// <summary>
		/// Get the Result field of a response object.
		/// </summary>
		/// <param name="response">The response object.</param>
		/// <returns>The result.</returns>
		private static object GetResult(this object response)
		{

			return response.GetType().GetProperty("Result").GetValue(response, new object[0]);

		}

		/// <summary>
		/// Set the Result field of a response object.
		/// </summary>
		/// <param name="response">The response object.</param>
		/// <param name="result">The result.</param>
		private static void SetResult(this object response, object result)
		{

			response.GetType().GetProperty("Result").SetValue(response, result, new object[0]);

		}

		/// <summary>
		/// Get the IsSuccessful flag of a response object.
		/// </summary>
		/// <param name="response">The response object.</param>
		/// <returns>The value of the IsSuccessful flag.</returns>
		private static Boolean GetIsSuccessful(this object response)
		{

			return (Boolean)response.GetType().GetProperty("IsSuccessful", typeof(Boolean)).GetValue(response, new object[0]);

		}

		/// <summary>
		/// Break a record set in two so it can be re-attempted, the reassemble the response from the two Attempt calls into a single response.
		/// </summary>
		/// <typeParam name="T">The typeof the response. This must have an Errors property of type TradingSupportReference.ErrorInfo[].</typeParam>
		/// <param name="func">The call ot make.</param>
		/// <param name="records">The records to pass to func.</param>
		/// <param name="stopsOnError">If true, dead lock errors will continue from the point of failure, rather than simply retrying the failed
		/// records.</param>
		/// <param name="sentSize">The number of records per call.</param>
		private static T Reattempt<T>(Func<TradingSupportClient, Array, T> func, Array records, Boolean stopsOnError, out Int32 sentSize)
		{

			T response;
			T[] responses;
			Array[] splitRecords;
			ConstructorInfo recordsConstructor = records.GetType().GetConstructor(new Type[] { typeof(Int32) });
			ConstructorInfo responseConstructor = typeof(T).GetConstructor(new Type[0]);
			Int32 splitSize;

			response = (T)responseConstructor.Invoke(new object[0]);

			splitRecords = new Array[2];
			responses = Array.CreateInstance(typeof(T), 2) as T[];

			splitRecords[0] = recordsConstructor.Invoke(new object[] { records.Length / 2 }) as Array;
			splitRecords[1] = recordsConstructor.Invoke(new object[] { records.Length / 2 + records.Length % 2 }) as Array;

			System.Diagnostics.Debug.WriteLine(
				String.Format("NetworkHelper split {0} records into sizes of {1} and {2} to try again.", typeof(T), splitRecords[0].Length, splitRecords[1].Length));

			Array.Copy(records, splitRecords[0], splitRecords[0].Length);
			Array.Copy(records, splitRecords[0].Length, splitRecords[1], 0, splitRecords[1].Length);

			responses[0] = NetworkHelper.Attempt(func, splitRecords[0], stopsOnError, out sentSize);
			response.SetResult(responses[0].GetResult());
			response.SetErrors(responses[0].GetErrors());

			if (responses[0].GetIsSuccessful())
			{

				responses[1] = NetworkHelper.Attempt(func, splitRecords[1], stopsOnError, out splitSize);

				response.SetResult(responses[1].GetResult());
				response.SetErrors(new TradingSupportReference.ErrorInfo[responses[0].GetErrors().Length + responses[1].GetErrors().Length]);

				Array.Copy(responses[0].GetErrors(), response.GetErrors(), responses[0].GetErrors().Length);

				foreach (TradingSupportReference.ErrorInfo error in responses[1].GetErrors())
					error.BulkIndex += splitRecords[0].Length;

				Array.Copy(responses[1].GetErrors(), 0, response.GetErrors(), responses[0].GetErrors().Length, responses[1].GetErrors().Length);

				if (splitSize < sentSize)
					sentSize = splitSize;

			}

			return response;

		}

	}

}
