# IIS
This app has been designed to run on an aws EC2 instance of a windows server running IIS. In IIS configure the following environment variables:
```
ModelPath = ../models/{YOUR_MODEL_FILE},
Webhook = {YOUR_WEBHOOK_URL}
```
# running locally
`ASPNETCORE_ENVIRONMENT=Development dotnet run --urls="http://localhost:5015" --project "{PATH}/Yriclium.LlmApi/Yriclium.LlmApi.csproj"`

# running in production
`dotnet run --project "{PATH}/Yriclium.LlmApi/Yriclium.LlmApi.csproj"`