---
layout: post
title:  "Add title with customer name here"
author: "Martin Bald"
author-link: "#"
#author-image: "{{ site.baseurl }}/images/authors/photo.jpg"
date:   2016-05-19
categories: [IoT]
color: "blue"
#image: "{{ site.baseurl }}/images/imagename.png" #should be ~350px tall
excerpt: Add a short description of what this article is about.
language: The language of the article (e.g.: [English])
verticals: The vertical markets this article has focus on (e.g.: [Energy, Manufacturing & Resources, Financial Services, Public Sector, “Retail, Consumer Products & Services”, Environmental, Communications/Media, Transportation & Logistics, Smart Cities, Agricultural, Environmental, Healthcare, Other])
---

Begin with an intro statement with the following details:

-   Solution overview

-   Key technologies used

-   Core Team: Names, roles and Twitter handles

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

	Beijer Automotive are expanding their business by re-collecting and extending
	the telematics their customers are generating and transforming this input into a
	new data service called Vetuda©. Based on this data service they will be able to
	unlock even more scenarios in the field of traffic management, road conditions,
	public safety or whatever opportunity this vast dataset might disclose.

	Beijer Automotive are expanding their business by re-collecting and extending
	the telematics their customers are generating and transforming this input into a
	new data service called Vetuda©. Based on this data service they will be able to
	unlock even more scenarios in the field of traffic management, road conditions,
	public safety or whatever opportunity this vast dataset might disclose.

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


This section will define the problem(s)/challenges that the customer wants to
address with an IoT solution. Include things like costs, customer experience,
etc.

*If you’d really like to make your write-up pop, include a customer quote that
highlights the customer’s problem(s)/challenges.*

## Solution and steps (Sander) ##
The problem for Beijer as described above is to come to a technical and economical feasible solution to ingest large amounts of data from cars into Azure in order to process and anlyse the data into useful information and alerts. 

###Setting the scene in numbers (Sander) ###
In order to set the scene on how much data will be ingest and processed by the solution this paragraph will define the number. In the Figure 0, the numbers that are set by Beijer are given. These are the number that the solution needs to process once the solution is fully deployed. However it is still required that the solution needs to scale to larger number in the future.

![Figure 0: Numbers](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/Constants.PNG)`

The data need to be gathered from 50000 cars for every second. We do the test only for a number signal/messages in order to test the signal with many small messages and less larger messages. We start with 1 signal value per message.
Because not all cars will drive every moment the number of cars that is driving and producing data is set to 15% (occupation) on average and 33% in peak moments. We assume that about 6 hours per day the peak level occurs. 
We also define the number of signal values that need to be retrieved per second. This will be 1 as we want at least know the speed of a car every second. This might however be decreased of this lead to oversampling or a lower sampling rate does not decrease the quality of the alerts.

With the input as described above this will lead to a number of messages and bytes that need to get ingested by the solution as described in Figure 1.

![Figure 1: Numbers](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/Numbers.PNG)`

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

`![Figure 2: Overall Architecture Pull Solution](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/OverallPullArchitecture.png)`

Every car / vehicle has a device to capture the car data via the CanBus. This data is sent to each customer (fleet owner). They store in they own data store and perform their local business processes on it. The data is exposed via a secure ViBeX REST API. In the Beijer Solution in Azure a solution runs that pulls this data into Azure.

In order to be able to pull all data from all customers it is required that a solution is build that can handle the required throughput. In order to this the solution will have the following components as described in figure 3.

`![Figure 3: Pull Solution Components](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/PullComponents.png)`


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

`![Figure 4: Pusg Solution Components](https://github.com/svandenhoven/IoTArchitecture/blob/master/images/PushComponents.png)`

The customer has instead of the an deployment of the ViBeX Pull Api an alternative with an deployment of a ViBex Push Service. This ViBeX Push Service will directly sent car data to the Data Retriever (same as described in Pull Scenario). This Data Retriever will service has hub for the data and consequently sent it to storage and the Data Processor. The Data Processor will create alerts that will be sent to Beijer's Clients

The main advantage is that for this architecture no Job Scheduler and Job Worker is required which simplifies it. The numbers of messages, bandwidth usage and storage will remain the same.


----------

The majority of your win artifacts will be included in this section, including
(but not limited to) the following: Pictures, drawings, architectural diagrams,
value stream mappings and demo videos.

This section should include the following details:

-   What was worked on and what problem it helped solve.

-   Architecture diagram/s (**required**). Example below:

IoT Architecture Diagram

IoT Architecture Diagram


----------

**Directions for adding images:**

1.  Create a folder for your project images in the “images” folder in the GitHub
    repo files. This is where you will add all of the images associated with
    your write-up.

2.  Add links to your images using the following absolute path:

`![Description of the image]({{site.baseurl}}/images/projectname/myimage.png)`

Here’s an example:

`![Value Stream Mapping]({{site.baseurl}}/images/orckestra/orckestra2.jpg)`

Note that capitalization of the file name and the file extension must match
exactly for the images to render properly.

*If you’d really like to make your write-up pop, include a customer quote that
highlights the solution.*

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


####Push Scenario####

####Feedback Solution using IOT Hub####

This section will include the following details of how the solution was
implemented:

-   Security details

-   Device used (be specific, details if PLC, microcontroller, etc.)

-   Device messages sent (packet size, frequency of send/day/device, number of
    messages)

-   SDKs used, languages, etc.

-   Code artifacts

-   Pointers to references or documentation

-   Learnings from the Microsoft team and the customer team

Cost
----------
This section describes the cost of the different scenarios

###Cost of Pull Scenario using Azure Functions ###

###Cost of Pull Scenario using Web Apps/Jobs ###

###Cost of Pull Scenario using Service Fabric ###

###Cost of Push Scenario###

###Cost of using IOT Hub###

###Total Cost###

Conclusion
----------

This section will briefly summarize the technical story with the following
details included:

-   Measurable impact/benefits resulting from the implementation of the
    solution.

-   General lessons:

-   Insights the team came away with.

-   What can be applied or reused for other environments or customers.

-   Opportunities going forward:

-   Details on how the customer plans to proceed or what more they hope to
    accomplish.

*If you’d really like to make your write-up pop, include a customer quote
highlighting impact, benefits, general lessons, and/or opportunities.*

Additional resources
--------------------

In this section, include a list of links to resources that complement your
story, including (but not limited to) the following:

-   Documentation

-   Blog posts

-   GitHub repos

-   Etc…
