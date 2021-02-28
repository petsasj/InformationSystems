# Digital Hermes API
> Digital Hermes as a means of Telecommunication Data consistency

Digital Hermes API was released as a part of my academic studies.
It provides the ability of Telecommunication Data Submission from Providers in order to maintain a central database of every infrastructure.
It features JWT Authentication, ability to query data, if authenticated, and multiple ways to verify the final outcome of the data submitted.

![](header.png)

## Prerequisites
This solution has been developed using Microsoft dotnet Core.
Dotnet core in itself is cross-platform and should run on all OS environments.
There are no extra requirements, other than the Docker-Engine

## Installation

OS X & Linux:

```sh
git clone https://github.com/petsasj/InformationSystems.git
```

Windows:
Download repository via Github and unzip.

Open terminal/console targetting the root folder. 

```sh
docker-compose build
docker-compose up -d
```

## Usage example

After verifying that you are meeting all the installation requirements, proceed below.

To get started, after cloning the git repository do the following:
* Navigate into the root folder. 
* Modify the .env file according to your needs.
* Make sure that you modify the NETWORKMODE attribute. In its default setting it is setup for use in a custom Docker network. Default values are host or bridge.
* Set Database passwords according to your needs
* Run docker-compose build
* Run docker-compose up -d
* Using the POSTMAN Collections below
* That's it!


## POSTMAN Collections

The following POSTMAN Collection corresponds to the Web API Actions within the application FIWAREHub.Web

Namely the most useful actions are:
* Add new Company
* Authenticate
* Submit Data
* Query Data Submission Outcome
* Query All Data

[![Run in Postman](https://run.pstmn.io/button.svg)](https://www.getpostman.com/collections/66d6e857cb8c3c35ba77)

## Release History
* 1.0.0
	* Initial Release
* 0.2.0
	* Software architecture implemented
* 0.0.1
    * Work in progress

## Meta

Yiannis Petsas - el05628@mail.ntua.gr

 Distributed under the GPL license. See ``LICENSE`` for more information.

[https://github.com/petsasj/InformationSystems](https://github.com/petsasj/)

## Contributing

1. Fork it (<https://github.com/petsasj/InformationSystems/fork>)
2. Create your feature branch (`git checkout -b feature/fooBar`)
3. Commit your changes (`git commit -am 'Add some fooBar'`)
4. Push to the branch (`git push origin feature/fooBar`)
5. Create a new Pull Request

<!-- Markdown link & img dfn's -->
[npm-image]: https://img.shields.io/npm/v/datadog-metrics.svg?style=flat-square
[npm-url]: https://npmjs.org/package/datadog-metrics
[npm-downloads]: https://img.shields.io/npm/dm/datadog-metrics.svg?style=flat-square
[travis-image]: https://img.shields.io/travis/dbader/node-datadog-metrics/master.svg?style=flat-square
[travis-url]: https://travis-ci.org/dbader/node-datadog-metrics
[wiki]: https://github.com/yourname/yourproject/wiki