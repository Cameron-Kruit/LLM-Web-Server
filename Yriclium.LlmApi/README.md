# Interaction
## Authentication
Want no authentication on your endpoints? Omit the `ApiKey` field from your appsettings.json.
If you want API key authentication, set the `ApiKey` field to your desired API key in appsettings.yourenvironment.json.
If you want some form of external authentication set the field `ExternalAuth` to `true`. This will generate a new API key every hour. Your auth server can retrieve this generated key from `GET /auth/key` using the `ApiKey` from appsettings. You can grant the generated keys to your authenticated users, and keep your `ApiKey` a secret between your auth server and the LLM server.

All endpoints starting with `/api` or `/ws` require the query parameter `key` or the HTTP header `X-API-KEY` to equate to your configured API key or the generated key if you've got this configured. The endpoint `auth/key` also requires this query param or HTTP header.

Besides using the built in key generator for external authentication, it might be wise to set up some sort IP whitelisting limitations on the auth endpoint if your setup allows for this. Another option is to keep this API open, set up a firewall that only allows your LLM server to communicate with your auth server, and then run a proxy on your auth server. Or you could fork this repo and build your own custom auth directly into the code base.

## API
To learn about the REST API, spin up the app in development and go to the index page. You will see a Swagger UI for all the endpoints.

`GET /auth/key` - If `ExternalAuth` has been set to true, this will return the current generated API key. Very important you don't leak your static API key or people could bypass your authentication flow.

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

## persistence
Jobs are stored in memory. They get lost when the app or server shuts down. If you want to save jobs, you should have an external app do so. Using webhooks you can update the entries in your database. I will write some functionalities which will execute pending jobs from your persistent storage later.