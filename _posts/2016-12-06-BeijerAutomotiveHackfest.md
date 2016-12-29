Begin with an intro statement with the following details:

-   Solution overview

-   Key technologies used

-   Core Team: Names, roles and Twitter handles

Customer profile
----------------

This section will contain general information about the customer, including the
following:

-   Company name and URL

-   Company description

-   Company location

-   What are their product/service offerings?

Problem statement
-----------------

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
    streams.

-   How does the architecture need to be setup in order to meet the requirements
    for handling 50.000 vehicles recording data on 1 per second interval.

-   What are the costs involved in implementing the architecture using Azure
    PaaS services and how can these costs be minimized.

-   How can the current architecture be optimized for the cloud to improve
    performance and scalability.

-   Which alternatives are there in the implementation and what are their pro’s
    and cons.

To get to an acceptable of details the hackfest was scoped to focus on data
ingest. Further processing and analyzing the data is considered a project by
itself with many options to consider and include in a proof of concept.

*If you’d really like to make your write-up pop, include a customer quote that
highlights the customer’s problem(s)/challenges.*

Overall solution and steps
--------------------------

### Adding IoT Hub for real-time data and cloud to device communication

Although the Vetuda system focusses on the ingestion of large amounts of data it
does make sense to categorize these data streams. Data can be handled as it
comes into the system to result in near-real-time alerts (hot path) while at the
same time it can be analyzed later on together with data accumulated over time
(cold path).

Vetuda also has plans to extend their services with new types of vehicles that
might not even involve the intermediate data provider role but consists of
devices (vehicles) connecting to the cloud back-end directly.

For these requirements to be satisfied we added one of the core Azure IoT
Services into the mix, IoT Hub. IoT Hub makes sense for those scenario’s where a
direct connection between an end node device or a gateway and the cloud is
involved. It offers high-volume data ingest and a registry for potentially
millions of devices. It communicates over multiple protocols like HTTP (mostly
for backwards compatibility), MQTT (as it is the most popular protocol in M2M
and IoT currently) and AMQP (a new, Microsoft backed, member in the IoT family
offering updates specs compared to MQTT).

When a fleet of devices is directly connected to the Vetuda back-end it also
places the responsibility of device management in that domain. Luckily IoT Hub
device management has multiple features that accelerate implementing device
management.

Under the hood IoT Hub is based on Azure Event Hubs and on the consumer side
(reading the data from the buffer) the developer experience is identical

The majority of your win artifacts will be included in this section, including
(but not limited to) the following: Pictures, drawings, architectural diagrams,
value stream mappings and demo videos.

This section should include the following details:

-   What was worked on and what problem it helped solve.

-   Architecture diagram/s (**required**). Example below:

IoT Architecture Diagram

IoT Architecture Diagram

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