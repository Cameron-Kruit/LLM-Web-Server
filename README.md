# LLM Webserver
This is a webserver which allows users to interact with an LLM. This is not a chatbot. This system is designed for an LLM to answer a single prompt. You can engineer your own prompts with context to get it to perform like a chatbot. Supported protocols are REST and Websockets.

## Swagger UI
When this program is run in development mode, you can access Swagger UI to get better insight in all the endpoints and how they function on `http://0.0.0.0:80` and `https://0.0.0.0:443`
 
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

## [For more about which language models you can use click here](./models/README.md)
