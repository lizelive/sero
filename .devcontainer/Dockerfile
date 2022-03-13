FROM cm2network/steamcmd as download

RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends wget ca-certificates lib32z1

ENV STEAMAPPID 298740
ENV STEAMAPP se
ENV STEAMAPPDIR "/server"
RUN mkdir -p "${STEAMAPPDIR}" && chown -R "${USER}:${USER}" "${STEAMAPPDIR}"
ENV DEBIAN_FRONTEND noninteractive

RUN ./steamcmd.sh +force_install_dir ${STEAMAPPDIR} +login anonymous +app_update ${STEAMAPPID} +quit

# See here for image contents: https://github.com/microsoft/vscode-dev-containers/tree/v0.224.2/containers/dotnet/.devcontainer/base.Dockerfile

# [Choice] .NET version: 6.0, 5.0, 3.1, 6.0-bullseye, 5.0-bullseye, 3.1-bullseye, 6.0-focal, 5.0-focal, 3.1-focal
FROM mcr.microsoft.com/vscode/devcontainers/dotnet:0-3.1

# [Optional] Uncomment this section to install additional OS packages.
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends mono-devel

RUN sudo dpkg --add-architecture i386
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends \
    file \
    xz-utils \
    libc6-i386 \
    make \
    gnupg \
    libgl1 \
    libxtst6 \
    libxrandr2 \
    libglib2.0-0 \
    libgtk2.0-0 \
    libpulse0 \
    libva2 \
    libbz2-1.0 \
    libvdpau1 \
    libva-x11-2 \
    libcurl4-gnutls-dev \
    libopenal1 \
    libsm6 \
    libice6 \
    libasound2-plugins \
    libsdl2-image-2.0-0 \
    dbus \
    dbus-x11 \
    pulseaudio \
    pciutils \
    zenity \
    libopenal1 \
    bumblebee bumblebee-nvidia primus linux-headers-generic wine wine32 winetricks

# insall steam
RUN TEMP_DEB="$(mktemp --suffix=.deb)" \
    && wget -O "$TEMP_DEB" https://cdn.cloudflare.steamstatic.com/client/installer/steam.deb \
    && apt-get -y install --no-install-recommends  "$TEMP_DEB" \
    && rm -f "$TEMP_DEB"

# RUN winetricks dotnet45

COPY --from=download /server/DedicatedServer64 /se/DedicatedServer64

RUN mkdir /se/ && echo 244850 > /se/steam_appid.txt

RUN TEMP_ZIP="$(mktemp --suffix=.zip)" \
    && wget -O "${TEMP_ZIP}" https://github.com/Gwindalmir/SEWorkshopTool/releases/download/v0.7.15-alpha/SEWorkshopTool-v0.7.15.zip \
    && unzip -d /se/DedicatedServer64/ "${TEMP_ZIP}" \
    && rm -f "${TEMP_ZIP}"

RUN chown vscode -R /se/


# [Optional] Uncomment this line to install global node packages.
# RUN su vscode -c "source /usr/local/share/nvm/nvm.sh && npm install -g <your-package-here>" 2>&1
