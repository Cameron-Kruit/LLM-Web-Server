## run locally
`dotnet run environment=Development urls="http://localhost:5015"`
In production, set the environment to Production.

## build for deployment
In `LLama.WebAPI.csproj` replace
```
<PackageReference Include="LLamaSharp.Backend.Cpu" Version="0.10.0" /> 
<!-- <PackageReference Include="LLamaSharp.Backend.Cuda12" Version="0.10.0" /> -->
```
with
```
<!-- <PackageReference Include="LLamaSharp.Backend.Cpu" Version="0.10.0" /> -->
<PackageReference Include="LLamaSharp.Backend.Cuda12" Version="0.10.0" />
```