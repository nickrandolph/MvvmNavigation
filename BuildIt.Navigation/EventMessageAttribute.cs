using System;
using System.Collections.Generic;
using System.Text;

namespace BuildIt.Navigation
{
    [AttributeUsage(AttributeTargets.Event)]
    public class EventMessageAttribute:Attribute
    {
        public Type MessageType { get;  }

        public EventMessageAttribute(Type messageType)
        {
            MessageType = messageType;
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
}
