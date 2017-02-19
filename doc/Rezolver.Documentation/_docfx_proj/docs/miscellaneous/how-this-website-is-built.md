# How this website is built

We've used [docfx](https://dotnet.github.io/docfx/) to build this documentation site - a cool tool built by Microsoft
which generates API documentation directly from source code.  It also solves the typically knotty problem of mixing
conceptual documentation such as the content in this developer guide with auto-generated API documentation.

The website project itself is just a standard Asp.Net website - the source for which is available in our repo on 
github.  The rest of content on this page refers to files in that project, so you might want to follow the 
'Improve this Doc' link above to get to this file's location in the repo, and navigate up to the project root.

We're using the recommended [`docfx.console`](https://www.nuget.org/packages/docfx.console/) nuget package to add docfx 
preprocessing to this website's build process.  However, our usage of the package diverges from the documented process in order
to customise the underlying bootstrap theme that the documentation pages use, as well as to add additional features such as
browser-specific icons etc.

As such, this project looks slightly different from a 'standard' docfx web project, and this document describes what we've done.

> [!WARNING]
> Feel free to use this site as a model for your own OSS project's documentation site, but please *do not* use it to publish an
> alternative version of the Rezolver documentation!  If you have a suggestion for content to be added to the site, you can 
> follow the 'Improve this Doc' link above and submit a pull request to the [repo on github](https://github.com/zolutionsoftware/rezolver) 
> to have it included in the build.

## 1 - Template usage

As mentioned before, the site is using a custom build of bootstrap as its backbone - replacing the version that the docfx
team have included by default.

This was *not* done by building bootstrap into the site's `main.css`, or otherwise using a theme as per the docfx documentation, 
but by breaking apart the `default` template and inserting our custom build of bootstrap into the build process of the default 
theme's own `docfx.vendor` `js` and `css` files.

The reason for this is that we don't want to include all of docfx's bootstrap content only to overwrite it with another version.

### 1.1 - Default template

Upon installing the docfx.console package (or upgrading it), the first thing we do is to extract the default theme into
the `~/_docfx_themes/default` folder.

This can be done easily by opening a console to the install folder of the docfx package (or whichever folder you've downloaded
the docfx binaries to if not using nuget) and running the command:

`docfx template export default`

_(As per the docfx [documentation](https://dotnet.github.io/docfx/tutorial/howto_create_custom_template.html#merge-template-with-default-template))_

> [!TIP]
> It's worth becoming familiar with how the default template is built, and how you can 'peek' into what's in the latest version by 
> taking a look directly at the source at https://github.com/dotnet/docfx/tree/dev/src/docfx.website.themes (correct at the time
> of writing - Feb 2017).
> 
> The contents of those folders there are similar to what you get when you export a template, except a docfx template is actually 
> a combination which includes the `common` template folder which you will see in that source tree.


The entire contents of the generated folder are then copied into this web project's local copy of the template.

### 1.2 - 'Rezolver' template

The custom Rezolver template is found in `~/_docfx_themes/rezolver`.

This theme replaces the standard `docfx.vendor.js` and `docfx.vendor.css` files (containing jquery, bootstrap, 
lunr, highlightjs and more) so that the Rezolver bootstrap theme and the Rezolver highlightjs themes in `~/styles/`
(built using LESS) are used instead of those which are baked into docfx.

The versions of these files present in the default template are built by gulp (the gulpfile can be found in 
our project at `~/_docfx_themes/default/gulpfile.js`).

We also use gulp to run our LESS compilation, so the docfx gulpfile is imported into ours so that we can replicate
the `~/_docfx_themes/rezolver/docfx.vendor.*` files, but replacing the core bootstrap with ours.

If the gulpfile of the default docfx template changes, we can just mimic those changes in ours after updating and re-exporting
the template.  The most likely thing that will have changed will be the `docfx.css` file - which contains all the additional styles
which the docfx team add in order to make the site look like a docfx site - and which we do not modify.

> [!TIP]
> The docfx gulpfile relies on bower packages and npm packages - therefore the `bower.json` and `package.json` files which 
> describe those dependencies (found, again, under the default theme's folder in the docfx repo at 
> https://github.com/dotnet/docfx/tree/dev/src/docfx.website.themes) need to be imported into this site's own 
> bower and npm package files, so we have all the same build-time components to replicate their build.

## 2 - docfx content files (~/api/ ~/articles/ etc)

We relocated all the docfx project content (`md`, `yml` and image files in the `~/api`, `~/articles` and `~/images` folders)
into this single subfolder in the root of the project in order to keep it clean.

The default folders might still be there because the docfx package adds them every time we upgrade the  package, 
but the root docfx.json file has been customised not to use them.

Also - all our MSBuild publish profiles import the `Properties/PublishExclude.props` file to ensure all the build-time docfx
content which is typically marked as 'content' by the web project template is not included in the publish - including the xref 
zip files which balloon the release to over 20Mb.

This file looks like this:

[!code-xml[PublishExclude.props](../../../properties/PublishExclude.props)]

