FROM cm2network/steamcmd:root as download

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

# [Optional] Uncomment this line to install global node packages.
# RUN su vscode -c "source /usr/local/share/nvm/nvm.sh && npm install -g <your-package-here>" 2>&1
COPY --from=download /server /server