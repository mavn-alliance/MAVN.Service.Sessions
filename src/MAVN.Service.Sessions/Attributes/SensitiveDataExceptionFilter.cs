using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Log;
using Lykke.Common.Log;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace MAVN.Service.Sessions.Attributes
{
    public class SensitiveDataExceptionFilter : IExceptionFilter, IActionFilter
    {
        private readonly ILog _log;

        private const string ControllerRouteKey = "Controller";

        private const string ActionRouteKey = "Action";

        private readonly IDictionary<string, string> _sensitiveData;


        public SensitiveDataExceptionFilter(ILogFactory log)
        {
            _log = log.CreateLog(this);
            _sensitiveData = new Dictionary<string, string>();
        }

        public void OnException(ExceptionContext context)
        {
            if (context.ExceptionHandled)
                return;

            var controllerName = context.HttpContext.GetRouteValue(ControllerRouteKey) as string;

            var actionName = context.HttpContext.GetRouteValue(ActionRouteKey) as string;

            context.HttpContext.Request.Path = FilterSensitiveData(context.HttpContext.Request.Path);

            context.HttpContext.Request.QueryString =
                new QueryString(FilterSensitiveData(context.HttpContext.Request.QueryString.Value));

            FilterSensitiveData(context.Exception);

            _log.Error(nameof(SensitiveDataExceptionFilter), context.Exception, $"{controllerName}, {actionName}");

            context.ExceptionHandled = true;

            context.Result = new ContentResult
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!(context.ActionDescriptor is ControllerActionDescriptor descriptor)) return;

            var parameters = descriptor.MethodInfo.GetParameters();

            foreach (var parameter in parameters)
            {
                var parameterValue = context.ActionArguments[parameter.Name];

                FindSensitiveData(parameter, parameterValue);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Do nothing
        }

        private static bool IsSensitiveData(ICustomAttributeProvider provider)
        {
            return provider != null && provider.IsDefined(typeof(SensitiveDataAttribute), false);
        }

        private void FindSensitiveData(ParameterInfo parameter, object parameterValue)
        {
            if (parameter == null || parameterValue == null)
                return;

            var parameterType = parameter.ParameterType;

            var parameterName = parameter.Name;

            if (IsSensitiveData(parameter))
                _sensitiveData.Add(parameterName, $"{parameterValue}");

            var props = parameterType.GetProperties().Where(IsSensitiveData);

            foreach (var prop in props)
            {
                var sensitiveValue = prop.GetValue(parameterValue);
                _sensitiveData.Add($"{parameterName}.{prop.Name}", $"{sensitiveValue}");
            }
        }

        /// <summary>
        ///     Method that removes all occurences of sensitive data inside exception and inner exceptions messages.
        ///     This method modifies exception message by using reflection!
        /// </summary>
        /// <param name="exception">Exception to remove sensitive data from.</param>
        private void FilterSensitiveData(Exception exception)
        {
            var tempException = exception;

            while (tempException != null)
            {
                var safeMessage = FilterSensitiveData(tempException.Message);

                var messageField = tempException.GetType()
                    ?.GetField("_message", BindingFlags.Instance | BindingFlags.NonPublic);

                messageField?.SetValue(tempException, safeMessage);

                tempException = tempException.InnerException;
            }
        }

        private string FilterSensitiveData(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var fileteredInput = input;

            foreach (var data in _sensitiveData)
            {
                var placeholder = data.Key;
                var sensitiveValue = data.Value;

                if (fileteredInput.Contains(sensitiveValue))
                    fileteredInput = fileteredInput.Replace(sensitiveValue, placeholder);
            }

            return fileteredInput;
        }
    }
}