---
layout: post
title:  "Ingesting massive amounts of Car Sensor data into Azure"
author: "Valery Jacobs, Sander van den Hoven, Eric Maino"
author-link: ""
#author-image: 
date:   2016-12-30
categories: [IoT] [Service Fabric] [Azure Functions]
color: "blue"
#image: 
excerpt: Ingesting massive amounts of Car Sensor data into Azure using Azure Functions, Service Fabric, Event Hub, IOT Hub.
language: English
verticals: Any vertical that produces lots of sensordata and want be able to analyse it in Azure)
---

## Introduction ## 
Begin with an intro statement with the following details:

-   Solution overview
	Ingesting massive amounts of Car Sensor data into Azure

-   Key technologies used
	Ingesting massive amounts of Car Sensor data into Azure using Azure Functions, Service Fabric, Event Hub, IOT Hub.

-   Core Team: Names, roles and Twitter handles

	Microsoft:
	-	Eric Maino
	-	Valery Jacobs
	-	Sander van den Hoven

	Beijer and Sioux	
	-	Daan Gerrits
	-	Matthijs van Bemmelen
	-	Erwin de Jong	

## Customer profile ## (sander)


-   Company name and URL

	Beijer Automotive, <http://www.beijer.com/en/>

-   Company description

	Beijer Automotive is a team of engineers that build solutions for mobility and
	vehicle telemetry. One of their core products is a CAN bus solution that
	uniforms countless variations of CAN bus implementations (brand, model, etc.)
	into a single consistent format. This enables their customers to build flexible
	mobility solutions on top of their platform like fleet management systems,
	taximeters, traffic management and many more. More information can be found at [http://www.beijer.com/en/services-solutions/can-solutions/integration-into-your-product/
	](http://www.beijer.com/en/services-solutions/can-solutions/integration-into-your-product/)

-   Company location

	Beijer Automotive is located in the Netherlands. There address is:

	Ambachtstraat 22-A

	5481 SL Schijndel,
 
	The Netherlands

-   What are their product/service offerings?

	Beijer Automotive are expanding their business by re-collecting and
    extending the telematics their customers are generating and transforming
    this input into a new data service called Vetuda©. Based on this data
    service they will be able to unlock even more scenarios in the field of
    traffic management, road conditions, public safety or whatever opportunity
    this vast dataset might disclose.

	They work with many companies in the Automotive industry and they already
    introduced us to organizations like the Dutch Vehicle Authority (RDW) and
	*Highways England* whom are also launching customers for the **Vetuda©**
    platform.

	Beijer Automotive has many years of experience in a field where we as
    Microsoft would like to learn more about. Their feedback on our approach to
    IoT and our IoT services is expected to be very valuable.

	One of goals is to change the business model for their customers where they
    are being offered the opportunity to monetize the data they contribute to
    the platform. This incentive will accelerate adoption and improve the data
    coverage needed to enable even more innovations.

	Each connecting customer could benefit from having their **Vetuda©**
    connector running in Azure as they currently need to manage their own
    infrastructure to take part in the data collection for the platform limiting
    the possibilities on both ends.

	They will be extending their business to the agricultural vertical which has
    a big worldwide potential for Microsoft as well as this could boost the
    scale level significantly.

.

# Problem statement #

Handling large amounts of data is challenging in any architecture but in this
scenario specifically the frequency with which data needs to be recorded and the
volume that the devices (vehicles) are deployed in poses extra challenges that
need to be recognized and tackled.

The potential value of the data depends on many factors like data quality,
frequency, validity and depth. This means that the specifications for this IoT
solution must be finely calibrated to keep costs at a feasible level and offer
the performance that is needed for outputting the end result of the system.

An additional challenge was that Beijer doesn’t exactly know yet the treasures
the data might contain and how the customers, who pay for the data, will be
using it as they probably also have to validate its value. This called for an
exploratory mind-set that states that initially the amount of data and the
number of measurements in that data should be as extensive as possible. This
approach offers the opportunity to take a broad overview of what information the
data could unlock and then narrow down to the cases that have these
characteristics:

-   The data has a business value without intense processing (f.e. GPS
    positioning offering direct insights on traffic congestion by simply
    plotting it on a map)

-   The data interchange frequency is at the right level to have an adequate
    level of accuracy but should also not be too high as this has direct
    consequences on costs and performance (f.e. the frequency with which vehicle
    speed is sensed determines the scenario’s that this data can be used for).

-   The payload is optimized for the use in a high-throughput architecture.
    Carefully choosing the payload format is an important step in the process of
    minimizing the byte streams that need to pass the system.

We anticipated the amount of data would be quite high based on the indicative
requirements we received early in the engagement. This article will outline in
detail what numbers we were trying to hit and how we calculated various other
metrics and performance targets from that.

Customer that act as data providers for Vetuda currently don’t receive an SDK
from Beijer but they receive specifications describing to how the integration
needs to be implemented. This means that each customer has to build their own
connector to connect to Vetuda, or more precise be connected by Vetuda. Changing
the underlying mechanism means impacting all these implementations incurring
costs on a per-customer basis.

In the existing implementation data providers run HTTP based interfaces that are
called from the Vetuda back-end to collect data. These requests vary in
properties (last hour update, full day data dump, subset of metrics etc.). The
benefit of this is that Vetuda ingests data on its own terms and providers act
as a buffer for the incoming data streams the vehicles submit. A downside is
that extra logic is needed to call out to the interfaces and to keep track of
data completeness.

The goal of the hack fest was to further investigate the topics mentioned above
and to validate the assumption we made in advance that Azure can offer the
performance needed in handling the large amount of data at a rate that is
required for the more complex and data intensive scenario’s.

The core points that were part of this investigation were:

-   Can Azure offer the bandwidth and processing power to ingest the data
    streams?

-   How does the architecture need to be setup in order to meet the requirements
    for handling 50.000 vehicles recording data on 1 per second interval?

-   What are the costs involved in implementing the architecture using Azure
    PaaS services and how can these costs be minimized?

-   How can the current architecture be optimized for the cloud to improve
    performance and scalability?

-   Which alternatives are there in the implementation and what are their pro’s
    and cons?

To get to an acceptable of details the hackfest was scoped to focus on data
ingest. Further processing and analyzing the data is considered a project by
itself with many options to consider and include in a proof of concept.

## Solution and steps (Sander) ##
The problem for Beijer as described above is to come to a technical and economical feasible solution to ingest large amounts of data from cars into Azure in order to process and anlyse the data into useful information and alerts. 

###Setting the scene in numbers (Sander) ###
In order to set the scene on how much data will be ingest and processed by the solution this paragraph will define the number. In the Figure 0, the numbers that are set by Beijer are given. These are the number that the solution needs to process once the solution is fully deployed. However it is still required that the solution needs to scale to larger number in the future.

![Figure 0: Constants](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/Constants.PNG)

The data need to be gathered from 50000 cars for every second. We do the test only for a number signal/messages in order to test the signal with many small messages and less larger messages. We start with 1 signal value per message.
Because not all cars will drive every moment the number of cars that is driving and producing data is set to 15% (occupation) on average and 33% in peak moments. We assume that about 6 hours per day the peak level occurs. 
We also define the number of signal values that need to be retrieved per second. This will be 1 as we want at least know the speed of a car every second. This might however be decreased of this lead to oversampling or a lower sampling rate does not decrease the quality of the alerts.

With the input as described above this will lead to a number of messages and bytes that need to get ingested by the solution as described in Figure 1.

![Figure 1: Numbers](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/Numbers.PNG)

Per month this will lead to considerable load and storage (estimated 14TB). In this document we will describe what solutions were tested, if they succeed and what the expected cost is. 


###Overall Solution (Sander)### 

To solve the problem to get ingest the request number of messages and bytes we have tested two different scenarios:

1. Using a **Pull Scenario** where the solution is capable to pull data from the Vibex endpoint at all data suppliers (car fleet owners) and move that data into Azure via Event / IOT Hub
2. Using a **Push Scenario** where software running at the data supplier that pushed data to the Event / IOT Hub in Azure.

**The Pull Scenario**

The pull scenario is a scenario where every customer of Beijer has it own data storage where the data of their cars is stored. The timing to pull car data is depended on the customer scenario, but each car will produce car sensor data to the customer storage on on average every 2.5 seconds.

The data in the data storage of each customer need to be moved to Azure. This is done via an endpoint service that is called Vibex. Vibex is a RESTFull interface that is developed by Beijer and is able to return data from cars in JSON format. Per request the cars, the period and the signals can be defined.

An example Vibex REST that get values for a certain signal 191 over a time frame is as follows:

    https://[IMPLEMENTER]/VIBeX/x.y/Vehicles/VehicleID/191?StartDateTime=2016-12-18T13:00:12+2:00&StopDateTime=2016-12-18T13:00:14+2:00

This call will result in a JSON payload that return the values for signal 191:

    {
    	“Result” :
    	[
    		{
    			“SignalId” : 2,
    			“Time” : “2014-04-18T13:00.123:12+2:00”
    			“Location” : “51.4333, 5.4833”, “Value” : true
    		},
    		{
    			“SignalId” : 191,
    			“Time” : “2014-04-18T13:00.544:12+2:00”
    			“Location” : “51.4333, 5.4833”, “Value” : 31.55
    		},
       ]
    }
    

The VIBEX interface is described in detail in the VIBEX manual:
[https://github.com/svandenhoven/IoTArchitecture/blob/master/VIBEX-002%20-%20API%20Design%20v1.3.pdf](https://github.com/svandenhoven/IoTArchitecture/blob/master/VIBEX-002%20-%20API%20Design%20v1.3.pdf "ViBeX 1.3 Manual")

Each customer will have a Vibex Endpoint which can be called to retrieve the data from cars. Once the data is retrieved it need to be stored and analyzed in Azure. The architecture is shown in the overall architecture that is shown in figure 2.

![Figure 2: Overall Architecture Pull Solution](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/OverallPullArchitecture.png)

Every car / vehicle has a device to capture the car data via the CanBus. This data is sent to each customer (fleet owner). They store in they own data store and perform their local business processes on it. The data is exposed via a secure ViBeX REST API. In the Beijer Solution in Azure a solution runs that pulls this data into Azure.

In order to be able to pull all data from all customers it is required that a solution is build that can handle the required throughput. In order to this the solution will have the following components as described in figure 3.

![Figure 3: Pull Solution Components](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/PullComponents.png)


- Job Scheduler: 

	In order to be able to schedule the jobs to pull data from a certain ViBeX EndPoint a JobScheduler is created. This JobSchedular defines the Job that needs to be Executed. The Jobs will be added to a JobQueue that is accesible for the JobWorker.
	
- Job Worker

	The JobWorker retrieves the job from the JobQueue and get the action that need to done. In the Job the EndPoint to the ViBeX is mentioned and the JobWorker call the EndPoint, retrieves the result data and sent the data to the Data Retreiver.

- Data Retriever

	The Data Retriever is capable to retrieve large quantities of data and performs two actions:

	1.  Store the data in persistent storage for future usage
	2.  Sent the data to the Data Processor for real-time processing

- Data Processor

	The Data processor retrieves the data and analysis the data to detect alerts. This can be for instance a defined number of fog lights in a certain region, this will raise the fog alert. The alert are sent to Beijer Client that are subscribed to the alerts.



**The Push Scenario**
The Push Scenario is much simpler than the push scenario as is also tested in this HackFest. It will require a change in architecture, but the promises of a simpler architecture as such large that Beijer architects has taken it into account as an alternative for the current ViBeX API. The architecture for the Push Scenario is show in Figure 4.

![Figure 4: Push Solution Components](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/PushComponents.png)

The customer has instead of the an deployment of the ViBeX Pull Api an alternative with an deployment of a ViBex Push Service. This ViBeX Push Service will directly sent car data to the Data Retriever (same as described in Pull Scenario). This Data Retriever will service has hub for the data and consequently sent it to storage and the Data Processor. The Data Processor will create alerts that will be sent to Beijer's Clients

This scenario has the the following advantages:

1. No data collector and processor is required in Azure. This reduces compexity and cost

2. The solution scales with the number of customers. When more customers are connected, the solution scales itself as then processing is done at the customer side

3. The system only sents data when car actually produce data. In the pulling scenario the system needs to request for data even if there is none, just to know there is no data. With the push scenario, the system only sents data when car are driving.

### Adding IoT Hub for real-time data and cloud to device communication

Although the Vetuda system focusses on the ingestion of large amounts of data it does make sense to categorize these data streams. Data can be handled as it comes into the system to result in near-real-time alerts (hot path) while at the same time it can be analyzed later on together with data accumulated over time (cold path).

Vetuda also has plans to extend their services with new types of vehicles that might not even involve the intermediate data provider role but consists of devices (vehicles) connecting to the cloud back-end directly.

For these requirements to be satisfied we added one of the core Azure IoT Services into the mix, IoT Hub. IoT Hub makes sense for those scenario’s where a direct connection between an end node device or a gateway and the cloud is involved. It offers high-volume data ingest and a registry for potentially millions of devices that can maintain durable communication channels supporting data transports in both directions. It communicates over multiple protocols like HTTP(s) (mostly for backwards compatibility), MQTT (as it is the most widely used protocol in M2M and IoT currently) and AMQP (a new, Microsoft backed, member in the IoT family offering updated specs compared to MQTT on security and efficiency).

When a fleet of devices is directly connected to the Vetuda back-end it also places the responsibility of device management in that domain. Luckily IoT Hub device management has multiple features that accelerate implementing this responsbility into the solution. Microsoft's approach here is to provide APIs on top of the IoT Hub services to either integrate with or build a custom asset management solution. As devices go through a life-cycle of provisioning, configuring, updating and eventually decommisioning there are quite a few things that need to be implemented or integrated before that cycle is complete. For the hackfest the focus was on the feature set that directly could be applied to the Vetuda case and the first step to leveraging the benefits of this service.

![ScreenShot 4.0: IoT Hub Architectural positioning](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/BeijerIoTHubArch.png)

Under the hood IoT Hub is based on Azure Event Hubs and on the consumer side (reading the data from the buffer) the developer experience is identical. The main differences are on service and device registry end of IoT Hub where there are several endpoints for specific tasks like managing devices and their state.

For the hackfest we looked at the specific application for IoT Hub and noted the fact that for Vetuda device identity actually was an issue in that data should never be tracable to its source, which sounds like the opposite of that IoT Hub tries to offer. On the other hand, one of the more advanced and differentiating scenarios is where Vetuda would be able to query and communicate back to vehicles in a way that the the system would target vehicles in a specific geographical area (geo-fenced). The approach we chose was to use device twins and let vehicles update their GPS data in device twin properties.

![ScreenShot 4.1: IoT Hub Device Twins](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/BeijerIoTDeviceTwins.png)

The Device Twin properties could look like the following:
'''
	@"{
             tags: {
                 location: {
                     region: 'EU',
                     lon: '52.3054724',
                     lat: '4.7533562'
                 },
                 allocation:{fleet: 'AgriAutomotive_F12',
                     fleetId: 'F12C345'},
			     state:{wipers:1,
				 		foglight:1,
						 warningLight:0}
             }
         }"
'''

The vehicle client would update the state using:
'''
await registryManager.UpdateTwinAsync(twin.DeviceId, patch, twin.ETag);
'''
And the back-end can query the state of all vehicles in a registry using this syntax:

	"SELECT * FROM devices WHERE tags.location.lon < 53 AND tags.location.lat < 5

	query = registryManager.CreateQuery("SELECT * FROM devices WHERE tags.location.lon < 53 AND tags.location.lat < 5", 100);
	var twinsInNearFogZone = await query.GetNextAsTwinAsync();
	Console.WriteLine("Vehicles in fog zone: {0}", string.Join(", ", twinsInNearFogZone.Select(t => t.DeviceId)));


Using this approach a back-end service can query state without the need for vehicles being online. In general it is a good practice to be independant on both connectivity and bandwidth for back-end querying as 3rd party services might depend on swift query results coming through. 

The query detects vehicles in a geo-fenced region and based on the result a couple of things could be determined:
1. The vehicle is in a specific region
2. In this region x vehicles have fog light on
3. An alert should be send to vehicles in this region

To confirm with the requirement that vehicle data should not be tracable to its owner or driver all de registry data could be stored anonymously where IoT Hub know how to query or send a message to a specific vehicle but there is no meta-data available to trace any other entity related to that vehicle. GPS locations are tricky in this regard as one could trace a work or home address and still find out who was driving. A solution to this problem could be to rasterize the GPS data to a lower resolution (offering less accuracy in querying vehicle positioning) or only storing region identifiers that have no queriable GPS properties.

![ScreenShot 4.2: Applying a geographical raster for device querying](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/BeijerIoTHubRaster.png)

There are multiple options for sending messages back to vehicles using IoT Hub:

Direct methods
	Direct response
	Applicable for lower latency messages like: turn on/off 
	8Kb max.
	1500/minute/unit max.

Device Twin desired properties
	State update, 
	Applicable for higher latency messages like config changes
	8Kb max.

C2D messages
	one-way
	Larger sized messages, up to 256Kb
	Lower frequency
	5000/minute/unit max.

A new feature, recently added to IoT Hub, offers the opportunity to filter incoming messages and reroute them to external endpoint as they enter the IoT Hub ingest endpoint. This can be interesting for setting up an alerting scenario where certain message values (properties) can be routed to an andpoint that translated incoming messages into notifications to mobile phones or trigger that communicate back to vehicles or other services using f.e. a web hook.

![ScreenShot 4.3: Routing messages from within IoT Hub](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/BeijerIoTHubRoutes.png)

The way we chose to implement this was to use Azure Service Bus as the external endpoint and filter the warning light state so that engaged warning lights could result in a very quick response on the backend. Azure Service Bus additionally offers topics on top of queues where multiple parties, or system, could subscribe to event coming from the filtered stream of events.


Using the Service Bus Explorer tool we simulated this scenario. Device state updates would come in and the IoT Hub routing filter would pass them through to the service bus. It is possible to define multiple endpoint and filters. IoT Hub manages all the state for filtering and splitting message routes.


Technical delivery
------------------
####The Job Scheduler for the Pull Scenario ####

For the Pull scenario it is required that at dedicated moments the data is pulled form Beijers customer ViBeX endpoint. To schedule and scale the pulling of data a scalable solution is designed. This design is based on a application that generates Jobs and added these Jobs into a Azure Queue. The Job Worker can retrieve the jobs from the Azure Queue and performs the Jobs.

The code can be found at LINK. The JobSchedular is for the HackFest just a Console App that add an certain message to a Azure Queue in a certain time interval. The message is according the following class:

    [Serializable]
    public class CollectorJob
    {
        public Uri EndPoint;
        public string Query;
    }

This class is serialized to JSON and sent to the Azure Queue as a JSON Payload. The JSON Payload looks as follows:

	{
		"EndPoint":"http://vetudavibexdemo.azurewebsites.net",
		"Query":"api/IOTObject?customer=1&nrCars=165&nrSignalValues=1"
	}

The code that sent the message to the queue is:

    static void Main(string[] args)
     {
        var cString = ConfigurationManager.AppSettings["StorageConnectionString"];
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(cString);
        CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
        CloudQueue queue = queueClient.GetQueueReference("ventudacollectorjobs");
        queue.CreateIfNotExists();

        Random rand = new Random();
        var j = 1;
        var nrJobs = 100;
        var nrSignalValues = 16500;
        var nrCarPerCompany = nrSignalValues / nrJobs;
        var pullingTimeSpan = 1000*10; //in milliseconds

        var run = 1;
        while (run <= 100000)
        {
            run++;
            var now = DateTime.Now;
            for (int c = 1; c <= nrJobs; c++)
            {
                var job = new CollectorJob { EndPoint = new Uri("http://vetudavibexdemo.azurewebsites.net"), Query = $"api/IOTObject?customer={c.ToString()}&nrCars={nrCarPerCompany}&nrSignalValues=1" };
                var json = JsonConvert.SerializeObject(job);
                CloudQueueMessage message = new CloudQueueMessage(json);
                queue.AddMessage(message);
                Console.WriteLine($"Created Job {j} to {job.EndPoint}/{job.Query}");
                j++;
            }
            var done = DateTime.Now;
            var duration = (done - now).TotalMilliseconds;
            if (duration < pullingTimeSpan)
                Thread.Sleep(Convert.ToInt32(pullingTimeSpan - Convert.ToInt32(duration)));
        }
    }

In a loop (max 10000 runs) there are nrJobs created. Each Job represents a call to a Beijer Customer that we have set to 100. In the code this is just one endpoint that is very scalable configured to very fast deliver data from memory and is therefor capable to deliver very high load. For this test this is valid as we do not want to test the retrieval of data, but the ingest into Azure.

We want to retrieve 1 signal for 16500 (the number of cars drive on a peak moment) every  second. However because the system is not able to add a message to queue, picks it up at the Job Worker, Get Data and Put it into Data Retriever we set the pullingTimeSpan to 10 seconds. Every call will then retrieve data for 10 seconds.

####Pull Scenario using Azure Function ####
The Pull Scenario using Azure Functions implements the Job Worker function from the architecture. In this architecture we have used dynamic azure functions that do not have their own hosting plan but share a hosting plan and the owner pays per executing and used memory (see [https://azure.microsoft.com/en-us/pricing/details/functions/](https://azure.microsoft.com/en-us/pricing/details/functions/ "Azure Functions Pricing") for more details).

We have used a Azure Function that is triggered when a message is added to the Azure Queue. The Azure Functions has therefor one input (the Azure Queue) and one Output (Azure Blob Storage). The output to Azure EventHub is done in code as we need to processing on the incoming data to disassemble on response with data of multiple cars into multiple messages to EventHub. This can be seen by the function.json that is used for the Azure Function.

	{
	  "bindings": 
		[
			{
			  "name": "myQueueItem",
			  "type": "queueTrigger",
			  "direction": "in",
			  "queueName": "vetudacollectorjobs",
			  "connection": "vetudajobs_STORAGE"
			},
			{
			  "type": "blob",
			  "name": "outputBlob",
			  "path": "outcontainer/{rand-guid}",
			  "connection": "vetudajobs_STORAGE",
			  "direction": "out"
			}
		  ],
		  "disabled": false
	} 

The Azure Function is automatically triggered when an item is added to the queue "vetudacollectorjobs". In order to run all the code we need the following namespaces were used: 

	#r "Newtonsoft.Json"
	#r "Microsoft.ServiceBus"
	
	using System;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using System.Net;
	using Microsoft.ServiceBus.Messaging; 
	using System.Text;
	using System.Configuration;

We had to reference Newtonsoft.Json and Microsoft.ServiceBus as these are not natively available in the Azure Function code.

The following code is executed when an item is added to the Azure Queue "vetudacollectorjobs":
	
	public static void Run(string myQueueItem, TraceWriter log, out string outputBlob)
	{
	
		//Step 1: Config
	    var eventHubName = "functions";
	    var maxBatchSize = 200000; //Max Bytes of a message
	    long totalCurrentSize = 0;
	    var connectionString = ConfigurationManager.AppSettings["vetudaEventHubConnectionString"].ToString();
	    var eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, eventHubName);
	    var batch = new List<EventData>();

		//Step 2: Get Queue Item
	    var json = myQueueItem;
	    var job = JsonConvert.DeserializeObject<CollectorJob>(json); 
	
		//Step 3: Run the Job to get Data
	    var result = Task.Run(() => PerformJob(job)).Result; 

		//Step 4: Output the Job to Blob Storage
	    outputBlob = result;

		//Step 5: Create batches and sent to EventHub
	    var signalList  = JsonConvert.DeserializeObject<Measurements>(result); 
	    foreach(SignalValue obj in signalList.SignalValues)
	    {
	        var serializedIOTObject = JsonConvert.SerializeObject(obj);
	        var x = new EventData(Encoding.UTF8.GetBytes(serializedIOTObject));
	        totalCurrentSize += x.SerializedSizeInBytes;
	        if (totalCurrentSize > maxBatchSize)
	        {
	            SendBatchToEventHub(batch, eventHubClient);
	            totalCurrentSize = 0;
	            batch.Clear();
	        }
	       
	        batch.Add(x);
	    }
	    SendBatchToEventHub(batch, eventHubClient);
	}

The code goes through five steps:

1. Step 1: Config
	
	The configuration is done to set the constants and create the eventhubclient and an empty list of the class EventData. Also the maxBatchSize is set to 200000. This is used as the max size of the batch that is sent to the Eventhub. This is set to 256 KB, but to error by possible overhead we have set it a little lower.

2. Step 2: Get Queue Item

	Next step is to get the message from the queue end desiralize it into a class. The class that is used is CollectorJob that looks as follows.

		public class CollectorJob
		{
		    public Uri EndPoint;
		    public string Query;
		}

3. Step 3: Run the Job to get Data

	As the Job is know it can be executed. Because the Azure Function is not asynchronous the action needs to run as a anonymous function in Task.Run. The job is run by the function PerformJob which looks as follows:

		public async static Task<string> PerformJob(CollectorJob job) 
		{
		    try
		    {
		        string uri = $"{job.EndPoint}{job.Query}";
		        var client = new HttpClient();
		        client.BaseAddress = job.EndPoint;
		        var response = await client.GetAsync(job.Query);
		        var json = await response.Content.ReadAsStringAsync();
		        return json;
		    }
		    catch(Exception ex)
		    {
		        return null;
		    }
		}

	The uri is determined as the endpoint for the call. This RESTAPI call is done and JSON is returned.

4. Step 4: Output the Job to Blob Storage

	The JSON is sent to blobstore. In Azure Functions this is very simple by assigning a string to the variable. Each document gets automatically a unique name as a GUID. This way the raw data is stored and can be used for future processing.

5. Step 5: Create batches and sent to EventHub

	In Step 5, the first action is to deserialize the JSON into a object called Measurements. This object is a List of SignalValues and is defined as follows:

		public class SignalValue
		{
		    public string Fleet { get; set; }
		    public string Vin { get; set; }
		    public int SignalId { get; set; }
		    public DateTime Time { get; set; }
		    public string Location { get; set; }
		    public string Value { get; set; }
		}
		
		public class Measurements
		{
		    public Measurements()
		    {
		        this.SignalValues = new List<SignalValue>();
		    }
		    public List<SignalValue> SignalValues { get; set; }
		}

	The SignalValue is the value that actually contains the data of a Car. The SignalId defines what type of signal it is (one of 300 ranging from speed, to gear that is used, to fuel level, use of lights etc etc). A JSON Payload can contain data from multiple cars and mutiple times. So it might contain data one one moment for 50 cars but also data of the 50 cars for the last 5 minutes.

	Once a list is creates of SignalValues we want that each SignalValue is sent to the EventHub. This makes the processing in the EventHub, through for instance StreamAnalytics easier as it does not have to proces bulkdata and can do processing on individual signalvalues.
	To make the ingest into Azure EventHub more performant we use Batches of Events. Because the size of a batch has a maximuim we traverses trough all the signalvalues and add each SignalValue to the Batch untill we reach the max size. The batch then sent to the Azure EventHub.

Azure Functions run on the Azure Function Host build on Azure Web Jobs and they take care of their own scalability. This means that multiple Azure Function are executed in parallel based on the messages in the Azure Queue. The scalability is responsbility of Azure Function Hosting environment and outside the control of the HackFest.

####Pull Scenario using Azure Web Apps/Jobs####
The Azure WebJobs is very similar to the Azure Functions. The code is almost the same, expect for the coding to sent files to the Blob Store. Each WebJob as a continous WebJob and and have an infinite loop to check if a message exists in the Azure Queue. If this exist it will perform the job in exact the same manner as the above described azure function will. 

Because or the similarities we have not created a solution during the HackFest. We have however created a design and defined an assumption on the required infrastructure in order to make a estimate on cost.
 

####Pull Scenario using Azure Service Fabric####
The Azure Service Fabric is very similar to the Azure Functions, except a stateless server is created that in a loop reads job from the queue and processes the job. The stateless server is defined as follows:

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            QueueReader queue = new QueueReader(log, performer);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (!await queue.ReadItem(cancellationToken))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    log.LogWarning("Exception", ex.ToString());
                }
            }
        }

The performer is a JobPerformer that is defined in the constructor to the service:

        public CollectorWorker(StatelessServiceContext context)
            : base(context)
        {
            rand = new Random();
            log = new ServiceLogger(this);
            performer = new JobPerformer(log);
        }

In the queue.ReadItem a job from the qeueu is read (is there is one) and the associated Job is executed.

        public async Task<bool> ReadItem(CancellationToken cancellationToken)
        {
            await _queue.CreateIfNotExistsAsync();

            // Get the next message
            CloudQueueMessage retrievedMessage = await _queue.GetMessageAsync(cancellationToken);
            if (retrievedMessage != null)
            {
                try
                {
                    _watch.Start();
                    var json = retrievedMessage.AsString;
                    var job = JsonConvert.DeserializeObject<CollectorJob>(json);
                    _log.LogMessage($"Starting Job {job.EndPoint}{job.Query}");
                    var success = await _performer.PerformJob(job, cancellationToken);

                    if (success)
                    {
                        await _queue.DeleteMessageAsync(retrievedMessage, cancellationToken);
                    }
                }
                finally
                {
                    _watch.Stop();
                    _log.LogInfo("ExecutionTime", _watch.AverageExecutionInSeconds.ToString());
                }
            }

            return retrievedMessage != null;
        }

The Job is similar as in the Azure Function scenario where the data is retrieved and batches of 200kb are sent to the EventHub.

####Push Scenario####

The push scenario as described above has a component that sends signal to the Event/IOT Hub from the Vibex Server. This will require that the ViBex Server that run at the Beijer Customer need an additional component that can sent car signal if they are present. The car data will be locally store in the data store of the customer. This can be any data store ranging from sql database to flatfiles. In the hackfest we assume that we retrieve data from a database. The test implementation is done via a console app, but this can be refactored to a daemon of a Windows Service to run as a background process.

The code of the application is very similar to the code used in the Azure Function as it does the same functionality.

The class of data that is sent to the eventhub is the following:
        public class SignalValue
        {
            public string Vin { get; set; }
            public int SignalId { get; set; }
            public DateTime Time { get; set; }
            public string Location { get; set; }
            public string Value { get; set; }
        }

That is a class EventHubAgent that check that status of the database of the BeijerCustomer and in case there is new data it will sent it to the eventhub. 

        public EventHubAgent(IDatabaseAccessFacade databaseAccessFacade, string eventhubName, string connectionString )
        {
            databaseAccessFacade.DatabaseActionOcurred += DatabaseAccessFacade_DatabaseActionOcurred;
            mEventhubName = eventhubName;
            mConnectionString = connectionString;
        }

The databaseAccessFacade is depending on the implemention of the database at the customer and can range from flatfile to sqlserver.

The code that sent the message to the queue is the following. The method SendBatchToEventHub is same as the one in Azure Functions. If there is new data and the following event will be triggered:

        private void DatabaseAccessFacade_DatabaseActionOcurred(object sender, DatabaseActionEventArgs e)
        {
			//Step 1
            Console.WriteLine($"database action occurred. Sending {e.Values.Count} to eventHub");
            Stopwatch s = new Stopwatch();
            s.Start();
            var values = e.Values.Select(v => new SignalValue { Vin = v.Vin, Location = v.Location, SignalId = v.SignalId, Time = v.Time, Value = v.Value }).ToList();
            var eventHubClient = EventHubClient.CreateFromConnectionString(mConnectionString, mEventhubName);

            var batch = new List<EventData>();
            var maxBatchSize = 200000; //max size of eventhub batch

			//Step 2: Create Batch
            long totalCurrentSize = 0;
            int i = 0;
            foreach (var value in values)
            {
                i++;
                var serializedSignalValue = JsonConvert.SerializeObject(value);
                var x = new EventData(Encoding.UTF8.GetBytes(serializedSignalValue));
                totalCurrentSize += x.SerializedSizeInBytes;
                if (totalCurrentSize > maxBatchSize)
                {
                    Console.WriteLine($"Sending batch to eventHub. batch size: {i}");
                    SendBatchToEventHub(batch, eventHubClient);
                    totalCurrentSize = 0;
                    batch.Clear();
                    i = 0;
                }

                batch.Add(x);
            }

			//Step 3: Finalize
            Console.WriteLine($"Sending batch to eventHub. batch size: {i}");
            SendBatchToEventHub(batch, eventHubClient);
            Console.WriteLine($"done sending batches, time spent sending: {s.ElapsedMilliseconds} milliseconds");
        }

        public static void SendBatchToEventHub(List<EventData> batch, EventHubClient eventHubClient)
        {
            eventHubClient.SendBatch(batch);
        }

This solutuion will get the car data via DatabaseActionEventArgs and created a values List in Step 1. In Step 2 the data is batched into a eventhub batch of max site 200kb and sent to the event hub in batches.

Technical Execution
-------------------

When we had created the code for all the scenario's we have executed the code and the most important finding can be seen in the cost section. We had created a powerbi dashboard on top of the Stream Analistics to see how many signal were sent to the Event Hub. In all scenario's were were able to get to 3 around 3 million events. See screenshot 1 from our PowerBI.

![ScreenShot 1: 3 Million Signals in 5 minutes](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/Screenshot1.png)

In the execution the eventhub was hit quite hard and we had to do some configuration on the number of Throughput Units for the EventHub to accomodate the throughput. This resulted in high througput in our eventhub as can be see in screenshot 2.

![ScreenShot 2: Busy Eventhub](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/EventHubMetrics.png)


Cost
----------


This section describes the cost of the different scenarios. The cost will be defined for the usage of Azure resources including:

- Virtual Machines

	Virtual Machines on Azure are payed on hourly usage) and the price differs per type of VM that is created. 

- Bandwidth

	Bandwith is for normal use only payed for outbound (egress) network traffic. This is for this case out of scope as we only ingress data, so no costs are calculated for bandwidth. Of course the customers of Beijer will need to sent the data to Azure. The cost of this bandwidth is set our of scope for this document.
	
- Storage

	In Azure storage is calculated for storing, writing, retrieval and operation. In this document we only define the cost for the data that is gathered. The cost we define is to store and write to the storage. Cost for retrieval and operation are out of scope. The cost for storage are €8,64/TB for storage, €2.16/TB for writing.

- Azure Functions 

	Azure Functions cost is calculated on executions and on used memory. The price is determined Execution Time and use of Memory plus the executions. THe prise is €0.000013/GB-s and €0.1687 per Million Executions, where the first 400,000 GB/s and 1,000,000 executions are free.
	The advantage of Azure Functions is that if the solution is not used there are no cost and when the system is gradually used more, the cost also gradually rise.

- Usage EventHub

	The Azure Event Hub is priced on the number of events that are processed (called ingres) and the use of throughput units. Each throughput unit is capable to handle 1 MB/sec of events, with max 1000 ingress events. We assume that we need the max of 20 thoughput units (10 for input and 10 for output to for instance stream analytics). The cost are €0.024 per million events and €9.41 euro per throughput unit for a full month (744 hours).



###Cost of Pull Scenario using Azure Functions ###

In order to define the cost for Azure Function we have done a test with the software described in section "Pull Scenario using Azure Function". We haven taken some constants to do the cost calculation as can be seen in Figure 5. More detail on the cost can be found in the Azure Pricing Calculator on https://azure.microsoft.com/en-us/pricing/calculator. We assume that the azure functions need the max memory of 1536 MB.

![Figure 5: Azure Function Durations](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/AzureFunctionConstants.PNG)

We have done mutiple run to find the average throughput. We have done tests with 1000, 2500, 5000,7500 and 50000 signals in one job. For the system it does not matter if this are signals from 1 car, from mutiple cars or a combination. Figure 6 shows the average execution time.

![Figure 6: Azure Function Durations](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/AzureFunctionDuration.PNG)

In Figure 7 a graphical representation of this table is given:

![Figure 7: Azure Function Duration Graph](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/AzureFunctionDurationGrap.PNG)


We have done an extrapolation to the number of items that can be done in 1 second (which is on average 320).
To get the an optimum for the retrieval of data we need to consider that  the time it takes to retrieve and processed the data should not be longer that the retrieval interval. For instance if we want data for every second,but we retrieve every 5 seconds to get the data from last 5 seconds, the processing must be lower than 5 seconds to avoid congestion. This has impact on the Sampling Rate. THe higher the sampling rate the more signalvalues need to be retrieved the more retrieval processes need to run in parallel.

In Figure 8 gives the time it takes to ingest 16500 signal values with a 320 signal/sec depending on the number of vibex servers are called. This is in fact executing parallel requests.

![Figure 8: Time depending on number of Vibex Servers](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/RequiredVibexServers.PNG)

In the following tables an overview is given of the cost of Azure Function based on the sampling rate. Table 1 shows the cost optimized on duration of each job, Table 2 shows the cost optimized of the number of executions.


![Table 1: Azure Function Cost optimized on Duration](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/AzureFunctionCostOptimizedonDuration.PNG)
Table 1

In Table 1 can be seen that if it required that from every car in peak time (33% of 55000 = 16500) every second one signal is required, this mean that there are 52 parallel processes need to start that each will retrieve avg 320 signals per second (this is max as we saw in our tests. This will result with memory consumption of 1536 MB in a monthly cost of €2641,-. 
This is not a feasible option as:

- Azure Function are not able to run 53 parallel processes

- The timespan of 1 sec is very short and leaves not room for latency to actually retreive the data from a remote location 

- The the costs are relativey high.

A more feasible solution is too lower the sampling rate to once per 10 seconds. In the calcuation it can we seen that the cost lower to €264 and the number of paralle processes is feasible.

If the optimization is not done on duration but of number of execution this will be the estimate:

![Table 2: Azure Function Cost optimized on number executions](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/AzureFunctionCostOptimizedonExecutions.PNG)
Table 2

We see that with 7 Vibex servers the request time is also 7 seconds. With The estimate cost to have 1 signal every second is much lower that in previous table. But this is not possible at it would take more that 1 second (7 sec with 7 vibex) to perform the task. 
Be if the sampling rate is lowered to once per 10 seconds and the 16500 are divided over 7 Vibex Server (=customers) it is possible to get ingest the required signal values and have 3 seconds left to do the http request. This seems to be the optimal constalation.

So optimum is:
-   7 Vibex servers that can be called in parallel
-   Having sample rate of every 10 seconds to allow request to be retreived and sent to event hub processing.
-   This will lead to monthly cost for Azure Functions of €36.

###Cost of Pull Scenario using Service Fabric ###
The cost of Service Fabric are easier to determine than for azure function. Azure Service Fabric is billed on the rent of virtual machines. Service Fabric 4 dimensions where they are billend;

-   Size Virtual Machines, Virtual machine can have a wide range of sizes, ranging from a very small one "A1, with q core and 0.75 GB RAM" to very large ones "H16MR, with 16 core and 224 GB RAM". Ofcourse the price differs per size. We have chosen that D3 with 4 cores and 14 GB RAM. The system needs to run lots of actions that are memory intensive and large JSON PayLoads need to be processed. 

- 	Duration. The VM are billed on the time they are active, disregard what they do. SO the number of hours that a VM runs is billed. We assume that VMs will run constantly so 744 hours a month.

- 	Number of VMs. Service Fabric is a solution where compute and data is distributed over mulitple machines. Service Fabric can have scale sets and has a minimum of machines of 5. We assume that we need 1 Scale Set and that the minimum of 5 will be sufficient. 

- Storage. There is storage required for log and diagnostics. The cost are similar to normal storage cost and we assume that 100 Gb will be enough.

With these assumption the cost of Service Fabric will be:

Number of VM: 		5
Price of VM: 		€0.5000 / hr 
Number of hours: 	744
Storage: 			200 GB

This will result in a stable price every month of € 1,864. 


###Cost of Pull Scenario using Web Apps/Jobs ###

The cost of Web Jobs will be the same as Service Fabric as the pricing is also be done on VM (called Instances) and we assume that similar sizing is require for WebJobs as Service Fabric. Advantage for WebJobs op a Azure Web APp is that scaling can start from 1 VM, so if the system is not processing data if can be automatically scale down to 1 machine, reducing cost. As the App Service will only be used for WebJobs we assume that a B3 (4 core, 7 GB Ram) should be enough. This results in a price of:

Number of VM: 		5
Price of VM: 		€0.253 / hr 
Number of hours: 	744
Storage: 			N/A

This will result in a stable price of € 941 per month.

###Cost of Push Scenario###

The push scenario will not have cost for the retrieval and ingest into eventhub, since this is done at the customer site. This results in a very stable cost of € 0.

###Cost of Event Hub

The eventhub is billled on two dimenions:

1. Throughput units for Ingress and Egress. The Ingress is the unit that are available to get the signals in. To get data into a system like Stream ANalytics also throughput unit for egress are required, which process the same data some the some number is required. The cost of 1 Throughput unit that can handle 1 MB / sec of ingress events and 2 MB/ Sec for egress events.

2. Number of events. The event hub is billed per event and the price is € 0,024 per million events.

In the below table 3 the calculation for the price for the cars is given for 1 signal per second.

![Table 3: EventHubCost 1 per sec](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/EventHubCost 1 per sec.PNG)
Table 3: EventHubCost 1 per sec

In the table 3 can be seen that with 1 event . second and with a payload (size of signal message) the number of messages per month is 25,272 millon. This is calculated with 6 hours peak and 18 hour normal car usage.
This results in a number of MB/Sec of 1.8. This means that we need 3 Ingress Throughput units (2 + one spare for peaks) and 3 Egress Throughput units. This all results in a montly cost of € 668.

As we have seen in the Azure Functions description, it will be hard have 1 signal per second and it is more likely to have 1 signal per 10 second to allow the retrieval and processing into eventhub. This results into the following Table 4:

![Table 4: EventHubCost 1 per 10 sec](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/EventHubCost 1 per 10 sec.PNG)
Table 4: EventHubCost 1 per 10 sec

This will result in a lower number of messages (2,527 million per month) and lower price ofcours and with 1 signal per 10 second the total price will be € 87,-.

###Storage Cost###
Next tot the processing also the storage of the data introduces some cost. For the calcuation we use the following constants as shown in table 4:
![Table 4: Storage Cost constants](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/CostConstants.PNG)
Table 4: Storage Cost constants

This table 4 shows that per month there will be 2.57 TB of data will be produced. The cost for storage has 3 parts:
Storage: 	€ 8.64 per TB/month
retrieval 	€ 8.64 per TB/month
Write 		€ 2,16 per TB/month

In summary this will result in the following cost for 3 years with 1 signal per second. We assume that data will removed after 3 years, which means that cost are stable after 3 years.

Storage Cost Year 1: 	€ 2,068
Storage Cost Year 2: 	€ 5,271
Storage Cost Year 3: 	€ 8,474
Storage Cost Year >3: 	€ 8,474

The cost when 1 signal per 10 seconds is retrieved is
Storage Cost Year 1: 	€ 206,-
Storage Cost Year 2: 	€ 527,-
Storage Cost Year 3: 	€ 847,-
Storage Cost Year >3: 	€ 847,-

THe cost per month for retrieving 1 signal per 10 seconds per car is shown in table 5.

![Table 5: Storage Cost 1 per 10 sec](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/Storagecost 1 per 10 sec.PNG)
Table 5: Storage Cost 1 per 10 sec


###Cost of using Streaming Analytics###
Streaming analystics is used in this Hackfest to calculate the number of signal values. If Stream Analytics will be used in a production solutio the cost are straight forward:

Usage 															Price
Volume of data processed by the streaming job 					€0.0008/GB 
Streaming Unit (Blended measure of CPU, memory, throughput)		€0.0261/hr 

For the Beijer case we calculated 2,57 TB per month and 2 MB / sec which result in cost of
Volume of data processed by the streaming job 					€ 2,17 
2 Streaming Unit 												€ 38.90
Total 															€ 41.07


###Total Cost###
The cost of the different solution is as follows:

####Pull scenario with 1 message per 10 seconds####
Azure Function (per month):

	Storage Cost: 			€ 70,- (€ 847 / 12)

	Azure Function Cost: 	€ 36,-

	Event Hub Cost: 		€ 87,-

	Total cost: 			€ 193,-


Service Fabric (per month):

	Storage Cost: 			€ 70,- (€ 847 / 12)

	Service Fabric Cost: 	€ 1,864

	Event Hub Cost: 		€ 87,-

	Total Cost: 			€ 2,021
	

Azure WebJob (per month):

	Storage Cost: 			€ 70,- (€ 847 / 12)

	App Web Cost: 			€ 941

	Event Hub Cost:			€ 87,-

	Total Cost: 			€ 1,098

	

####Push scenario with 1 message per 10 second####

The cost for the Push scenario will only introduce Event Hub and Storage Cost:

Cost per month:

	Storage Cost: 			€ 70,- (€ 847 / 12)

	Event Hub Cost: 		€ 87,-

	Total cost: 			€ 147,-


The cost for the processing at the customer at Beijer is not included in this.

Conclusion
----------

The Hackfest has proved that it is possible to send very large amounts of data to Azure and to be able to receive and process them. The HackFest delovered some interesting outcomes

1. The push scenario is much simpler and cheaper for Beijer. This has convinced them to change their architecture from exiting pull scenario into the push scenario

2. Azure Functions are for single purpose solutions a more low cost solution than service fabric. In this scenario the functionality and services are very limited and the added value of Service Fabric is not used.

3. Altough storage is very low cost, with large amounts of data the cost accumulates when data is stored over longer periods. It is there adviceable to proces data and store aggredations instead of the raw data.


*If you’d really like to make your write-up pop, include a customer quote
highlighting impact, benefits, general lessons, and/or opportunities.*

Additional resources
--------------------

In this section, include a list of links to resources that complement your
story, including (but not limited to) the following:

-   Documentation
	- http://www.beijer.com/

-   GitHub repos
	- This repo contains all the code in Sources folder

-   Microsoft Investment and other solutions
	- Azure Vehicle Telemetry Analytics: https://gallery.cortanaintelligence.com/Solution/Vehicle-Telemetry-Analytics-9
