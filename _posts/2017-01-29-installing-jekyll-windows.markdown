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

Finally:

    bundle install
    bundle exec jekyll serve


2020-04-24: Removed update of `Gemfile` to add `wdm`, `jekyll new` adds that already.