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

## Solution and steps ## (Sander)
The problem for Beijer as described above is to come to a technical and economical feasible solution to ingest large amounts of data from cars into Azure. 

###Overall Solution ### (Sander)
The overall solution is twofold:

1. Using a **Pull Scenario** where the solution is capable to pull data from the Vibex endpoint at all data suppliers (car fleet owners) and move that data into Azure via Event / IOT Hub
2. Using a **Push Scenario** where software running at the data supplier that pushed data to the Event / IOT Hub in Azure.

The Pull Scenario Solution
 

**The Pull Scenario Solution**

The push scenario is a scenario where every customer of Beijer has it own data storage where the data of their cars is stored. The timing is depended on the customer scenario, but each car will add data to this customer storage on on average every 2.5 seconds.

The data in the data storage of each customer need to be moved to Azure. This is done via a endpoint service that is called Vibex. Vibex is a RESTFull interface that is developed by Beijer and is able to return data from cars in JSON format. Per request the cars, the period and the signals can be defined.

Each customer will have a Vibex Endpoint that can be called to retrieve the data from Cars. Once the data is retrieved it need to be stored and Analyzed in Azure. The architecture is shown in the overall architecture that is shown in figure 1.

`![Figure 1: Overall Architecture Pull Solution]({{site.baseurl}}/images/OverallPullArchitecture.png)`

In order to be able to pull all data from all customers it is required that a solution is build that can handle the required throughput. In order to this the solution will have the following components as described in figure 2.

`![Figure 2: Pull Solution Components]({{site.baseurl}}/images/PullComponents.png)`


####Solution using Azure Function ####

####Solution using Azure Service Fabric####

####Feedback Solution using IOT Hub####


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
