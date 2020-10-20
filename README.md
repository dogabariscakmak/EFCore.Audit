# EFCore.Audit

[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]

<br />
<p align="center">
  <a href="https://github.com/dogabariscakmak/EFCore.Audit">
    <img src="logo.png" alt="Logo" width="80" height="80">
  </a>

  <h3 align="center">EFCore.Audit</h3>

  <p align="center">
    A simple library that adds audit capabilities to the Entity Framework Core. 
    <br />
    <a href="https://github.com/dogabariscakmak/EFCore.Audit/issues">Report Bug</a>
    ·
    <a href="https://github.com/dogabariscakmak/EFCore.Audit/issues">Request Feature</a>
  </p>
</p>

<!-- TABLE OF CONTENTS -->
## Table of Contents

* [About the Project](#about-the-project)
* [Getting Started](#getting-started)
    * [Usage](#usage)
* [Roadmap](#roadmap)
* [License](#license)
* [Contact](#contact)


<!-- ABOUT THE PROJECT -->
## About The Project 

![EFCore.Audit][product-screenshot]

With this library, you can automatically have an audit mechanism for labeled model classes in EF Core.

<!-- GETTING STARTED -->
## Getting Started

To get start with EFCore.Audit you can clone repository or add as a reference to your project from nuget.

#### Package Manager
```Install-Package EFCore.Audit -Version 1.0.0```

#### .NET CLI
```dotnet add package EFCore.Audit --version 1.0.0```

#### ```PackageReference```
```<PackageReference Include="EFCore.Audit" Version="1.0.0" />```


<!-- USAGE EXAMPLES -->
## Usage

EFCore.Audit adds audit data by overriding and inserting audit logic. The library has its own audit entities to store audit data. To use EFCore.Audit, as a developer you need to do a couple of things.

- Since EFCore.Audit override ```SaveChanges()```, you need to inherit your DBContext class from ```AuditDbContextBase```.

```csharp
public class PersonDbContext :AuditDbContextBase<PersonDbContext>
{
    ...
}
```

- You need also provide a class which implements ```IAuditUserProvider``` to the base class. This class provides user who did the operation.

```csharp
public class PersonDbContext :AuditDbContextBase<PersonDbContext>
{
    public PersonDbContex(DbContextOptions<PersonDbContext> options,IAuditUserProvider auditUserProvider) : bas(options, auditUserProvider) { }
}
```

```csharp
public class UserProvider : IAuditUserProvider
{
    private readonly IHttpContextAccessor_httpContextAccessor;
    public UserProvider(IHttpContextAccessorhttpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public string GetUser()
    {
        return _httpContextAccessor.HttpContext.UserIdentity.Name;
    }
}
```

- Also base class ```OnModelCreating()``` method should be called.

```csharp
public class PersonDbContext :AuditDbContextBase<PersonDbContext>
 {
    ...

     protected override void OnModelCreating(ModelBuilder modelBuilder)
     {
         ...
         base.OnModelCreating(modelBuilder);
     }
 }
```

- The last thing is that labeling auditable classes and excluding desired properties of auditable classes. To label which classes will be auditable and which attributes will be excluded, you need to use ```Auditable``` attribute for classes and ```NotAuditable``` attribute for properties.

```csharp
[Auditable]
public class PersonEntity
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public GenderEnum Gender { get; set; }
    [NotAuditable]
    public string DummyString { get; set; }
}
```

<!-- ROADMAP -->
## Roadmap

See the [open issues](https://github.com/dogabariscakmak/StackExchange.Redis.Branch/issues) for a list of proposed features (and known issues).

<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE` for more information.



<!-- CONTACT -->
## Contact

Doğa Barış Çakmak - dogabaris.cakmak@gmail.com

Project Link: [https://github.com/dogabariscakmak/EFCore.Audit](https://github.com/dogabariscakmak/EFCore.Audith)

*Created my free logo at LogoMakr.com*

[issues-shield]: https://img.shields.io/github/issues/dogabariscakmak/EFCore.Audit.svg?style=flat-square
[issues-url]: https://github.com/dogabariscakmak/EFCore.Audit/issues
[license-shield]: https://img.shields.io/github/license/dogabariscakmak/EFCore.Audit.svg?style=flat-square
[license-url]: https://github.com/dogabariscakmak/EFCore.Audit/blob/master/LICENSE
[product-screenshot]: usage.gif