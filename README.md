<h2 align="center">Helpers Library for .Net 5</h2>
  
 <div align="center"> 
  
[![license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/Milvasoft/Milvasoft/blob/master/LICENSE)  [![NuGet](https://img.shields.io/nuget/v/Milvasoft.Helpers)](https://www.nuget.org/packages/Milvasoft.Helpers/)   [![NuGet](https://img.shields.io/nuget/dt/Milvasoft.Helpers)](https://www.nuget.org/packages/Milvasoft.Helpers/) 

</div>


![](https://i.hizliresim.com/12q7jh2.gif)

<h3 align="center">Milvasoft library helps you to create your .Net projects in the simplest way.</h3>
<h3 align="center">Thanks to the helper methods in it, it makes your work easier.</h3>
<br>

***

<h3 align="center">The library includes the following features:</h3>

***
<h3 align="center">Multi Tenancy</h3>
It’s a single codebase that responds differently depending on which “tenant” is accessing it, there’s a few different patterns you can use like.<br>

**Application level isolation:** Spin up a new website and associated dependencies for each tenant.<br>
**Multi-tenant app each with their own database:** Tenants use the same website, but have their own database.<br>
**Multi-tenant app with multi-tenant database:** Tenants use the same website and the same database.
<br><a href="https://github.com/Milvasoft/Milvasoft/wiki/Multi-Tenancy"><b>Visit the library's Multi Tenancy page.<b></a><br>


***


<h3 align="center">Data Access</h3>
Every application that persists data, needs to store the data some place and retrieve it back. Usually, it is into some database through various CRUD operations.
Often, there are complex operations performed on the data before sending it in a response. These operations include merging data from different sources, filtering, validating, etc.<a href="https://medium.com/@k.ramankishore/data-access-layer-dao-why-is-it-needed-how-to-structure-it-47d00d84f00c"> For more.</a><br>
<a href="https://github.com/Milvasoft/Milvasoft/wiki/DataAccess"><b>Visit the library's data access page.<b></a><br>


***

<h3 align="center">Common Helper</h3>

Contains common helper methods. <br><a href="https://github.com/Milvasoft/Milvasoft/wiki/Common-helper"><b>Visit the library's common helper page.<b></a><br>


***

<h3 align="center">Response</h3>
A useful class for returning the results of end-points in controllers. It returns information containing important data such as status code, message, status, and result.<br><a href="https://github.com/Milvasoft/Milvasoft/wiki/Response"><b>Visit the library's response page.<b></a><br>


***

<h3 align="center">Request</h3>
HTTP defines a set of request methods to indicate the desired action to be performed for a given resource. Although they can also be nouns, these request methods are sometimes referred to as HTTP verbs. Each of them implements a different semantic, but some common features are shared by a group of them: e.g. a request method can be safe, idempotent, or cacheable. <a href="https://github.com/Milvasoft/Milvasoft/wiki/Attributes">
<br><a href="https://github.com/Milvasoft/Milvasoft/wiki/Request"><b>Visit the library's request page.<b></a><br>

***


<h3 align="center">Filter</h3>
It contains helper methods for filtering operations.
<br><a href="https://github.com/Milvasoft/Milvasoft/wiki/Filter"><b>Visit the library's filter page.<b></a><br>

***
<h3 align="center">Exceptions</h3>
It contains custom exceptions.
<br><a href="https://github.com/Milvasoft/Milvasoft/wiki/Exceptions"><b>Visit the library's exceptions page.<b></a><br>

***
<h3 align="center">Extensions</h3>
Extension methods enable you to "add" methods to existing types without creating a new derived type, recompiling, or otherwise modifying the original type. Extension methods are static methods, but they're called as if they were instance methods on the extended type.
<br><a href="https://github.com/Milvasoft/Milvasoft/wiki/Extensions"><b>Visit the library's extensions page.<b></a><br>

***

<h3 align="center">Models</h3>
A model is a class with .cs (for C#) as an extension having both properties and methods. Models are used to set or get the data. If your application does not have data, then there is no need for a model. If your application has data, then you need a model.
<br><a href="https://github.com/Milvasoft/Milvasoft/wiki/Models"><b>Visit the library's models page.<b></a><br>

***

<h3 align="center">Regex Matcher</h3>
Regular expressions provide a powerful, flexible, and efficient method for processing text. 
<br><a href="https://github.com/Milvasoft/Milvasoft/wiki/Regex-Matcher"><b>Visit the library's Regex Matcher page.<b></a><br>

***
<h3 align="center">Network Util</h3>
Provides access network statistics.
<br><a href="https://github.com/Milvasoft/Milvasoft/wiki/Network-Util"><b>Visit the library's Network Util page.<b></a><br>

***
<h3 align="center">Encryption</h3>
Encryption is a way of scrambling data so that only authorized parties can understand the information. In technical terms, it is the process of converting human-readable plaintext to incomprehensible text, also known as ciphertext. In simpler terms, encryption takes readable data and alters it so that it appears random. Encryption requires the use of a cryptographic key: a set of mathematical values that both the sender and the recipient of an encrypted message agree on. More information for encryption, visit <a href="https://www.cloudflare.com/learning/ssl/what-is-encryption/">here.</a>
<br><a href="https://github.com/Milvasoft/Milvasoft/wiki/Encryption"><b>Visit the library's Encryption page.<b></a><br>

***
<h3 align="center">Caching</h3>
The structures that allow us to produce faster results by caching the relevant data at a certain interval, instead of obtaining the original data again, are called caches, rather than obtaining the rarely updated data from the data shown to the end user through the database at every request.
<br><a href="https://github.com/Milvasoft/Milvasoft/wiki/Caching"><b>Visit the library's Caching page.<b></a><br>

***
<h3 align="center">Milva Mail Sender</h3>
It allows you to send e-mail in the simplest way.
<br><a href="https://github.com/Milvasoft/Milvasoft/wiki/Milva-Mail-Sender"><b>Visit the library's Milva Mail Sender page.<b></a><br>

***
<h3 align="center">File Operations</h3>
It is a helper class that contains operations such as file upload, file download, file verification.
<br><a href="https://github.com/Milvasoft/Milvasoft/wiki/File-Operations"><b>Visit the library's File Operations page.<b></a><br>

***
<h3 align="center">Identity</h3>
Manages users, passwords, profile data, roles, claims, tokens, email confirmation, and more.
<br><a href="https://github.com/Milvasoft/Milvasoft/wiki/Identity"><b>Visit the library's Identity page.<b></a><br>

***

<h3 align="center">Attributes</h3>
Attributes provide a powerful method of associating metadata, or declarative information, with code (assemblies, types, methods, properties, and so forth). After an attribute is associated with a program entity, the attribute can be queried at run time by using a technique called reflection. For more information, see <a href="https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/reflection">Reflection</a> (C#). More information for attributes, visit <a href="https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/attributes/">here.</a> <br><a href="https://github.com/Milvasoft/Milvasoft/wiki/Attributes"><b>Visit the library's attributes page.<b></a><br>
