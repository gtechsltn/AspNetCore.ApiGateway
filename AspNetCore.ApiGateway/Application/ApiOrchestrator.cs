﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCore.ApiGateway
{    
    public class ApiInfo
    {
        public string BaseUrl { get; set; }

        public Mediator Mediator { get; set; }
    }

    public class HubInfo
    {
        public string BaseUrl { get; set; }

        public HubMediator Mediator { get; set; }

        public HubConnection Connection { get; set; }

        public string ReceiveKey { get; set; }
    }

    public class EventSourcingInfo
    {
        public string BaseUrl { get; set; }

        public EventSourceMediator Mediator { get; set; }

        public object Connection { get; set; }

        public string RouteKey { get; set; }
    }

    internal class LoadBalancing
    {
        public LoadBalancingType Type { get; set; } = LoadBalancingType.Random;
        public string[] BaseUrls {  get; set; }
        public int LastBaseUrlIndex { get; set; } = -1;
    }

    public enum LoadBalancingType
    {
        Random,
        RoundRobin
    }

    public class ApiOrchestrator : IApiOrchestrator
    {
        Dictionary<string, ApiInfo> apis = new Dictionary<string, ApiInfo>();

        Dictionary<string, HubInfo> hubs = new Dictionary<string, HubInfo>();

        Dictionary<string, EventSourcingInfo> eventSources = new Dictionary<string, EventSourcingInfo>();

        Dictionary<string, LoadBalancing> apiLoadBalancing = new Dictionary<string, LoadBalancing>();

        private static Random _random = new Random();
        private static readonly object _syncLock = new object();

        public Dictionary<string, HubInfo> Hubs => hubs;

        public string GatewayHubUrl { get; set; }
        public bool StartGatewayHub { get; set; }

        public IMediator AddApi(string apiKey, params string[] baseUrls)
        {
            var mediator = new Mediator(this);

            apis.Add(apiKey.ToLower(), new ApiInfo() { BaseUrl = baseUrls.First(), Mediator = mediator });

            apiLoadBalancing.Add(apiKey.ToLower(), new LoadBalancing { BaseUrls = baseUrls });

            return mediator;
        }

        public IMediator AddApi(string apiKey, LoadBalancingType loadBalancingType, params string[] baseUrls)
        {
            var mediator = new Mediator(this);

            apis.Add(apiKey.ToLower(), new ApiInfo() { BaseUrl = baseUrls.First(), Mediator = mediator });

            apiLoadBalancing.Add(apiKey.ToLower(), new LoadBalancing { Type = loadBalancingType,  BaseUrls = baseUrls });

            return mediator;
        }

        public IHubMediator AddHub(string apiKey, Func<HubConnectionBuilder, HubConnection> connectionBuilder, string receiveKey = null)
        {
            var mediator = new HubMediator(this);

            var cb = new HubConnectionBuilder();

            var conn = connectionBuilder(cb);            

            hubs.Add(apiKey.ToLower(), new HubInfo() { Mediator = mediator, Connection = conn, ReceiveKey = receiveKey });

            return mediator;
        }

        public IEventSourceMediator AddEventSource(string apiKey, Func<object> connectionBuilder, string routeKey)
        {
            var mediator = new EventSourceMediator(this);

            var conn = connectionBuilder();

            eventSources.Add(apiKey.ToLower(), new EventSourcingInfo() { Mediator = mediator, Connection = conn, RouteKey = routeKey });

            return mediator;
        }

        public ApiInfo GetApi(string apiKey, bool withLoadBalancing = false)
        {
            var apiInfo = apis[apiKey.ToLower()];

            if (withLoadBalancing)
            {
                //if more than 1 base url is specified
                //get the load balancing base url based on load balancing algorithm specified.

                var loadBalancing = apiLoadBalancing[apiKey.ToLower()];
                
                if (loadBalancing.BaseUrls.Count() > 1)
                {
                    apiInfo.BaseUrl = this.GetLoadBalancedUrl(loadBalancing);
                }
            }            

            return apiInfo;
        }

        public HubInfo GetHub(string apiKey)
        {
            var apiInfo = hubs[apiKey.ToLower()];

            return apiInfo;
        }

        public EventSourcingInfo GetEventSource(string apiKey)
        {
            var apiInfo = eventSources[apiKey.ToLower()];

            return apiInfo;
        }

        public IEnumerable<Orchestration> Orchestration => apis?.Select(x => new Orchestration
        {
            Api = x.Key,
            Routes = x.Value.Mediator.Routes
        }).Union(hubs?.Select(x => new Orchestration
        {
            Api = x.Key,
            Routes = x.Value.Mediator.Routes
        }))
        .Union(eventSources?.Select(x => new Orchestration
        {
            Api = x.Key,
            Routes = x.Value.Mediator.Routes
        }));

        private string GetLoadBalancedUrl(LoadBalancing loadBalancing)
        {
            if (loadBalancing.Type == LoadBalancingType.RoundRobin)
            {
                loadBalancing.LastBaseUrlIndex++;

                if (loadBalancing.LastBaseUrlIndex >= loadBalancing.BaseUrls.Length)
                {
                    loadBalancing.LastBaseUrlIndex = 0;
                }
                return loadBalancing.BaseUrls[loadBalancing.LastBaseUrlIndex];
            }
            else
            {   
                lock(_syncLock)
                {
                    var selected = _random.Next(0, loadBalancing.BaseUrls.Count());
                    return loadBalancing.BaseUrls[selected];
                }                
            }
        }
    }
}
