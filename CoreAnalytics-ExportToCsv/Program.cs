////////////////////////////////////////////////////////////////////////////////
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////////

using System;

namespace Microsoft.Azure.Cdn.Tools
{
    /// <summary>
    /// This is sample code to export core analytics of Cdn endpoints
    /// to a csv file. Please refer to README.txt before running this code
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            //Read connection string of storage account from appconfig
            var connectionString = System.Configuration.ConfigurationManager.AppSettings["StorageConnectionString"];

            //Read path of targe csv file from appconfig
            var path = System.Configuration.ConfigurationManager.AppSettings["OutputCsvFilePath"];

            //Get CloudStorageAccount
            var storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connectionString);

            //Get StorageBlobClient
            var client = storageAccount.CreateCloudBlobClient();

            //Get storage blob container for Cdn core analytics
            const string cdnCoreAnalyticsBlobContainerName = "insights-logs-coreanalytics";
            var container = client.GetContainerReference(cdnCoreAnalyticsBlobContainerName);

            //Create a string builder to write to csv
            var sb = new System.Text.StringBuilder();

            //Headers
            sb.AppendLine(
                "Profile,Endpoint,Hostname,Time,RequestCountTotal,RequestCountHttpStatus2xx,RequestCountHttpStatus3xx,RequestCountHttpStatus4xx,RequestCountHttpStatus5xx,RequestCountHttpStatusOthers,RequestCountHttpStatus200,RequestCountHttpStatus206,RequestCountHttpStatus302,RequestCountHttpStatus304,RequestCountHttpStatus404,RequestCountCacheHit,RequestCountCacheMiss,RequestCountCacheNoCache,RequestCountCacheUncacheable,RequestCountCacheOthers,EgressTotal,EgressHttpStatus2xx,EgressHttpStatus3xx,EgressHttpStatus4xx,EgressHttpStatus5xx,EgressHttpStatusOthers,EgressCacheHit,EgressCacheMiss,EgressCacheNoCache,EgressCacheUncacheable,EgressCacheOthers");

            //Get blobs in a flat listing, rather than listing blobs hierarchically like virtual directories
            foreach (var item in container.ListBlobs(null, true))
            {
                //Blob hierarchy:
                //resourceId =/SUBSCRIPTIONS/{SUBSCRIPTIONID}/RESOURCEGROUPS/{GROUPNAME}/PROVIDERS/MICROSOFT.CDN/PROFILES/{PROFILENAME}/ENDPOINTS/{ENDPOINTNAME}/y={year}/m={month}/d={day}/h={hour}/m={minute}/PT1H.json
                var blob = (Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob)item;

                var split = blob.Name.Split('/');
                var profile = split[8];
                var endpoint = split[10];
                var year = int.Parse(split[11].Split('=')[1]);
                var month = int.Parse(split[12].Split('=')[1]);
                var day = int.Parse(split[13].Split('=')[1]);
                var hour = int.Parse(split[14].Split('=')[1]);
                var minute = int.Parse(split[15].Split('=')[1]);
                var time = new DateTime(year, month, day, hour, minute, 0);

                //Read content from blob, and extract analytics metrics
                //Sample content in blob, each property of "properties" represents a metric
                /*
				{
					"records":
					[
						{
							"time": "2017-04-01T08:00:00",
							"resourceId": "/SUBSCRIPTIONS/XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXX/RESOURCEGROUPS/GROUP1/PROVIDERS/MICROSOFT.CDN/PROFILES/PROFILE1/ENDPOINTS/ENDPOINT1",
							"operationName": "Microsoft.Cdn/profiles/endpoints/contentDelivery",
							"category": "CoreAnalytics",
							"properties": {
								"DomainName": "endpoint1.azureedge.net",
								"RequestCountTotal": 480,
								"RequestCountHttpStatus2xx": 0,
								"RequestCountHttpStatus3xx": 0,
								"RequestCountHttpStatus4xx": 480,
								"RequestCountHttpStatus5xx": 0,
								"RequestCountHttpStatusOthers": 0,
								"RequestCountHttpStatus200": 0,
								"RequestCountHttpStatus206": 0,
								"RequestCountHttpStatus302": 0,
								"RequestCountHttpStatus304": 0,
								"RequestCountHttpStatus404": 0,
								"RequestCountCacheHit": null,
								"RequestCountCacheMiss": null,
								"RequestCountCacheNoCache": null,
								"RequestCountCacheUncacheable": null,
								"RequestCountCacheOthers": null,
								"EgressTotal": 0.0,
								"EgressHttpStatus2xx": null,
								"EgressHttpStatus3xx": null,
								"EgressHttpStatus4xx": null,
								"EgressHttpStatus5xx": null,
								"EgressHttpStatusOthers": null,
								"EgressCacheHit": null,
								"EgressCacheMiss": null,
								"EgressCacheNoCache": null,
								"EgressCacheUncacheable": null,
								"EgressCacheOthers": null
							}
						}
					]
				}
				*/
                using (var memoryStream = new System.IO.MemoryStream())
                {
                    //Download content to in memory stream
                    blob.DownloadToStream(memoryStream);

                    //Get content from blob as text
                    var text = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());

                    //Deserialize content to json
                    var jObject = Newtonsoft.Json.Linq.JObject.Parse(text);

                    //Get properties object
                    var properties = jObject["records"][0]["properties"];

                    //Get hostname
                    var hostname = properties["DomainName"];

                    //Get metrics
                    var requestCountTotal = properties["RequestCountTotal"];
                    var requestCountHttpStatus2Xx = properties["RequestCountHttpStatus2xx"];
                    var requestCountHttpStatus3Xx = properties["RequestCountHttpStatus3xx"];
                    var requestCountHttpStatus4Xx = properties["RequestCountHttpStatus4xx"];
                    var requestCountHttpStatus5Xx = properties["RequestCountHttpStatus5xx"];
                    var requestCountHttpStatusOthers = properties["RequestCountHttpStatusOthers"];
                    var requestCountHttpStatus200 = properties["RequestCountHttpStatus200"];
                    var requestCountHttpStatus206 = properties["RequestCountHttpStatus206"];
                    var requestCountHttpStatus302 = properties["RequestCountHttpStatus302"];
                    var requestCountHttpStatus304 = properties["RequestCountHttpStatus304"];
                    var requestCountHttpStatus404 = properties["RequestCountHttpStatus404"];
                    var requestCountCacheHit = properties["RequestCountCacheHit"];
                    var requestCountCacheMiss = properties["RequestCountCacheMiss"];
                    var requestCountCacheNoCache = properties["RequestCountCacheNoCache"];
                    var requestCountCacheUncacheable = properties["RequestCountCacheUncacheable"];
                    var requestCountCacheOthers = properties["RequestCountCacheOthers"];
                    var egressTotal = properties["EgressTotal"];
                    var egressHttpStatus2Xx = properties["EgressHttpStatus2xx"];
                    var egressHttpStatus3Xx = properties["EgressHttpStatus3xx"];
                    var egressHttpStatus4Xx = properties["EgressHttpStatus4xx"];
                    var egressHttpStatus5Xx = properties["EgressHttpStatus5xx"];
                    var egressHttpStatusOthers = properties["EgressHttpStatusOthers"];
                    var egressCacheHit = properties["EgressCacheHit"];
                    var egressCacheMiss = properties["EgressCacheMiss"];
                    var egressCacheNoCache = properties["EgressCacheNoCache"];
                    var egressCacheUncacheable = properties["EgressCacheUncacheable"];
                    var egressCacheOthers = properties["EgressCacheOthers"];

                    //Append to string builder
                    sb.AppendLine(
                        $"{profile},{endpoint},{hostname},{time},{requestCountTotal},{requestCountHttpStatus2Xx},{requestCountHttpStatus3Xx},{requestCountHttpStatus4Xx},{requestCountHttpStatus5Xx},{requestCountHttpStatusOthers},{requestCountHttpStatus200},{requestCountHttpStatus206},{requestCountHttpStatus302},{requestCountHttpStatus304},{requestCountHttpStatus404},{requestCountCacheHit},{requestCountCacheMiss},{requestCountCacheNoCache},{requestCountCacheUncacheable},{requestCountCacheOthers},{egressTotal},{egressHttpStatus2Xx},{egressHttpStatus3Xx},{egressHttpStatus4Xx},{egressHttpStatus5Xx},{egressHttpStatusOthers},{egressCacheHit},{egressCacheMiss},{egressCacheNoCache},{egressCacheUncacheable},{egressCacheOthers}");
                }
            }

            //Write to csv file
            System.IO.File.WriteAllText(path, sb.ToString());
        }
    }
}
