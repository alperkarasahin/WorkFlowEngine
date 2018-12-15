# Work Flow Engine
Simple Work Flow Engine with C# .NET MVC

You can design simple work flow with using four process type.
* Process: Process which has special form or standart form
* Condition: Process which has more than one direction
* Decision Point: Process which include a method which will be developed. Return value must be [Y]es or [N]o
* Sub Process: Definition for parallel task instance area which includes sub process.

You can create a relation between all process type. Work flow diagram is showed automatically by mermaid.js 
You can select process with single click or edit process with double click inside diagram.
You can run workflow with built in work flow engine.

Application includes examples of usage below;
* UnitOfWork Pattern
* Repository Pattern
* IOC Container(Autofac)
* mermaid.js
* bootstrap
* AutoMapper
* Reflection
* Hangfire

After download you should run command below from package manager console with select Default project:WorkFlowManager.Common

PM> update-database

Happy Designing ;)


You can see detail explanation of application from https://medium.com/@alperkarasahin/work-flow-engine-with-c-net-mvc-b8ef2f7ecbdf
