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

Requires the [.NET SDK](https://dotnet.microsoft.com/download) (version 10.0 or later) to be installed.

The CLI is available on [NuGet](https://www.nuget.org/packages/MoBro.Plugin.CLI) and can be installed by a single
command:

```
dotnet tool install --global MoBro.Plugin.Cli
```

To update to the latest version:

```
dotnet tool update --global MoBro.Plugin.Cli
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

If a path to a plugin project directory is provided, the plugin is automatically built and published as a temporary
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

### Update marketplace plugin information

The following commands allow updating the metadata of an already published marketplace plugin, identified by its
plugin id. All of them require a valid API key.

Update the general plugin info (prompts for input):

```
mobro marketplace-update-info --api-key [your_api_key] [plugin_id]
```

Update the plugin's logo:

```
mobro marketplace-update-logo --api-key [your_api_key] --logo-file .\logo.png [plugin_id]
```

Update the plugin's store page (markdown):

```
mobro marketplace-update-store-page --api-key [your_api_key] --store-page-file .\store-page.md [plugin_id]
```

Update the plugin's install notice (markdown):

```
mobro marketplace-update-install-notice --api-key [your_api_key] --install-notice-file .\install-notice.md [plugin_id]
```

All `marketplace-*` commands support an optional `--dev` flag to target the DEV marketplace instead of production.

----

Feel free to visit us on our [Discord](https://discord.com/invite/DSNX4ds) or [Forum](https://www.mod-bros.com/en/forum)
for any questions or in case you run into any issues.
