# GerwimFeiken.Publishing
This NuGet package adds additional functionality to the project. The current version (0.1.0) has limited functionality, as it only sets the `GerwimFeiken_Publishing_WindowsReleaseId` environment variable to match your Windows release ID.

The reasoning behind this is because you want to match the kernel version of the docker image with your docker host to run containers in process isolation mode (for more information, see [the Microsoft docs](https://docs.microsoft.com/en-us/virtualization/windowscontainers/deploy-containers/version-compatibility)). 

E.g. you are running:

| Operating system | Version | Result |
| ------------| ------------ | ------- |
| Windows 10 | 1809 | ltsc2019 |
| Windows 10 | 1903 | 1903 |
| Windows 10 | 2004 | 2004 |
| Windows 11 | * (any) | ltsc2022 |
| Windows Server 2016 | n/a | ltsc2016 |
| Windows Server 2019 | n/a | ltsc2019 |
| Windows Server 2022 | n/a | ltsc2022 |

\* the table above is an example and not exhaustive

## Docker-compose and dockerfile examples
When you have the package installed, the environment variable `GerwimFeiken_Publishing_WindowsReleaseId` will be set. This variable can be used in your docker-compose and dockerfiles:

docker-compose.yml:
```
version: '3.5'

services:
  website:
    image: myimage
    isolation: 'process'
    build:
      dockerfile: Dockerfile
      args:
        - WINDOWS_VERSION=${GerwimFeiken_Publishing_WindowsReleaseId}
```
dockerfile:
```
ARG WINDOWS_VERSION
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-${WINDOWS_VERSION}
```