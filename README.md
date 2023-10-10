## Running the project

A project to load, view, and modify polygons defined by latlong coordinates in [wwwroot/data](wwwroot/data).

You will need the .NET 7 [dotnet runtime](https://dotnet.microsoft.com/download) to run the application server.

If you have the runtime, within the [bin/Release/net7.0/](bin/Release/net7.0/) directory you can run Spacemaker.exe directly or from the command line run:

### `dotnet spacemaker.dll`

This will run the app server.\
Open [http://localhost:5000](http://localhost:5000) to view it in the browser.

Click on a solution on the left side to load the shapes associated with that solution, clicking on a shape will select or deselect it, shapes can be modified using union or intersect after two shapes have been selected.

Changes to the proposed solutions are not persistent, you can just refresh the page to reset them.

https://github.com/TylerLig/SpacemakerRepo/assets/29814578/c5ac4a03-5c77-42ff-9aea-6dbae29682dc

