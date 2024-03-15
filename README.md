## run locally
`dotnet run environment=Development`
In production, set the environment to Production.

## build for deployment
In `Yriclium.LlmApi.csproj` replace
```
<PackageReference Include="LLamaSharp.Backend.Cpu" Version="0.10.0" /> 
<!-- <PackageReference Include="LLamaSharp.Backend.Cuda12" Version="0.10.0" /> -->
```
with
```
<!-- <PackageReference Include="LLamaSharp.Backend.Cpu" Version="0.10.0" /> -->
<PackageReference Include="LLamaSharp.Backend.Cuda12" Version="0.10.0" />
```

## [For more technical details click here](./Yriclium.LlmApi/README.md)