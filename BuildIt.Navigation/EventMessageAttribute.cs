﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BuildIt.Navigation
{
    [AttributeUsage(AttributeTargets.Event)]
    public class EventMessageAttribute : Attribute
    {
        public Type MessageType { get; }

        public Object MessageParameter { get; set; }

        public EventMessageAttribute(Type messageType, object parameter = null)
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

        public ViewModelAttribute(Type viewModelType, string initMethod = null)
        {
            ViewModelType = viewModelType;
            InitMethodName = initMethod;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ApplicationServiceAttribute : Attribute
    {
        public string RegistrationMethod { get; }
        public string RegistrationServicesMethod { get; }

        public ApplicationServiceAttribute(string registrationMethod, string serviceRegistrationMethod)
        {
            RegistrationMethod = registrationMethod;
            RegistrationServicesMethod = serviceRegistrationMethod;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ViewModelMappingRegisterAttribute : Attribute
    {
        public string RegistrationMappingMethod { get; }

        public ViewModelMappingRegisterAttribute(string registrationMethod)
        {
            RegistrationMappingMethod = registrationMethod;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterAttribute : Attribute
    {
    }
}
