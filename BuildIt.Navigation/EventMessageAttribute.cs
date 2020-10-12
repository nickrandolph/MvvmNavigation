using System;
using System.Collections.Generic;
using System.Text;

namespace BuildIt.Navigation
{
    [AttributeUsage(AttributeTargets.Event)]
    public class EventMessageAttribute:Attribute
    {
        public Type MessageType { get;  }

        public Object MessageParameter { get; set; }

        public EventMessageAttribute(Type messageType, object parameter=null)
        {
            MessageType = messageType;
            MessageParameter = parameter;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ViewModelAttribute : Attribute
    {
        public Type ViewModelType { get; }
        public string InitMethodName { get; }

        public ViewModelAttribute(Type viewModelType, string initMethod=null)
        {
            ViewModelType = viewModelType;
            InitMethodName = initMethod;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ApplicationServiceAttribute : Attribute
    {
        public string RegistrationMethod { get;  }

        public ApplicationServiceAttribute(string registrationMethod)
        {
            RegistrationMethod = registrationMethod;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ApplicationAttribute : Attribute
    {
        public string RegistrationMappingMethod { get; }

        public ApplicationAttribute(string registrationMethod)
        {
            RegistrationMappingMethod = registrationMethod;
        }
    }
}
