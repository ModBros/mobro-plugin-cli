# mobro-plugin-cli

[![Nuget](https://img.shields.io/nuget/v/MoBro.Plugin.CLI?style=flat-square)](https://www.nuget.org/packages/MoBro.Plugin.CLI)
![GitHub](https://img.shields.io/github/license/ModBros/mobro-plugin-cli)
[![MoBro](https://img.shields.io/badge/-MoBro-red.svg)](https://mobro.app)
[![Discord](https://img.shields.io/discord/620204412706750466.svg?color=7389D8&labelColor=6A7EC2&logo=discord&logoColor=ffffff&style=flat-square)](https://discord.com/invite/DSNX4ds)

This is the official repository of the MoBro Plugin CLI.  
This CLI provides an easy way to test and publish MoBro plugins built using
the [MoBro Plugin SDK](https://github.com/ModBros/mobro-plugin-sdk).

## Plugin documentation

Detailed developer documentation on how to create a MoBro plugin using
the [MoBro Plugin SDK](https://github.com/ModBros/mobro-plugin-sdk) can be found
on [developer.mobro.app](https://developer.mobro.app).

## Installation

The CLI is available on [NuGet](https://www.nuget.org/packages/MoBro.Plugin.CLI) an can be installed by a single
command:

```
dotnet tool install --global MoBro.Plugin.Cli
```

## Usage

After installation the CLI can be invoked by the `mobro` keyword.

### Publish a plugin

Building and publishing a plugin to a .zip file can be done by simply invoking the `publish` command, providing the path
to the plugins project directory.  
Optionally an output folder can be specified by the `--output` option (defaults to: `.`).

```
mobro publish .\Plugin.Template
```

The plugin is published as a .zip to the output folder named `[plugin_name]_[version].zip`

### Install a plugin for testing

A plugin can be installed to MoBro for testing purposes by invoking the `install` command.  
The provided path can either be a plugin project directory or an already published plugin .zip file.

Requires a running MoBro instance on the same machine.

```
mobro install .\Plugin.Template
```

If a path the a plugin project directory is provided, the plugin is automatically built and published as a temporary
.zip file before being installed.

### Publish a plugin to marketplace

To make a plugin publicly available it can be published to the MoBro marketplace by invoking the `marketplace-publish`
command and passing a published plugin .zip file.

If the plugin is not yet available in the marketplace, the CLI will prompt for some input and create it.  
If the plugin is already available in the marketplace, a new version will be published for the existing plugin.

```
mobro marketplace-publish --api-key [your_api_key] .\example_plugin_0.0.1.zip
```

Note: Accessing the marketplace requires a valid API key.

----

Feel free to visit us on our [Discord](https://discord.com/invite/DSNX4ds) or [Forum](https://www.mod-bros.com/en/forum)
for any questions or in case you run into any issues.
