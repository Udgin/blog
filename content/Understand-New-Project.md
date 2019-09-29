+++
title = "Understand your new project"
date = 2019-08-01
draft = true
[taxonomies]
categories = ["documentation"]
+++
Joining a new project is always stressful and not easy. As a new joiner, it is needed to learn a lot of information about the project: how it works, it's main functionality, how to build it, monitor, how to make changes and run tests, how to run the product locally and so on. Unfortunately, there isn't always good documentation which covers all these details, so you have to be prepared to learn and get all the details by yourself.

This article is to help a newcomer to understand the project and learn it. It describes the main steps and points, which can be used to getting knowledge about a project. Additionally, it can help to improve existing documentation of the project to fill gaps in the knowledge base.
<!-- more -->
## Introduction

### New team and who is responsible for what

The project is not able to evolve without people who are working on it. So it is quite important to know the whole team and who is responsible for what. Using this information it will be much easy to identify the right person who should be asked about questions or some particular point.

All teams are different, but there are usually the next roles:

* Research & Development team - a team which works on developing, writing code, adding new features to the product;
* Technical lead or architect - a person who is responsible for most important technical decisions in the project;z
* Project manager - a person who is responsible for the project, its timeline and teams which works on the product;
* QA team - quality assurance team or testers, who works on testing the product;
* Product owner or business representative - a person who knows business requirements of the product, how the product makes money and how to improve the product to make it more profitable;
* Release manager - a person who is responsible for release process of the product, usually only this person can change the production environment;
* DevOps member - a person who is responsible to automate the process of building, releasing of the product or any other repetitive task (e.g. CI/CD), additionally this person is responsible for non-prod environments (installing the product on it)
* SaaSOps member - a person who is supporting a product in the production, this person is responsible for SLA of a product, makes RCA (root cause analyses) of any issue in the production, sets up monitoring for a product and check logs from it.

Some of these roles can be covered by one person or a dedicated team. 

### Product overview

It is important to see the goal of the product, why it exists and what types of problems it solves. Besides, it is great to understand what market/niche the product suite covers. It will allow understanding the project, some decisions what made earlier and future tasks better. Moreover, significant competitors should be pointed to see the difference between our product and theirs.

The type of the product should be described (web service, PLM solution, ERP solution and so on) which will uncover the nature of the product. It worths to read the history of the product if it exists.

Last 2-3 releases should be checked and the list of released features, fixes should be scanned for better understanding last changes in the product and what potentially will bring a new type of bugs.

List of topics:

* Type of the product
* Goal of the product
* Market of the product
* History of the product
* Main competitors
* Recent releases

#### Key Product objectives and capabilities

Why does the product exist and how does it help customers to solve their problems? Usually, this section allows for understanding the main interface of the product, which clients work with. Additionally, it shows which functions the product suite performs. It worths to check the demo of the product which is usually showed to potential customers.

Moreover, it will be great to have a list of a specific customer or consumer, business needs which are being satisfied. A list of differences from other similar solutions and what the product is doing which distinguish it from them should be prepared.

Also, the list of direct competitors or other players in the same market, a niche of the market should be discovered and their products should be investigated more closely.

#### Key customers

Knowing the main customers and how they are using the product allows to understand what are the main workflows and features of the product.

So the next question should be answered:

* Which is the top 5-10 customer?
* Why are they using the product and not using competitor's product A, B, C?
* How are they using it? What is their main workflow?
* What are the most important features that customers use?
* How is all information about customer's usage collected?

#### Applications Overview

It is important to understand how a customer or user sees the application or product. So the description of all different solutions and/or applications which are available in the product can be covered in this chapter. All descriptions should be done from a customer's perspective without any technical details.

#### Architecture Diagrams

The are couple of ways to understand application architecture. Using any of it or combination will be valuable.

The following list shows the most common approaches:

* UML diagrams
* 4+1 Architectural view model
* The C4 Model
* Architecture Decision Records
* Dependency diagrams
* Application Map

Please read more information [here](https://herbertograca.com/2019/08/12/documenting-software-architecture/).

#### Component Details

Describe in a little bit more detail, each of the components that make up the product.

#### Data Model and Storage

Describe what kinds of data exist, and where they are stored

### Product Implementation

This section does not need to go in a deep level of detail since SaaS is less concerned about implementation details than it is about how the components of a product are put together and deployed on the infrastructure.

#### Software Overview

Summary of the languages used for each of the applications in the product suite.

#### Key Technologies

Which frameworks and components are used in the product?  List all Open-source, COTS (commercial off-the-shelf) or 3rd-party services or components which the product embeds, uses or that are instantiated.  Email service?  Indexing service?  Messaging?  CDN?  Storage solution?  Also capture any insights about why these particular frameworks, components and services were used.   Eg.  CloudFlare is used as a CDN and also provides security and protection against DDoS attacks.

### Services Architecture

Describe how the product is instantiated across the servers, networks and other IT resources that allow the product to function.

#### Cloud Services

Describe the cloud services that are used for the product and how they are used.

#### Product Instantiation

Describe how all the components, 3rd-party and cloud services that were previously mentioned are deployed on the production infrastructure.  How do they connect and how do they interact to deliver the product’s capabilities?

#### Playground Infrastructure

Describe the infrastructure of the playground environment.

### Use Cases
    
Describe all the customer and operations scenarios that define the product and its usage, especially those that have or could have an impact on operations.  Think of this as the list of all the test scenarios to validate that the product is functioning as it should and all the places where synthetic monitoring is required.  No need to go into a deep level of detail;  the goal is to provide enough info so that SaaSOps understands the scenario.  The set of scenarios should cover all customer end-points.

#### Use Case: Description

1. Business Requirement

Describe the purpose of the use case.

2. Customer’s View

Describe how the use case is initiated?

3. Data Flow

Describe how data flows and which components/services are used.  If a call sequence diagram  is appropriate, include it here.>

4. Common Incidents 

List the most frequent types of incidents related to this customer scenario.  Highlight any concerns, recommendations, playbooks, runbooks that might or would be helpful during troubleshooting.

### Limitations & Problems
    
Discuss the strengths and weaknesses of the product’s architecture/implementation.  Identify limitations and key components that incur high operation cost, describe recurring problems.

### Post-Import Opportunities

#### Import End-state

Describe the solution environment which will exist at the end of import/integration phase.  How is AWS/IBM/etc used?  Where are components installed?  Where is storage?  Databases?

#### Reduced Complexity

List and describe all the things that could or should be done to the product to reduce operational complexity.  Improved automation?  Automated deployment?  Component consolidation/elimination?

#### Cost Reduction

List and describe all the things that could or should be done to the product to reduce operational costs.  Improved automation?   Component consolidation/elimination?  Replacing services?

#### Performance Improvement

List and describe all the things that could or should be done to the product to improve scalability, resilience, uptime.

### Operational Overview

Only provide a general overview. Detailed instructions for how things are done are provided in the appropriate runbooks.

#### Deployment Strategy

Describe deployment strategy.  Provide an overview of how the product is installed/deployed into production and provisioned/configured.

#### Testing Strategy

Describe testing strategy for CI/CD, Smoke testing, etc.  How would SaaS go about validating that a deployment was successful.

#### Monitoring Strategy

Provide an overview of the strategy captured in the Monitoring Manual covering Infrastructure, APM and  Infrastructure and Synthetic monitoring.

#### SLAs

Describe support SLAs and uptime SLOs and any Maintenance Window considerations

#### Disaster Recovery and Business Continuity 

Describe approach to DR, backup/restore & data retention. Describe the backup/restore strategy in case of failure or data loss

#### High Availability & Scalability

Describe redundancy, load balancing, database replication, etc.

#### Automated and Manual OAM Activities

List all repetitive jobs that are performed automatically or need to be performed manually.  Repetitive jobs such as disk or database clean up etc.

#### Troubleshooting Tools and Approaches

Describe troubleshooting strategy. Which tools are used?

#### Incidents Analysis

Provide and explain the results of the Incidents analysis? 5 Whys

## Reference

* [Documenting Software Architecture](https://herbertograca.com/2019/08/12/documenting-software-architecture/)

