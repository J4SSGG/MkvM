FROM debian:stable-slim
WORKDIR /deps

# Enable Volumes
VOLUME /config
VOLUME /media

# Install dependencies
RUN apt-get update;
RUN apt-get install -y \
    ffmpeg \
    wget \
    gpg;

# Install the .NET SDK
ENV DOTNET_SDK_VERSION 9.0
RUN wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && rm packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y dotnet-sdk-$DOTNET_SDK_VERSION; 


# Install mkvmerge (available in the mkvtoolnix package)
RUN wget -O /etc/apt/keyrings/gpg-pub-moritzbunkus.gpg https://mkvtoolnix.download/gpg-pub-moritzbunkus.gpg \
    && echo "deb [signed-by=/etc/apt/keyrings/gpg-pub-moritzbunkus.gpg] https://mkvtoolnix.download/debian/ bookworm main" | tee /etc/apt/sources.list.d/mkvtoolnix.download.list \
    && echo "deb-src [signed-by=/etc/apt/keyrings/gpg-pub-moritzbunkus.gpg] https://mkvtoolnix.download/debian/ bookworm main" | tee -a /etc/apt/sources.list.d/mkvtoolnix.download.list \
    && apt-get update \
    && apt-get install -y mkvtoolnix;


# Copy all files to the container
WORKDIR /app
COPY . ./

# Build and release process
RUN dotnet restore
RUN dotnet publish  /app/Program/Program.csproj -c Release --self-contained -r linux-x64 -o /app/out
ENTRYPOINT ["dotnet", "/app/out/Program.dll"]

