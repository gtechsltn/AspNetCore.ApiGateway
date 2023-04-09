﻿using System.Threading.Tasks;
using AspNetCore.ApiGateway;
using Microsoft.AspNetCore.Http;

namespace ApiGateway.API
{
    public interface IWeatherService
    {
        HttpClientConfig GetClientConfig();
        Task<object> GetForecasts(ApiInfo apiInfo, HttpRequest request);
    }
}