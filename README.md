# Coronavirus (COVID-19) in the UK - API Service

## Software Development Kit (SDK) for .NET

This is a .NET SDK for the COVID-19 API, as published by Public Health England on Coronavirus (COVID-19) in the UK.

The API supplies the latest data for the COVID-19 outbreak in the United Kingdom.

The endpoint for the data provided using this SDK is:

https://api.coronavirus.data.gov.uk/v1/data

The SDK is also available for [Python](https://github.com/publichealthengland/coronavirus-dashboard-api-python-sdk), [R](https://github.com/publichealthengland/coronavirus-dashboard-api-R-sdk), [JavaScript](https://raw.githubusercontent.com/publichealthengland/coronavirus-dashboard-api-javascript-sdk) and [Elixir](https://github.com/publichealthengland/coronavirus-dashboard-api-elixir-sdk).


### Pagination

Using this SDK will bypass the pagination process. You will always download the entire
dataset unless the `latest_by` argument is defined.



### Installation


.NET Core is required to use this SDK.

To install visit [here](https://dotnet.microsoft.com/download) and download the correct installation for your OS:

### Example

We would like to extract the number of new cases England using the API.

We start off by adding the project via [NuGet]():

```bash
dotnet add package Cov19API
```
In our application or library we can then instantiate the API by providing [filters](https://coronavirus.data.gov.uk/developers-guide#params-filters) and a [structure](https://coronavirus.data.gov.uk/developers-guide#params-structure):
NOTE: The structure key/value object must use one of the valid metrics for the value however the key can match to any property you have on a POCO.
```csharp
public class CovidData
{
    public DateTime MyDate { get; set; }

    public int NewCases { get; set; }
}

var cov19api = new Cov19Api(new UkCovid19Props
{
    FiltersType = new Dictionary<string, string> { { "areaType", "nation" }, { "areaName", "England" } },
    StructureType = new Dictionary<string, string> { { "MyDate", "date" }, { "newCases", "newCasesByPublishDate" } }
});

var covidData = await cov19api.Get<CovidData>();

foreach (var covidData in data.Data)
{
    Console.WriteLine($"Date:{covidData.MyDate} No. of New Cases:{covidData.NewCases}");
}
```

You may also use `cov19api.GetXml()` to return the data into a .NET `XDocument` object. This is exemplified later 
in this document.

To see the timestamp for the last update, run:

```csharp
var dateTimeOffset = await cov19api.LastUpdate();
Console.WriteLine(dateTimeOffset.ToString("O"));
```

```
2020-07-28T15:34:31.000Z
```

To get the latest data by a specific metric, you can supply the `LatestBy` argument to the API:

```csharp
var cov19api = new Cov19Api(new UkCovid19Props
{
    FiltersType = new Dictionary<string, string> { { "areaType", "nation" }, { "areaName", "England" } },
    StructureType = new Dictionary<string, string> { { "MyDate", "date" }, { "newCases", "newCasesByPublishDate" } },
    LatestBy = "newCasesByPublishDate"
});
```

```
Date:08/17/2020 00:00:00 No. of New Cases:634
```

To get an underlying picture of the API you can use the `cov19api.Head()` method which will return all the headers from the underlying API
which will return `IEnumerable<KeyValuePair<string, IEnumerable<string>>>`.

If you prefer OpenAPI, you can use the `cov10api.Options()` which will return a `OpenApiDocument` object that can be inspected. 



-----------

Developed and maintained by [Public Health England](https://www.gov.uk/government/organisations/public-health-england).

Copyright (c) 2020, Public Health England.
