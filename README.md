# Interpretarr

Interpretarr is a middleware that sits between Sonarr/Prowlarr and indexer sites to handle non-standard releases from indexers.

## How does it work?

- Sonarr/Prowlarr requests a release from an indexer
- The indexer returns a release that is not supported by Sonarr/Prowlarr

## How does Interpretarr fix this?
- Interpretarr receives the request from Sonarr/Prowlarr
- Interpretarr requests the release from the indexer
- Interpretarr parses the release and returns a supported release to Sonarr/Prowlarr
- Sonarr/Prowlarr downloads the release
- Interpretarr renames the release to a supported format
- Sonarr/Prowlarr imports the release

## Supported Release Formats
- Formula 1 (2014-Present) 
	- Format: Formula.1.YEAR.Round.XX.LOCATION.TYPE.RELEASE_INFO
	- Example: Formula.1.2021.Round.01.Australia.Qualifying.1080p.WEB.h264
	- Output: Formula.1.S2021E6.Australia.Qualifying.1080p.WEB.h264
	- Indexer: 1337x / torrentleech

## Installation
- Download source code
- Extract the release to a folder
- Update .env.template with your environment variables and rename to .env
- Update 1337xMiddleman.yml with to your desired url for Interpretarr
- Copy 1337xMiddleman.yml to your Prowlarr install under /Definitions/Custom/1337xMiddleman.yml
- Configure Prowlarr to use 1337xMiddleman.yml
- Build and run the docker container
```
docker build -t interpretarr .
docker run -d --name=interpretarr -p 5000:5000 --env-file .env interpretarr
```

## TODO
 - [ ] More indexers
 - [ ] Make Flaresolverr optional
 - [ ] Configurable services ie. url, flaresolverr support, categories, priority?, enabled/disabled etc.
 - [ ] Move more logic to Prowlarr/Cardigann definitions, could be a better way of handling url rewrites
 - [ ] Use reflection to register helpers so they don't need to be defined in Program.cs
 - [ ] Proxy support
 - [ ] More thorough testing capabilities
 - [ ] More thorough testing of SonarrService and how it interacts with qbittorrent