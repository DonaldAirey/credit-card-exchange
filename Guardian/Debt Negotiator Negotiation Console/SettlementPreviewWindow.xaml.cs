using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FluidTrade.Core;

namespace FluidTrade.Guardian
{
    /// <summary>
    /// Interaction logic for SettlementPreviewWindow.xaml
    /// </summary>
    public partial class SettlementPreviewWindow : Window
    {
        /// <summary>
        /// SettlementInfo dependency property.
        /// </summary>
        public static readonly DependencyProperty SettlementInfoProperty = DependencyProperty.Register(
            "SettlementInfo", typeof(SettlementOffer), typeof(SettlementPreviewWindow),
            new PropertyMetadata(SettlementPreviewWindow.OnSettlementInfoPropertyChanged));

        /// <summary>
        /// The SettlementInfo property.
        /// </summary>
        public SettlementOffer SettlementInfo
        {
            get { return (SettlementOffer)this.GetValue(SettlementInfoProperty); }
            set { this.SetValue(SettlementInfoProperty, value); }
        }

        /// <summary>
        /// Handle changes of the DependencyProperty that is registered with this event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private static void OnSettlementInfoPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {

            // Extract the target of this property change from the parameters.
            SettlementPreviewWindow thisCtrl = sender as SettlementPreviewWindow;

        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SettlementPreviewWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Overloaded constructor.
        /// </summary>
        /// <param name="finalOffer">The SettlementOffer object.</param>
        public SettlementPreviewWindow(SettlementOffer finalOffer)
        {
            InitializeComponent();

            SettlementInfo = finalOffer;

        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {

            this.spinnerAccountBalance.Value = SettlementInfo.AccountBalance;
            this.spinnerSettlementCash.Value = SettlementInfo.SettlementCashValue;
            this.spinnerNumOfPayments.Value = SettlementInfo.NumberOfPayments;
            this.spinnerSettlementPercentage.Value = SettlementInfo.SettlementPercentage;
            this.termLengthPaymentLength.Length = SettlementInfo.PaymentStartDateLength;
            this.termLengthPaymentLength.Units = SettlementInfo.PaymentStartDateUnit;
            foreach (Guid guid in SettlementInfo.PaymentMethods)
                this.paymentMethodsCtrl.SelectedValues.Add(guid);

        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

		private void OnOkay(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        // PUBLIC CLASS
        
        /// <summary>
        /// Object that describes the settlement parameters.
        /// </summary>
        public class SettlementOffer
        {
            public Nullable<Decimal> AccountBalance { get; set; }
            public Nullable<Decimal> NumberOfPayments { get; set; }
            public Nullable<Decimal> PaymentStartDateLength { get; set; }
            public Nullable<Guid> PaymentStartDateUnit {get; set;}
            public Nullable<Decimal> SettlementPercentage { get; set; }
            public Nullable<Decimal> SettlementCashValue { get; set; }
            public IList PaymentMethods { get; set; }
        }
    }
}
