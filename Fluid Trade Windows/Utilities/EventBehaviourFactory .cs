using System;
using System.Windows;
using System.Windows.Input;

namespace FluidTrade.Core.Windows.Utilities
{
    internal static class EventBehaviourFactory
    {
        public static DependencyProperty CreateCommandExecutionEventBehaviour(RoutedEvent routedEvent, string propertyName, Type ownerType)
        {
            DependencyProperty property = DependencyProperty.RegisterAttached(propertyName, typeof(ICommand), ownerType,
                                    new PropertyMetadata(null,
                                        new ExecuteCommandOnRoutedEventBehaviour(routedEvent).PropertyChangeHandler));

            return property;
        }

        /// <summary>
        /// An internal class to handle listening for an event and executing a command.
        /// </summary>
        private class ExecuteCommandOnRoutedEventBehaviour : ExecuteCommandBehaviour
        {
            private readonly RoutedEvent m_routedEvent;

            public ExecuteCommandOnRoutedEventBehaviour(RoutedEvent routedEvent)
            {
                m_routedEvent = routedEvent;
            }

            protected override void AdjustEventHandlers(DependencyObject sender, object oldValue, object newValue)
            {
                UIElement element = sender as UIElement;

                if (element == null)
                {
                    return;
                }

                if (oldValue != null)
                {
                    element.RemoveHandler(m_routedEvent, new RoutedEventHandler(EventHandler));
                }

                if (newValue != null)
                {
                    element.AddHandler(m_routedEvent, new RoutedEventHandler(EventHandler));
                }
            }

            protected void EventHandler(object sender, RoutedEventArgs e)
            {
                HandleEvent(sender, e);
            }

        }

        internal abstract class ExecuteCommandBehaviour
        {
            protected DependencyProperty m_property;
            protected abstract void AdjustEventHandlers(DependencyObject sender, object oldvalue, object newValue);

            protected void HandleEvent(object sender, EventArgs e)
            {
                DependencyObject dp = sender as DependencyObject;
                if (dp == null)
                {
                    return;
                }

                ICommand command = dp.GetValue(m_property) as ICommand;

                if (command == null)
                    return;

                if (command.CanExecute(e))
                {
                    command.Execute(e);
                }
            }

            public void PropertyChangeHandler(DependencyObject sender, DependencyPropertyChangedEventArgs  e)
            {
                if (m_property == null)
                {
                    m_property = e.Property;
                }

                object oldValue = e.OldValue;
                object newValue = e.NewValue;

                AdjustEventHandlers(sender, oldValue, newValue); 
            }
        }

    }
}
