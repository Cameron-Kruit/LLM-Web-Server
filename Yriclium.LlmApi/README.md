# Interaction
All endpoint require the query parameter `key` to equate to your configured API key.

## API
To learn about the REST API, spin up the app in development and go to the index page. You will see a Swagger UI for all the endpoints.

`POST /api/instant/message` - Send a message and receive a response as soon as it's ready. The output is the complete response of the LLM.

`POST /api/job/message` - Ask the LLM to generate a response to your message. The output is a job ID. You can also specify a webhook url which will be sent the prompt and the response through a POST request.

You can keep track of your jobs using `GET api/job/status` and `GET api/job/response` with the query parameter `id` equating to your job ID.

`Websocket /ws/message` - Works the same as `/api/instant/message` but through websocket protocol. Response will be a json containing `message` (the response) and `id` (to identify which query it's a response to).


# Technical info
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

## running in development
`ASPNETCORE_ENVIRONMENT=Development dotnet run --project "{PATH}/Yriclium.LlmApi/Yriclium.LlmApi.csproj"`

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
