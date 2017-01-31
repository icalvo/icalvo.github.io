---
layout: post
title: "Installing Jekyll in Windows"
date: 2017-01-29
comments: true
categories: [jekyll,ruby]
---

The best least failure-prone way I've found is by using RubyInstaller builds. Go to http://rubyinstaller.org/downloads/ to get the latest Ruby (be careful to choose well your architecture) and Ruby DevKit.

Ruby installs itself, but the DevKit just uncompresses to some folder and you have to manually install it.

Go to the uncompressed DevKit folder and type:

    ruby dk.rb init

This generates a config.yml file, in which you will put the directory of your ruby installation in this way:

```yaml
---
- C:/tools/ruby23
```

Then execute:

    ruby dk.rb install

That will install the DevKit, which will in turn enable the installation of the wdm gem later on.

    gem install jekyll bundler
    jekyll new appdir
    cd appdir

Now we will edit the Gemfile to include wdm. Add this:

    gem 'wdm', '>= 0.1.0' if Gem.win_platform?

Finally:

    bundle install
    bundle exec jekyll serve