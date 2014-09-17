ConfigMapper
=================

ConfigMapper for .NET provides interfaced configuration objects with properties mapped from ```appSettings```, allowing your ```IConfiguration``` to be injected anywhere in your code in a unit-testable manner.

Installation - NuGet
-----
```
Install-Package ConfigMapper
```

Usage
------

```
public interface IConfiguration
{
    string KeyInAppSettings { get; }
    decimal WorksWithManyTypes { get; }
    MyEnum AlsoWorksWithEnums { get; }
}

IConfiguration configuration = ConfigMapper.Map<IConfiguration>();
```

Usage is simple. Configure your DI to inject ```IConfiguration``` using the Map method into any class. The code above assumes we defined ```MyEnum``` somewhere and we have our config file as:
```
<appSettings>
  <add key="KeyInAppSettings" value="Foo"/>
  <add key="WorksWithManyTypes" value="42.0"/>
  <add key="AlsoWorksWithEnums" value="Bar"/>
</appSettings>
```

It works with anything the System.Convert.ChangeType method can handle, plus enums.

Comments/questions/bugs are very welcome, please forward them to me. Thanks!
