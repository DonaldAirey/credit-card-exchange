namespace FluidTrade.Guardian.Windows
{

    using System;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// A Xaml 'container' for building (and calling) method-call trees.
    /// </summary>
    public class Builder : ItemsControl
    {

        /// <summary>
        /// Dependency property for the Assembly property.
        /// </summary>
        public static DependencyProperty AssemblyProperty;
        /// <summary>
        /// Dependency property for the Callee property.
        /// </summary>
        public static DependencyProperty CalleeProperty;
        /// <summary>
        /// Dependency property for the Method property.
        /// </summary>
        public static DependencyProperty MethodProperty;
        /// <summary>
        /// Dependency property for the Value property.
        /// </summary>
        public static DependencyProperty ValueProperty;

        static Builder()
        {

            Builder.AssemblyProperty = DependencyProperty.Register("Assembly", typeof(String), typeof(Builder));
            Builder.CalleeProperty = DependencyProperty.Register("Callee", typeof(Object), typeof(Builder));
            Builder.MethodProperty = DependencyProperty.Register("Method", typeof(String), typeof(Builder));
            Builder.ValueProperty = DependencyProperty.Register("Value", typeof(Object), typeof(Builder));

        }

        /// <summary>
        /// Create an empty Builder.
        /// </summary>
        public Builder()
        {

        }

        /// <summary>
        /// The name of the assembly where the Callee can be found. If Assembly is set, Callee should be a string that contains a fully qualified Type
        /// name.
        /// </summary>
        public String Assembly
        {

            get { return this.GetValue(Builder.AssemblyProperty) as String; }
            set { this.SetValue(Builder.AssemblyProperty, value); }

        }

        /// <summary>
        /// The object or type that the Method will be called on.
        /// </summary>
        public Object Callee
        {

            get { return this.GetValue(Builder.CalleeProperty); }
            set { this.SetValue(Builder.CalleeProperty, value); }

        }


        /// <summary>
        /// Evaluate the arguments as Builder objects and return their Values.
        /// </summary>
        /// <returns>The array of the effective values of the arguments.</returns>
        private Object[] ConvertArguments()
        {

            Object[] arguments = null;

            if (this.Items != null && this.Items.Count > 0)
            {

                arguments = new Object[this.Items.Count];

                for (int index = 0; index < this.Items.Count; ++index)
                {

                    Builder item = this.Items[index] as Builder;
                    item.DoCall();
                    arguments[index] = item.Value;

                }

            }
            else
            {

                arguments = new Object[0];

            }

            return arguments;

        }

        /// <summary>
        /// Evaluate method call represented by this object. A method call can only be made if both Callee and Method are set. All arguments are
        /// evaluated whether or not a method call actually takes place. Value is only set if a method actually takes place.
        /// </summary>
        public void DoCall()
        {

            Object[] methodArguments = ConvertArguments();

            if (this.Callee != null && this.Method != null)
            {
                MethodInfo method = null;
                Type[] parameterTypes = methodArguments.Select(obj => obj == null? null : obj.GetType()).ToArray();
                Object callee = this.Callee;


                if (this.Callee is String && this.Assembly != null)
                {

                    Assembly assembly = System.Reflection.Assembly.Load(this.Assembly);
                    
                    callee = assembly.GetType(this.Callee as String, false, false);

                }

                method = callee is Type ? (callee as Type).GetMethod(this.Method, parameterTypes) : callee.GetType().GetMethod(this.Method);

                object returnValue = method.Invoke(this.Callee, methodArguments);

                if (method.IsConstructor)
                    this.SetValue(Builder.ValueProperty, this.Callee);
                else
                    this.SetValue(Builder.ValueProperty, returnValue);

            }
            else if (this.Callee != null && this.Assembly != null)
            {

                Assembly assembly = System.Reflection.Assembly.Load(this.Assembly);

                this.Value = assembly.GetType(this.Callee as String, false, false);

            }

        }

        /// <summary>
        /// The name of the method to call on the Callee.
        /// </summary>
        public String Method
        {

            get { return this.GetValue(Builder.MethodProperty) as String; }
            set { this.SetValue(Builder.MethodProperty, value); }

        }

        /// <summary>
        /// The result of evaluating the method call represented by this object, or null for methods returning void.
        /// </summary>
        public Object Value
        {

            get { return this.GetValue(Builder.ValueProperty); }
            set { this.SetValue(Builder.ValueProperty, value); }

        }

    }

}
