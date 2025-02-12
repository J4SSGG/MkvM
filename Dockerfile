FROM archlinux:latest

WORKDIR /deps

# Install dependencies
RUN pacman -Syu --noconfirm &&  \
    pacman -S --noconfirm dotnet-sdk wget mkvtoolnix-cli

# Copy all files to the container
WORKDIR /code
COPY ./Source ./

# Build and release process
RUN dotnet restore &&  \
    dotnet publish  /code/MkvM/MkvM.csproj -c Release --self-contained -r linux-x64 -o /app && \
    rm -rf /deps && \
    rm -rf /code
    
ENTRYPOINT ["dotnet", "/app/MkvM.dll"]

