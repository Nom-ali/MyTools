using System.Collections.Generic;
using Firebase.Analytics;
using GoogleMobileAds.Api;

public static class AdAnalyticsHelper
{
    public static Parameter[] BuildAdParameters(
        string adFormat,
        string adId,
        string adPosition,
        string adPlatform,
        ResponseInfo responseInfo = null,
        AdValue adValue = null,
        Dictionary<string, object> extraParams = null)
    {
        List<Parameter> parameters = new List<Parameter>
        {
            new Parameter("ad_format", SafeString(adFormat)),
            new Parameter("ad_id", SafeString(adId)),
            new Parameter("ad_position", SafeString(adPosition)),
            new Parameter("ad_platform", SafeString(adPlatform)),
            new Parameter("adapter", GetAdapter(responseInfo)),
            new Parameter("ad_source", GetAdSourceName(responseInfo)),
            new Parameter("ad_source_instance", GetAdSourceInstanceName(responseInfo)),
            new Parameter("response_id", GetResponseId(responseInfo))
        };

        if (adValue != null)
        {
            double revenueValue = adValue.Value / 1000000.0;

            parameters.Add(new Parameter("currency_code", SafeString(adValue.CurrencyCode)));
            parameters.Add(new Parameter("value_micros", adValue.Value));
            parameters.Add(new Parameter("value", revenueValue));
            parameters.Add(new Parameter("precision", adValue.Precision.ToString()));
        }

        if (extraParams != null)
        {
            foreach (var item in extraParams)
            {
                AddExtraParameter(parameters, item.Key, item.Value);
            }
        }

        return parameters.ToArray();
    }

    private static void AddExtraParameter(List<Parameter> parameters, string key, object value)
    {
        if (string.IsNullOrEmpty(key) || value == null)
            return;

        if (value is int intValue)
            parameters.Add(new Parameter(key, intValue));
        else if (value is long longValue)
            parameters.Add(new Parameter(key, longValue));
        else if (value is float floatValue)
            parameters.Add(new Parameter(key, floatValue));
        else if (value is double doubleValue)
            parameters.Add(new Parameter(key, doubleValue));
        else if (value is bool boolValue)
            parameters.Add(new Parameter(key, boolValue ? 1 : 0));
        else
            parameters.Add(new Parameter(key, SafeString(value.ToString())));
    }

    private static string GetAdapter(ResponseInfo responseInfo)
    {
        if (responseInfo == null)
            return "unknown";

        string adapter = responseInfo.GetMediationAdapterClassName();
        return SafeString(adapter);
    }

    private static string GetResponseId(ResponseInfo responseInfo)
    {
        if (responseInfo == null)
            return "unknown";

        return SafeString(responseInfo.GetResponseId());
    }

    private static string GetAdSourceName(ResponseInfo responseInfo)
    {
        if (responseInfo == null)
            return "unknown";

        string loadedAdapter = responseInfo.GetMediationAdapterClassName();

        if (string.IsNullOrEmpty(loadedAdapter))
            return "unknown";

        foreach (AdapterResponseInfo adapterInfo in responseInfo.GetAdapterResponses())
        {
            if (adapterInfo.AdapterClassName == loadedAdapter)
            {
                if (!string.IsNullOrEmpty(adapterInfo.AdSourceName))
                    return SafeString(adapterInfo.AdSourceName);

                if (!string.IsNullOrEmpty(adapterInfo.AdSourceInstanceName))
                    return SafeString(adapterInfo.AdSourceInstanceName);

                return SafeString(adapterInfo.AdapterClassName);
            }
        }

        return SafeString(loadedAdapter);
    }

    private static string GetAdSourceInstanceName(ResponseInfo responseInfo)
    {
        if (responseInfo == null)
            return "unknown";

        string loadedAdapter = responseInfo.GetMediationAdapterClassName();

        if (string.IsNullOrEmpty(loadedAdapter))
            return "unknown";

        foreach (AdapterResponseInfo adapterInfo in responseInfo.GetAdapterResponses())
        {
            if (adapterInfo.AdapterClassName == loadedAdapter)
            {
                if (!string.IsNullOrEmpty(adapterInfo.AdSourceInstanceName))
                    return SafeString(adapterInfo.AdSourceInstanceName);

                return "unknown";
            }
        }

        return "unknown";
    }

    private static string SafeString(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "unknown";

        return value.Length > 100 ? value.Substring(0, 100) : value;
    }
}