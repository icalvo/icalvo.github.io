# SWGen

SWGen is yet another static website generator. 

The main concept of SWGen is to be able to **use a typed language for both the metadata and the front-end generation logic**.

Therefore, we use Razor as the templating engine, where you can have autocompletion and full C# power. The templates themselves can define its own front matter and the metadata is also written in C#.

Future plans include using source generators to generate identifiers for each file and document so that we can reference them statically in the templates and the compilation will fail if the file is not found.

SWGen draws many ideas from [Fornax](https://github.com/ionide/Fornax) which is an F# static site generator where you use an F# DSL to express your layouts. However, it had several drawbacks:
* Metadata is still a plain text front matter section.
* The constant part of layouts are not expressed in HTML/CSS but a specific DSL.

As stated before, the only templating engine that I know that let you do have typed metadata and typed FE generation logic is Razor, which in turn requires C#.

So I basically ported to C# a good portion of Fornax and adapted it in order to use Razor.

When outside the ASP.NET world you got to use another Razor engine library. [RazorLight](https://github.com/toddams/RazorLight) came to the rescue.

# `SWGen.Razor`

Even with the conceptual plan of using Razor, the core ideas of a static website generator are very independent of a concrete engine, I've been able to move Razor-related generation logic into a separate library.

