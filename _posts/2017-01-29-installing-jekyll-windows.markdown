---
layout: post
title: "Installing Jekyll in Windows"
date: 2017-01-29
comments: true
categories: [jekyll,ruby]
---

The best least failure-prone way I've found is by using RubyInstaller builds. Go to http://rubyinstaller.org/downloads/ to get the latest Ruby+Devkit (be careful to choose well your architecture).

    gem install jekyll bundler

If you have to create a new Jekyll app:

    jekyll new appdir
    cd appdir

If you already got one:



Now we will edit the Gemfile to include wdm. Add this:

    gem 'wdm', '>= 0.1.0' if Gem.win_platform?

Finally:

    bundle install
    bundle exec jekyll serve