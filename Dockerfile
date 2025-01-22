FROM archlinux:latest

WORKDIR /deps

# Install dependencies
RUN pacman -Syu --noconfirm
RUN pacman -S --noconfirm dotnet-sdk wget mkvtoolnix-cli

# Copy all files to the container
WORKDIR /code
COPY ./Source ./

# Build and release process
RUN dotnet restore
RUN dotnet publish  /code/MkvM/MkvM.csproj -c Release --self-contained -r linux-x64 -o /app
ENTRYPOINT ["dotnet", "/app/MkvM.dll"]

# Clean up
RUN rm -rf /deps
RUN rm -rf /code
