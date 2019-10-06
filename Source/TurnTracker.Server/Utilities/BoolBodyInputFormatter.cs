﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace TurnTracker.Server.Utilities
{
    public class BoolBodyInputFormatter : InputFormatter
    {
        public BoolBodyInputFormatter()
        {
            SupportedMediaTypes.Add("text/plain");
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            using (var reader = new StreamReader(request.Body))
            {
                var content = await reader.ReadToEndAsync();
                bool.TryParse(content, out var result);
                return await InputFormatterResult.SuccessAsync(result);
            }
        }

        protected override bool CanReadType(Type type) => type == typeof(bool);
    }
}