namespace FluidTrade.Core.Windows.Utilities
{

	using System;

    /// <summary>
    /// Library extension methods
    /// </summary>
    public static class LibraryHelper
    {
        /// <summary>
        /// Use Linq to do chain Null checks.  So instead of if(Consumer != Null && Consumer.CreditCard != Null .... Consumer.CreditCard.x.x.x != Null)
        /// we can write it as if(Consumer.NullSafe(x => x.CreditCard).NullSafe(....)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="func"></param>
        public static TResult NullSafe<T, TResult>(this T target, Func<T, TResult> func)
        {
            if (target != null)
                return func(target);
            else
                return default(TResult);
        }
    }
}
