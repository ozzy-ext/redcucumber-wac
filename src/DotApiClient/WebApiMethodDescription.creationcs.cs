﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotApiClient
{
    partial class WebApiMethodDescription
    {
        public static WebApiMethodDescription Create(MethodInfo method)
        {
            var d = new WebApiMethodDescription
            {
                MethodId = CreateMethodId(method.Name, method.GetParameters().Select(p => p.Name))
            };

            ContentType defaultSubmitContentType;

            InitRelPathAndHttpMethod(method, d, out defaultSubmitContentType);

            ProcessSinglePostBinaryCase(method, d, ref defaultSubmitContentType);
            ProcessSingleStringCase(method, d, ref defaultSubmitContentType);

            InitContentType(method, d, defaultSubmitContentType);

            InitHeaders(method, d);

            InitUrlParrtParameters(method, d);

            InitParameters(method, d);

            CheckParametersConflict(d.Parameters);

            return d;
        }

        private static void InitUrlParrtParameters(MethodInfo method, WebApiMethodDescription d)
        {
            d.UrlPartParameters = method.GetParameters()
                .Where(p => d.Headers.All(h => h.ParameterName != p.Name))
                .Where(p => Attribute.IsDefined(p, typeof(RestIdAttribute)))
                .Select(p => p.Name)
                .ToArray();
        }

        private static void InitHeaders(MethodInfo method, WebApiMethodDescription d)
        {
            var parameters = method.GetParameters();
            d.Headers = parameters
                    .Where(p => Attribute.IsDefined(p, typeof(HeaderAttribute)))
                    .Select(p => new WebApiMethodHeader
                    {
                        ParameterName = p.Name,
                        HeaderName = p.GetCustomAttribute<HeaderAttribute>().HeaderName
                    })
                    .ToArray();
        }

        private static void InitRelPathAndHttpMethod(MethodInfo method,
            WebApiMethodDescription d,
            out ContentType defaultSubmitContentType)
        {
            var srvEp = method.GetCustomAttribute<ServiceEndpointAttribute>();
            if (srvEp != null)
            {
                d.RelPath = srvEp.Path ?? method.Name;
                d.HttpMethod = srvEp.Method;

                defaultSubmitContentType = ContentType.UrlEncodedForm;
            }
            else
            {
                var rcEp = method.GetCustomAttribute<RestActionAttribute>();
                if (rcEp != null)
                {
                    d.HttpMethod = rcEp.Method;
                    defaultSubmitContentType = ContentType.Json;
                }
                else
                {
                    throw new WebApiContractException("Method is not endpoint");
                }
            }
        }

        private static void InitContentType(MethodInfo method,
            WebApiMethodDescription d,
            ContentType defaultSubmitContentType)
        {
            if (d.HttpMethod == HttpMethod.Get || d.HttpMethod == HttpMethod.Delete)
            {
                d.ContentType = ContentType.Undefined;
            }
            else
            {
                var contentTypeAttr = method.GetCustomAttribute<ContentTypeAttribute>();
                d.ContentType = contentTypeAttr?.ContentType ?? defaultSubmitContentType;
            }
        }

        private static void InitParameters(MethodInfo method, WebApiMethodDescription d)
        {
            var parameters = method.GetParameters()
                .Where(p => d.Headers.All(h => h.ParameterName != p.Name))
                .Where(p => d.UrlPartParameters.All(up => up != p.Name))
                .Select(WebApiParameterDescription.Create)
                .ToArray();

            foreach (var p in parameters.Where(prm => prm.Type == WebApiParameterType.Undefined))
            {
                if (d.HttpMethod == HttpMethod.Get)
                {
                    p.Type = WebApiParameterType.Url;
                }
                else
                {
                    switch (d.ContentType)
                    {
                        case ContentType.UrlEncodedForm:
                            p.Type = WebApiParameterType.FormItem;
                            break;
                        case ContentType.Text:
                        case ContentType.Xml:
                        case ContentType.Html:
                        case ContentType.Json:
                        case ContentType.Javascript:
                        case ContentType.Binary:
                            p.Type = WebApiParameterType.Payload;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            d.Parameters = new WebApiParameterDescriptions(parameters);
        }

        static void CheckParametersConflict(WebApiParameterDescriptions parameters)
        {
            var payloadParamsCount = parameters.Values.Count(p => p.Type == WebApiParameterType.Payload);
            if (payloadParamsCount != 0)
            {
                if (payloadParamsCount > 1)
                    throw new WebApiContractException("Should be one paload parameter");

                if (parameters.Values.Count(p => p.Type == WebApiParameterType.FormItem) != 0)
                    throw new WebApiContractException("Shouldn't use paload parameter and one or more form parameters in the same method");
            }
        }

        private static void ProcessSinglePostBinaryCase(MethodInfo method, WebApiMethodDescription d, ref ContentType defaultSubmitContentType)
        {
            if (d.ContentType == ContentType.Undefined && d.HttpMethod != HttpMethod.Get)
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 1 && 
                    (parameters[0].ParameterType == typeof(WebApiFile) || parameters[0].ParameterType == typeof(byte[])))
                {
                    defaultSubmitContentType = ContentType.Binary;
                }
            }
        }

        private static void ProcessSingleStringCase(MethodInfo method, WebApiMethodDescription d, ref ContentType defaultSubmitContentType)
        {
            if (d.ContentType == ContentType.Undefined && d.HttpMethod != HttpMethod.Get)
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                {
                    defaultSubmitContentType = ContentType.Text;
                }
            }
        }

        public static string CreateMethodId(string methodName, IEnumerable<string> parameterNames)
        {
            return methodName + string.Join("_", parameterNames);
        }
    }
}