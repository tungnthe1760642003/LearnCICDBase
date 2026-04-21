# syntax=docker/dockerfile:1

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

WORKDIR /source
COPY . .

ARG TARGETARCH

# Build trực tiếp file .csproj để tránh lỗi đường dẫn trong file .sln
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish learnnet.csproj -a ${TARGETARCH/amd64/x64} --use-current-runtime --self-contained false -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
COPY --from=build /app .
USER $APP_UID

ENTRYPOINT ["dotnet", "learnnet.dll"]
