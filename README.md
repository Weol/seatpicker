I am experimenting with a project strucure that hides any internals between layers. The domain layer cannot refernce the Application or Adapater layer, if it needs to communicate outside of its own project it will need to define a port that either the Application or Adapter layer must implement. Likewise with the Application layer, it can only communicate with the domain layer by invoking its services, if it needs an adapter then it must create a port that the Adapter layer implements.

The application and adapter layer is represented with each their respectivly named projects. The domain layer consists of one or more projects, where each project is its own bounded-context. If the bounded-contexts need to communicate they must do it by creating a port that must be implemented by the application layer.

In order to facilitate this there is a fourth project called `Functions`. This project project contains the `Program.cs` file that configures the Azure Functions host and its configuration. It is here that the extension methods for each layer are invoked, such as `AddApplication()` and `AddAdapters()`. We need this project because if this was contained in the same project as the Application layer then we would have circular references, which is not allowed.

![diagram](https://user-images.githubusercontent.com/10326080/195200908-db080d86-24ea-4c44-b09b-aea4b5fb8ba4.svg)
