## Requirements
- dotnet sdk 7.0.x installed

## Setup
For development and production create `appsettings.Development.json` and `appsettings.Production.json` respectively.

Then overwrite the API key with whatever you want it to be, and overwrite the model path with the path to you LLM. Note that not all .gguf files are guaranteed to work. This API is built on LLamaSharp. If you face any issues with the models you've selected, please refer to [their community](https://github.com/SciSharp/LLamaSharp/discussions).

## IIS
This app has been designed to run on an aws EC2 instance of a windows server running IIS. In IIS configure the following environment variables:
```
ModelPath = ../models/{YOUR_MODEL_FILE},
Webhook = {YOUR_WEBHOOK_URL}
```
Though it was designed for a windows server, you could just as easily run it on a Linux server or host it locally if you know how.

## Running on network
If you want to run this on a local network (so people on the same wifi can use your stuff), find out your local/private IP. Then replace `localhost` in the commands below with your IP. Set up port forwarding on your router to serve it on your public IP. Purchase a domain and setup A records in your DNS settings to make the domain refer to your public IP.

If you're on mac like me just run `ifconfig -l | xargs -n1 ipconfig getifaddr` to find your local IP.

## running locally
`ASPNETCORE_ENVIRONMENT=Development dotnet run --urls="http://localhost:5015" --project "{PATH}/Yriclium.LlmApi/Yriclium.LlmApi.csproj"`

## running in production
`dotnet run --project "{PATH}/Yriclium.LlmApi/Yriclium.LlmApi.csproj"` or `dotnet Yriclium.LlmApi.dll` if you've published the project.

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
