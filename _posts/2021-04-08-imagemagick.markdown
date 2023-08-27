---
layout: post
title: "Generating thumbnails for a music score"
date: 2021-04-08
comments: true
tags: [thumbnail,music,score,pdf]
categories: [software]
---
At the beginning of this year I started to prepare my [new showcase of musical works]({% link music/index.markdown %}). After some iterations I thought it would be a great idea to create backgrounds for each work, based on its score sheet. But, being a developer (that's synonym for lazy), I didn't want to prepare those thumbnails manually. So I wondered if there was some way to take a PDF file, cut an specific part of the first page, and apply a number of transformations to get a beautiful background that didn't contrast a lot with the text and elements that I would place on top of that.

In the beginning I imagined that I would need some tool for extracting an image from that PDF and a different one that allowed me to do the graphical transformations. I was not exactly wrong on that, although I finally solved the problem with call to a single command line tool. A bit of a paradox? Not so much.

The tool that everyone of you should know for doing programmatic digital image processing is [ImageMagick](https://imagemagick.org/). This amazing tools allow you to take any number of graphical inputs (or even creating empty canvases) and transform them in any way you could imagine. The aforementioned paradox is explained because, in order to use a PDF as input, you need a different software already installed, the legendary [Ghostscript](https://www.ghostscript.com/).

ImageMagick works by applying operations in order. You can do tons of transformations, add layers and combine them, etc.

The following is the call I do to generate my thumbnails:

```powershell
magick `
  "$input" `
  -density 300 `
  -background white ` # PDF image comes with transparent background, so we set a white background, and...
  -alpha remove ` # ...we remove the alpha channel.
  -distort SRT '-5' ` # rotate 5 degrees counter-clockwise
  -resize 400 ` # resize to 400px wide
  -crop 340x340+10+10 +repage ` # crop a 340x340 rectangle, offset 10px down and 10px left
  -background gray ` # set a gray background for the...
  -vignette 0x100+0+0 ` # vignette effect (obscure image borders creating a round shadow)
  -fill "rgb(255,240,100)" -colorize 20 ` # colorize with a yellowish tone
  -channel RGB -function polynomial 0.6,0.4 ` # Reduce contrast by applying a transformation function
  -attenuate 0.3 +noise Gaussian ` # add a bit of noise to add texture
  "$output"
```
