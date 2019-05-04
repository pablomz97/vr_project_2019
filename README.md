# VR Project

## Prerequisites
- install a recent (!) version of git and check that git-lfs is enabled, if in doubt run `git lfs install` to enable it
- check your Unity version, this project uses Unity 2019.1 so i would recommend to use a similar version to avoid any incompatibilities

## Organization

some potentially useful guidelines for project organization:
- [Structure and Style](/doc/structure.md)
- [Asset Creation](/doc/assets.md)


## CI/CD
This project is configured to automatically build and deploy all required project files, there are two build types:

### Development builds
These are run on every commit to master and will build a standalone Win64 package of the project, this is mostly for detecting build errors

### Release builds
These are run on every tagged commit to master and will:
- Build all static lighting information
- Build a standalone Win64 package
- Package and deploy the result to a private itch.io page [https://moep.itch.io/vr-2019](https://moep.itch.io/vr-2019), the password required for access is: vr2019

The auto-deployment makes it easy to distribute packaged builds to external testers without giving them access to the source repository

