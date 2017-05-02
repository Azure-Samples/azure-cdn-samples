This is sample code to export core analytics of Cdn endpoints to a csv file. Please do the following steps before you run this code:
1. Launch Visual Studio, and create a C# console application
2. Install WindowsAzure.Storage nuget package
	a. Right click your project in Solution Explorer and choose "Manage NuGet Packages""
	b. Select "nuget.org" from "Package source"
	c. Search "WindowsAzure.Storage" under "Browse", and install the latest version
3. Newtonsoft nuget package should be installed with WindowsAzure.Storage, if not, install it with the same procedure
4. Replace App.config and Program.cs by the ones provided in this repository
5. Open App.config, and replace the value for StorageConnectionString with the connection string of your storage account
6. Open App.config, and replace the value for OutputCsvFilePath with the file path you want to export to
7. Run the project, when finished, the csv file will be created. Open the csv file (preferrably by Microsoft Excel).
8. Each core analytics metric is represented by a column. On top of that, the csv file should also have these columns:
	a. Profile
	b. Endpoint
	c. Hostname
	d. Time

