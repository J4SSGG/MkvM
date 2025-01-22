FROM debian:stable-slim

WORKDIR /deps

# Install dependencies
RUN apt-get update;
RUN apt-get install -y \
    wget \
    mkvtoolnix;

# Install the .NET SDK
ENV DOTNET_SDK_VERSION 9.0
RUN wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && rm packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y dotnet-sdk-$DOTNET_SDK_VERSION; 


# Copy all files to the container
WORKDIR /build
COPY ./Source ./

# Build and release process
RUN dotnet restore
RUN dotnet publish  /build/MkvM/MkvM.csproj -c Release --self-contained -r linux-x64 -o /app
ENTRYPOINT ["dotnet", "/app/MkvM.dll"]

# Clean up
RUN rm -rf /deps
RUN rm -rf /build
RUN apt-get clean
