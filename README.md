# ACR Command line tool


## Usage

```bash
acr:
  acr operates against an OCI conformant registry

Usage:
  acr [options] [command]

Options:
  --registry <registry>    Registry Login Server [default: ]
  --username <username>    Registry Username [default: ]
  --password <password>    Registry Login Server [default: ]
  --verbose                Enable verbose logging [default: False]
  --version                Show version information
  -?, -h, --help           Show help and usage information

Commands:
  repository          Repository operations
repository
  manifest            Manifest operations
manifest
  tag                 Tag operations
tag
  pull <reference>    Pull an artifact
pull
  layer               Layer Operations
layer
  push <reference>    Push an artifact
push


```

## Repository

```bash
repository:
  Repository operations

Usage:
  acr repository [options] [command]

Options:
  --registry <registry>    Registry Login Server [default: ]
  --username <username>    Registry Username [default: ]
  --password <password>    Registry Login Server [default: ]
  --verbose                Enable verbose logging [default: False]
  -?, -h, --help           Show help and usage information

Commands:
  list
repository list


```

## Repository list

```bash
Usage:
  acr repository list [options]

Options:
  --registry <registry>    Registry Login Server [default: ]
  --username <username>    Registry Username [default: ]
  --password <password>    Registry Login Server [default: ]
  --verbose                Enable verbose logging [default: False]
  -?, -h, --help           Show help and usage information


```

## Manifest

```bash
manifest:
  Manifest operations

Usage:
  acr manifest [options] [command]

Options:
  --registry <registry>    Registry Login Server [default: ]
  --username <username>    Registry Username [default: ]
  --password <password>    Registry Login Server [default: ]
  --verbose                Enable verbose logging [default: False]
  -?, -h, --help           Show help and usage information

Commands:
  show <reference>
manifest show
  config              Config operations
manifest config


```

## Manifest show

```bash
Usage:
  acr manifest show [options] <reference>

Arguments:
  <reference>

Options:
  -r, -raw                 Output the data without formatting [default: False]
  --registry <registry>    Registry Login Server [default: ]
  --username <username>    Registry Username [default: ]
  --password <password>    Registry Login Server [default: ]
  --verbose                Enable verbose logging [default: False]
  -?, -h, --help           Show help and usage information


```

## Manifest config

```bash
config:
  Config operations

Usage:
  acr manifest config [options] [command]

Options:
  --registry <registry>    Registry Login Server [default: ]
  --username <username>    Registry Username [default: ]
  --password <password>    Registry Login Server [default: ]
  --verbose                Enable verbose logging [default: False]
  -?, -h, --help           Show help and usage information

Commands:
  show <reference>
manifest config show


```

## Manifest config show

```bash
Usage:
  acr manifest config show [options] <reference>

Arguments:
  <reference>

Options:
  -r, -raw                 Output the data without formatting [default: False]
  --registry <registry>    Registry Login Server [default: ]
  --username <username>    Registry Username [default: ]
  --password <password>    Registry Login Server [default: ]
  --verbose                Enable verbose logging [default: False]
  -?, -h, --help           Show help and usage information


```

## Tag

```bash
tag:
  Tag operations

Usage:
  acr tag [options] [command]

Options:
  --registry <registry>    Registry Login Server [default: ]
  --username <username>    Registry Username [default: ]
  --password <password>    Registry Login Server [default: ]
  --verbose                Enable verbose logging [default: False]
  -?, -h, --help           Show help and usage information

Commands:
  list <repository>
tag list
  add <source> <tag>
tag add
  delete <reference>    Removes the tag and does not delete the image.
tag delete


```

## Tag list

```bash
Usage:
  acr tag list [options] <repository>

Arguments:
  <repository>    Repository to list tags of.

Options:
  --registry <registry>    Registry Login Server [default: ]
  --username <username>    Registry Username [default: ]
  --password <password>    Registry Login Server [default: ]
  --verbose                Enable verbose logging [default: False]
  -?, -h, --help           Show help and usage information


```

## Tag add

```bash
Usage:
  acr tag add [options] <source> <tag>

Arguments:
  <source>    Source image reference myregistry.azurecr.io/repos:source
  <tag>       Target tag which will be placed in the same repository.

Options:
  --registry <registry>    Registry Login Server [default: ]
  --username <username>    Registry Username [default: ]
  --password <password>    Registry Login Server [default: ]
  --verbose                Enable verbose logging [default: False]
  -?, -h, --help           Show help and usage information


```

## Tag delete

```bash
delete:
  Removes the tag and does not delete the image.

Usage:
  acr tag delete [options] <reference>

Arguments:
  <reference>    Fully qualified myregistry.azurecr.io/repo:tag or myregistry.azurecr.io/repo@sha2568fd4d2d7

Options:
  --registry <registry>    Registry Login Server [default: ]
  --username <username>    Registry Username [default: ]
  --password <password>    Registry Login Server [default: ]
  --verbose                Enable verbose logging [default: False]
  -?, -h, --help           Show help and usage information


```

## Pull

```bash
pull:
  Pull an artifact

Usage:
  acr pull [options] <reference>

Arguments:
  <reference>

Options:
  -o, --output <output>    Output Directory to download contents [default: /home/sajay/code/src/github.com/sajayantony/acr-cli]
  --registry <registry>    Registry Login Server [default: ]
  --username <username>    Registry Username [default: ]
  --password <password>    Registry Login Server [default: ]
  --verbose                Enable verbose logging [default: False]
  -?, -h, --help           Show help and usage information


```

## Layer

```bash
layer:
  Layer Operations

Usage:
  acr layer [options] [command]

Options:
  --registry <registry>    Registry Login Server [default: ]
  --username <username>    Registry Username [default: ]
  --password <password>    Registry Login Server [default: ]
  --verbose                Enable verbose logging [default: False]
  -?, -h, --help           Show help and usage information

Commands:
  pull <reference>
layer pull


```

## Layer pull

```bash
Usage:
  acr layer pull [options] <reference>

Arguments:
  <reference>    Layer reference e.g. myregistry.azurecr.io/repo@sha25627e17ff3

Options:
  -o, --output <output>    Filename to download contents. Defaults to digest of the layer
  --registry <registry>    Registry Login Server [default: ]
  --username <username>    Registry Username [default: ]
  --password <password>    Registry Login Server [default: ]
  --verbose                Enable verbose logging [default: False]
  -?, -h, --help           Show help and usage information


```

## Push

```bash
push:
  Push an artifact

Usage:
  acr push [options] <reference>

Arguments:
  <reference>

Options:
  -d, --directory <directory>                Directory path of the contents to upload which containts manifest.json and config.json and other tar files.
  -f, --file <file>                          File path of the content to upload
  --config-media-type <config-media-type>    File path of the content to upload
  --registry <registry>                      Registry Login Server [default: ]
  --username <username>                      Registry Username [default: ]
  --password <password>                      Registry Login Server [default: ]
  --verbose                                  Enable verbose logging [default: False]
  -?, -h, --help                             Show help and usage information


```
