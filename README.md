# Optimizely Search Server
This project was not intended to be able to compile.  It's bits of code that need to be integrated into an Optimizely v11 site.

This is intended to work in tandem with the https://github.com/markrullo/OptimizelySearchAngular project to produce a fully functional search experience.

The intent with making this public is to solicit feedback on how to improve the code and make it more useful to the community.

We found the documentation for the Optimizely Search API to be lacking and the examples to be incomplete, particularly in regards to Unified Search.  This project is an attempt to fill in the gaps and provide a starting point for other developers.

This project includes:
* Search index projections
* Search API endpoint
* Block to hold the settings
	* Select all the categories to display, up to 3 sections of them
	* Indicate page types to filter by
	* Holds the page types to allow the client to segment by
