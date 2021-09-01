# Usage

To use:

- Unzip an Umbraco release or NuGet install an Umbraco release
- Find the directory where Umbraco's web.config is
- Add that directory as the first command line argument

Example: `.\Cultiv.Umbraco.Unattended.exe D:\Temp\Unattended`

# Config options

In the `dependencies` directory you'll find:

- `unattended.user.json` this files holds the credentials you want to use for your Umbraco install's main administrator user
- `Web.config.xdt` this file transforms your `Web.config` to use SQL CE

If you are on versions below Umbraco 8.17.0, a blanks SQL CE database will be copied in for you. From 8.17.0 onwards, Umbraco creates that empty database for you.