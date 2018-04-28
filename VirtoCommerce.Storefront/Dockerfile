FROM microsoft/aspnet:4.7.1-windowsservercore-1709
ARG source
WORKDIR /inetpub/wwwroot
COPY ${source:-obj/Docker/publish} .
